"""Contract and actual Lua callback tests. Run with Python + lupa + Pillow."""
import json
import hashlib
import runpy
import unittest
import zipfile
from pathlib import Path

from PIL import Image

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
            Tracker = {objects = {}, BulkUpdate = false, ActiveVariantUID = 'standard', layouts = {}}
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
            function Tracker:AddLayouts(filename)
                for name, layout in pairs(read_json(filename)) do self.layouts[name] = layout end
                return true
            end
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
                    SetOverlayColor = function(self, color) self.color = color end,
                    SetOverlayAlign = function() end,
                    SetOverlayBackground = function() end
                }
            end
            function ScriptHost:AddOnFrameHandler(name, fn) self.frames[name] = fn end
            function ScriptHost:AddOnLocationSectionChangedHandler(name, fn) self.changed[name] = fn end
            ImageReference = {FromPackRelativePath = function(_, path) return path end}
            AutoTracker = {GetConnectionState = function() return 3 end}
            Archipelago = {handlers = {}, PlayerNumber = 1, TeamNumber = 0, game = 'Wedding Witch', queries = {}}
            function Archipelago:GetPlayerGame(slot) return slot == self.PlayerNumber and self.game or 'Other Game' end
            function Archipelago:GetPlayerAlias(slot) return 'Player ' .. slot end
            function Archipelago:GetItemName(id, game) return game .. ' item ' .. id end
            function Archipelago:GetLocationName(id, game) return game .. ' location ' .. id end
            function Archipelago:SetNotify(keys) self.queries.notify = keys end
            function Archipelago:Get(keys) self.queries.get = keys end
            function Archipelago:AddClearHandler(_, fn) self.handlers.clear = fn end
            function Archipelago:AddItemHandler(_, fn) self.handlers.item = fn end
            function Archipelago:AddLocationHandler(_, fn) self.handlers.location = fn end
            function Archipelago:AddRetrievedHandler(_, fn) self.handlers.retrieved = fn end
            function Archipelago:AddSetReplyHandler(_, fn) self.handlers.set_reply = fn end
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
        variants = json.loads((PACK / "manifest.json").read_text())["variants"]
        self.assertEqual(set(variants), {"standard", "compact_horizontal", "compact_vertical", "items_only"})
        for variant in variants.values():
            self.assertEqual(variant["flags"], ["ap"])  # Never send checks/scouts from the tracker.

    def test_native_icon_sources_and_transparency(self):
        assets = json.loads((PACK / "assets.json").read_text(encoding="utf-8"))["assets"]
        expected = {item["code"] for item in CONTRACT["items"].values()} | {"progress", "goal"}
        self.assertEqual(set(assets), expected)
        for code, asset in assets.items():
            image_path = PACK / asset["image"]
            self.assertEqual(hashlib.sha256(image_path.read_bytes()).hexdigest(), asset["sha256"], code)
            with Image.open(image_path) as image:
                self.assertEqual(image.mode, "RGBA", code)
                self.assertEqual(image.size, (128, 128), code)
                alpha = image.getchannel("A")
                self.assertEqual(alpha.getpixel((0, 0)), 0, code)
                bounds = alpha.getbbox()
                self.assertIsNotNone(bounds, code)
                self.assertGreater(bounds[2] - bounds[0], 32, code)
                self.assertGreater(bounds[3] - bounds[1], 32, code)
        self.assertEqual(assets["hard"]["sprite"], "Wedding_Normal")
        self.assertEqual(assets["nightmare"]["sprite"], "Wedding_Hard")
        for code in ("hard", "nightmare"):
            self.assertIn("SelectDifficultyCanvas", assets[code]["source"])
        for exp in ("bigbreast", "smallbreast", "corruption", "beast", "muscle", "hip"):
            self.assertIn("SelectModeBehaviour", assets["exp_" + exp]["source"])

    def test_all_layout_references_and_achievement_maps(self):
        codes = {entry["code"] for entry in CONTRACT["items"].values()}
        codes |= {"ww_status", "ww_progress", "ww_goal", "ww_hint_summary", "ww_hint_previous", "ww_hint_next"}
        codes |= {f"ww_setting_{n}" for n in range(1, 9)} | {f"ww_hint_{n}" for n in range(1, 19)}
        common = json.loads((PACK / "layouts/tracker.json").read_text(encoding="utf-8"))
        maps = {m["name"] for m in json.loads((PACK / "maps/maps.json").read_text())}

        def walk(node, layouts):
            if isinstance(node, list):
                for child in node:
                    walk(child, layouts)
            elif isinstance(node, dict):
                if node.get("type") == "layout":
                    self.assertIn(node["key"], layouts)
                if node.get("type") == "itemgrid":
                    for row in node["rows"]:
                        for code in row:
                            self.assertIn(code, codes)
                if node.get("type") == "map":
                    self.assertTrue(set(node["maps"]) <= maps)
                for child in node.values():
                    if isinstance(child, (dict, list)):
                        walk(child, layouts)

        for variant in ("standard", "compact_horizontal", "compact_vertical", "items_only"):
            layouts = dict(common)
            if variant != "standard":
                filename = f"layouts/{variant}.json"
                layouts.update(json.loads((PACK / filename).read_text(encoding="utf-8")))
                self.lua.globals().Tracker.AddLayouts(self.lua.globals().Tracker, filename)
            walk(layouts, layouts)
            self.assertIn("settings_popup", layouts)
            self.assertIn("tracker_broadcast", layouts)
        locations = json.loads((PACK / "locations/locations.json").read_text(encoding="utf-8"))
        achievements = next(root for root in locations if root["name"] == "Achievements")
        self.assertEqual(len(achievements["children"]), 38)
        for i, child in enumerate(achievements["children"]):
            self.assertEqual([m["map"] for m in child["map_locations"]], ["achievements", f"achievements_{i // 19 + 1}"])

    def test_read_only_settings_follow_seed_and_saved_layout(self):
        self.handlers.clear(self.slot(transformEnd=7, difficulty="nightmare", starting_exp_type="Hip", flower_checks=[0, 11, 0]))
        self.assertEqual(self.ww.settings[2].overlay, "Ziel: 7 verschiedene Endings")
        self.assertIn("nightmare", self.ww.settings[3].overlay)
        self.assertIn("Hip", self.ww.settings[4].overlay)
        self.assertIn("Normal 0 / Hard 11 / Nightmare 0", self.ww.settings[5].overlay)
        for _, setting in self.ww.settings.items():
            self.assertIsNone(setting.OnLeftClickFunc)
            self.assertIsNone(setting.OnRightClickFunc)
        saved = self.ww.status.SaveFunc()
        self.handlers.clear(self.slot())
        self.ww.status.LoadFunc(self.ww.status, saved)
        self.assertIn("7", self.ww.settings[2].overlay)
        self.handlers.clear(self.slot(schema_version=3))
        self.assertIn("nicht unterstützt", self.ww.settings[1].overlay)

    def hint(self, **changes):
        entry = dict(finding_player=1, receiving_player=2, location=BASE + 9001,
                     item=111, found=False, status=30, entrance="")
        entry.update(changes)
        return entry

    def test_hint_subscription_cross_game_dedup_and_found_updates(self):
        self.handlers.clear(self.slot())
        ap = self.lua.globals().Archipelago
        key = "_read_hints_0_1"
        self.assertEqual(ap.queries.notify[1], key)
        self.assertEqual(ap.queries.get[1], key)
        hints = [self.hint(), self.hint(), self.hint(finding_player=2, receiving_player=1, location=222, item=BASE + 2000),
                 self.hint(finding_player=2, receiving_player=3), {"item": "invalid"}]
        self.handlers.retrieved("_read_hints_0_2", self.lua.table_from(hints, recursive=True))
        self.assertEqual(len(self.ww.hints.records), 0)
        self.handlers.retrieved(key, self.lua.table_from(hints, recursive=True))
        self.assertEqual(len(self.ww.hints.records), 2)
        self.assertIn("Other Game item 111 → Player 2", self.ww.hint_rows[1].overlay)
        self.assertIn(f"Wedding Witch location {BASE + 9001} · bei Player 1", self.ww.hint_rows[2].overlay)
        self.handlers.location(BASE + 9001)
        self.ww.refresh()
        self.assertIn("Gefunden", self.ww.hint_rows[6].overlay)  # Found goes after open.
        hints[2]["found"] = True
        self.handlers.set_reply(key, self.lua.table_from(hints, recursive=True), None)
        self.assertEqual(self.ww.hint_rows[3].overlay, "Gefunden")
        self.handlers.set_reply(key, self.lua.table_from([]), None)
        self.assertEqual(len(self.ww.hints.records), 0)
        self.assertIn("Noch keine", self.ww.hint_summary.overlay)

    def test_hint_paging_unicode_reset_and_stale_packets(self):
        self.handlers.clear(self.slot())
        hints = [self.hint(location=BASE + 9000 + n, entrance="ü" * 100 + "\nHidden") for n in range(1, 15)]
        self.handlers.retrieved("_read_hints_0_1", self.lua.table_from(hints, recursive=True))
        self.assertIn("1/3", self.ww.hint_summary.overlay)
        self.assertTrue(self.ww.hint_rows[3].overlay.endswith("…"))
        self.assertNotIn("\n", self.ww.hint_rows[3].overlay)
        self.ww.hint_next.OnLeftClickFunc()
        self.ww.hint_next.OnLeftClickFunc()
        self.ww.hint_next.OnLeftClickFunc()
        self.assertIn("3/3", self.ww.hint_summary.overlay)
        self.assertEqual(self.ww.hint_rows[7].overlay, "")
        self.ww.hint_previous.OnLeftClickFunc()
        self.assertIn("2/3", self.ww.hint_summary.overlay)
        self.lua.globals().Archipelago.PlayerNumber = 2
        self.lua.globals().Archipelago.TeamNumber = 1
        self.handlers.clear(self.slot())
        self.assertEqual(self.ww.hints.key, "_read_hints_1_2")
        self.assertEqual(len(self.ww.hints.records), 0)
        self.handlers.set_reply("_read_hints_0_1", self.lua.table_from(hints, recursive=True), None)
        self.assertEqual(len(self.ww.hints.records), 0)
        self.handlers.clear(self.slot(schema_version=3))
        self.assertIsNone(self.ww.hints.key)

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
