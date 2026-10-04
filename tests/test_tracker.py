"""Contract and actual Lua callback tests. Run with Python + lupa + Pillow."""
import json
import runpy
import unittest
import zipfile
from pathlib import Path

from lupa.lua54 import LuaRuntime

ROOT = Path(__file__).resolve().parents[1]
PACK = ROOT / "poptracker"
CONTRACT = json.loads((PACK / "contract.json").read_text(encoding="utf-8"))
BASE = 252982000


class TrackerTests(unittest.TestCase):
    def setUp(self):
        self.lua = LuaRuntime(unpack_returned_tuples=True)
        self.lua.globals().read_json = lambda filename: self.lua.table_from(
            json.loads((PACK / filename).read_text(encoding="utf-8")), recursive=True)
        self.lua.execute("""
            Tracker = {objects = {}, BulkUpdate = false}
            function Tracker:AddItems(filename)
                for _, entry in ipairs(read_json(filename)) do
                    self.objects[entry.codes] = {Active = entry.initial_active_state or false,
                        AcquiredCount = entry.initial_quantity or 0, MaxCount = entry.max_quantity}
                end
                return true
            end
            function Tracker:AddLocations(filename)
                local function add(node, path)
                    path = path == '' and node.name or path .. '/' .. node.name
                    for _, section in ipairs(node.sections or {}) do
                        self.objects['@' .. path .. '/' .. section.name] = {ChestCount = section.item_count,
                            AvailableChestCount = section.item_count}
                    end
                    for _, child in ipairs(node.children or {}) do add(child, path) end
                end
                for _, root in ipairs(read_json(filename)) do add(root, '') end
                return true
            end
            function Tracker:AddMaps(_) return true end
            function Tracker:AddLayouts(_) return true end
            function Tracker:FindObjectForCode(code) return self.objects[code] end
            function Tracker:ProviderCountForCode(code)
                local item = self.objects[code]
                if not item then return 0 end
                if item.Active then return 1 end
                return item.AcquiredCount or 0
            end
            ScriptHost = {frames = {}, changed = {}}
            function ScriptHost:CreateLuaItem()
                return {
                    state = {},
                    Set = function(self, k, v) self.state[k] = v end,
                    Get = function(self, k) return self.state[k] or '' end,
                    SetOverlay = function(self, text) self.overlay = text end,
                    SetOverlayFontSize = function() end,
                    SetOverlayAlign = function() end,
                    SetOverlayBackground = function() end
                }
            end
            function ScriptHost:AddOnFrameHandler(name, fn) self.frames[name] = fn end
            function ScriptHost:AddOnLocationSectionChangedHandler(name, fn) self.changed[name] = fn end
            ImageReference = {FromPackRelativePath = function(_, path) return path end}
            AutoTracker = {GetConnectionState = function() return 3 end}
            Archipelago = {handlers = {}, PlayerNumber = 1, game = 'Wedding Witch'}
            function Archipelago:GetPlayerGame(_) return self.game end
            function Archipelago:AddClearHandler(_, fn) self.handlers.clear = fn end
            function Archipelago:AddItemHandler(_, fn) self.handlers.item = fn end
            function Archipelago:AddLocationHandler(_, fn) self.handlers.location = fn end
        """)
        script_dir = str(PACK / "scripts").replace("\\", "/")
        self.lua.globals().script_dir = script_dir
        self.lua.execute("package.path = script_dir .. '/?.lua;' .. package.path")
        self.lua.execute((PACK / "scripts/init.lua").read_text(encoding="utf-8"))
        self.handlers = self.lua.globals().Archipelago.handlers
        self.ww = self.lua.globals().WW

    def slot(self, **changes):
        result = dict(schema_version=4, pool_size=80, skill_mode="level_up", integration_mode="unlock_custom",
                      transformEnd=3, difficulty="normal", starting_exp_type="Beast", flower_checks=[4, 5, 6],
                      map_counts=[5, 6, 7], achievement_checks=CONTRACT["achievement_keys"])
        result.update(changes)
        return self.lua.table_from(result, recursive=True)

    def item(self, code):
        return self.lua.globals().Tracker.objects[code]

    def enabled(self):
        return [int(ident) for ident, entry in self.ww.data.locations.items() if self.ww.enabled(entry)]

    def test_complete_contract_and_package(self):
        constants = runpy.run_path(str(ROOT / "apworld/wedding_witch/constants.py"))
        self.assertEqual(len(CONTRACT["items"]), 25)
        self.assertEqual(len(CONTRACT["locations"]), 120)
        for i, (key, _, cap) in enumerate(constants["UPGRADES"]):
            self.assertEqual(CONTRACT["items"][str(BASE + 2000 + i)], dict(code=key.lower(), kind="consumable", cap=cap))
        for ident, location in CONTRACT["locations"].items():
            self.assertIsNotNone(self.item(location["code"]), ident)
        version = json.loads((PACK / "manifest.json").read_text())["package_version"]
        with zipfile.ZipFile(ROOT / "tracker-release" / f"WeddingWitch-PopTracker-{version}.zip") as archive:
            expected = {p.relative_to(PACK).as_posix(): (p.read_text(encoding="utf-8").encode("utf-8")
                        if p.suffix in (".json", ".lua", ".md") else p.read_bytes())
                        for p in PACK.rglob("*") if p.is_file()}
            self.assertEqual(set(archive.namelist()), set(expected))
            self.assertEqual(archive.namelist(), sorted(expected))
            for path, data in expected.items():
                self.assertEqual(archive.read(path), data, path)
            self.assertEqual(archive.read("images/charm.png"), (ROOT / "src/res/achievement-check.png").read_bytes())
        flags = json.loads((PACK / "manifest.json").read_text())["variants"]["standard"]["flags"]
        self.assertEqual(flags, ["ap"])  # No apmanual: never send checks from the tracker.

    def test_all_goal_difficulty_and_extreme_flower_layouts(self):
        for goal in range(1, 8):
            for d in ("normal", "hard", "nightmare"):
                for flowers in ([18 - goal, 0, 0], [0, 18 - goal, 0], [0, 0, 18 - goal]):
                    self.handlers.clear(self.slot(transformEnd=goal, difficulty=d, flower_checks=flowers))
                    self.assertEqual(len(self.enabled()), 80)
                    self.assertEqual(self.ww.progress.overlay, "0/80")
                    self.assertEqual(self.ww.goal.overlay, f"0/{goal}")

    def test_replay_duplicate_caps_reset_and_starting_potion(self):
        self.handlers.clear(self.slot(starting_exp_type="Hip"))
        self.assertTrue(self.item("exp_hip").Active)
        self.assertFalse(self.item("exp_beast").Active)
        for index in range(8):
            self.handlers.item(index, BASE + 2000)
            self.handlers.item(index, BASE + 2000)
        self.assertEqual(self.item("upgrade_witchhat").AcquiredCount, 5)
        self.handlers.item(8, BASE + 2105)  # Starting potion replay stays a toggle.
        self.assertTrue(self.item("exp_hip").Active)
        self.handlers.item(9, BASE + 2051)
        self.assertTrue(self.lua.globals().ww_difficulty("nightmare"))
        self.assertFalse(self.lua.globals().ww_difficulty("hard"))
        self.handlers.location(BASE + 9001)
        self.handlers.location(BASE + 9001)
        self.handlers.location(BASE + 5005)  # Not part of this seed.
        self.handlers.location(999999)
        self.ww.refresh()
        self.assertEqual(self.ww.progress.overlay, "1/80")
        self.handlers.clear(self.slot(starting_exp_type="SmallBreast"))
        self.assertEqual(self.ww.progress.overlay, "0/80")
        self.assertEqual(self.item("upgrade_witchhat").AcquiredCount, 0)
        self.assertFalse(self.item("nightmare").Active)
        self.assertFalse(self.item("exp_hip").Active)
        self.assertTrue(self.item("exp_smallbreast").Active)
        self.handlers.item(0, BASE + 2000)
        self.assertEqual(self.item("upgrade_witchhat").AcquiredCount, 1)

    def test_ending_logic_and_all_checks_complete(self):
        self.handlers.clear(self.slot(difficulty="hard"))
        self.assertFalse(self.lua.globals().ww_ending("1"))
        self.handlers.item(0, BASE + 2050)
        self.assertTrue(self.lua.globals().ww_ending("1"))
        self.assertFalse(self.lua.globals().ww_ending("2"))
        self.handlers.item(1, BASE + 2100)
        self.assertTrue(self.lua.globals().ww_ending("2"))
        self.assertTrue(self.lua.globals().ww_ending("3"))
        self.assertFalse(self.lua.globals().ww_ending("4"))
        for ident in self.enabled():
            self.handlers.location(ident)
        self.ww.refresh()
        self.assertEqual(self.ww.progress.overlay, "80/80")
        self.assertEqual(self.ww.goal.overlay, "3/3")

    def test_invalid_seed_and_wrong_game(self):
        cases = [dict(schema_version=3), dict(pool_size=142), dict(skill_mode="archipelago"),
                 dict(flower_checks=[500, 0, 0]), dict(flower_checks=[4, 4, 4]),
                 dict(starting_exp_type="Unknown"), dict(difficulty="Unknown"),
                 dict(transformEnd=1.5), dict(map_counts=[5, 5, 5]), dict(achievement_checks=[])]
        for change in cases:
            self.handlers.clear(self.slot(**change))
            self.assertEqual(self.enabled(), [])
            self.assertEqual(self.ww.status.overlay, "ERROR")
            self.handlers.item(0, BASE + 2000)
            self.assertEqual(self.item("upgrade_witchhat").AcquiredCount, 0)
            self.assertFalse(self.lua.globals().Tracker.BulkUpdate)
        self.lua.globals().Archipelago.game = "Another Game"
        self.handlers.clear(self.slot())
        self.assertEqual(self.enabled(), [])

    def test_saved_layout_and_bulk_error_recovery(self):
        self.handlers.clear(self.slot(transformEnd=7, difficulty="nightmare", flower_checks=[0, 11, 0]))
        saved = self.ww.status.SaveFunc()
        self.handlers.clear(self.slot())
        self.ww.status.LoadFunc(self.ww.status, saved)
        self.assertEqual(len(self.enabled()), 80)
        self.assertEqual(self.ww.config.difficulty, "nightmare")
        self.assertEqual(self.ww.config.flower_checks[2], 11)
        with self.assertRaises(Exception):
            self.ww.bulk(self.lua.eval("function() error('test failure') end"))
        self.assertFalse(self.lua.globals().Tracker.BulkUpdate)


if __name__ == "__main__":
    unittest.main()
