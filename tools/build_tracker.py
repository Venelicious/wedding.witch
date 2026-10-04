"""Build our self-contained PopTracker pack from the APWorld's public contract.

Requires Pillow for the original diagram/tile artwork. Game sprites are never
read. The user's existing charm is copied unchanged.
"""
import hashlib
import argparse
import json
import runpy
import shutil
import zipfile
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[1]
PACK = ROOT / "poptracker"
WORLD = ROOT / "apworld/wedding_witch"
C = runpy.run_path(str(WORLD / "constants.py"))
A = runpy.run_path(str(WORLD / "achievements.py"))["ACHIEVEMENTS"]
BASE = C["BASE_ID"]
BG, PANEL, INK, MUTED, GOLD = "#101626", "#1d2439", "#f5f0df", "#a5abc5", "#e7bd73"
COLORS = ("#e997b5", "#93bfec", "#b997e9", "#91ccad", "#eeb389", "#e9a7d6")
RENDER_ARTWORK = False


def dump(path, value):
    target = PACK / path
    target.parent.mkdir(parents=True, exist_ok=True)
    target.write_text(json.dumps(value, ensure_ascii=False, indent=2) + "\n", encoding="utf-8", newline="\n")


def font(size):
    # Explicit Pillow-provided font works identically on Windows and CI Linux.
    return ImageFont.load_default(size=size)


def canvas(size):
    image = Image.new("RGB", size, BG)
    return image, ImageDraw.Draw(image)


def text(draw, xy, value, size=16, fill=INK, anchor=None):
    draw.text(xy, value, font=font(size), fill=fill, anchor=anchor)


def tile(filename, label, color=GOLD):
    if not RENDER_ARTWORK:
        if not (PACK / "images" / filename).is_file():
            raise FileNotFoundError(f"Missing artwork {filename}; run with --render-artwork")
        return
    image, draw = canvas((80, 80))
    draw.rounded_rectangle((3, 3, 76, 76), radius=15, fill=PANEL, outline=color, width=2)
    draw.line((19, 18, 60, 18), fill=color, width=2)
    text(draw, (40, 39), label, 22, color, "mm")
    save_artwork(image, filename)


def save_artwork(image, filename):
    target = PACK / "images" / filename
    if RENDER_ARTWORK:
        image.save(target)
    elif not target.is_file():
        raise FileNotFoundError(f"Missing artwork {filename}; run with --render-artwork")


def lua(value):
    if isinstance(value, dict):
        return "{\n" + "\n".join("[" + lua(k) + "] = " + lua(v) + "," for k, v in value.items()) + "\n}"
    if isinstance(value, (list, tuple)):
        return "{" + ", ".join(lua(v) for v in value) + "}"
    if value is None:
        return "nil"
    if isinstance(value, bool):
        return "true" if value else "false"
    if isinstance(value, str):
        return json.dumps(value, ensure_ascii=False)
    return str(value)


def build():
    (PACK / "images").mkdir(parents=True, exist_ok=True)
    shutil.copyfile(ROOT / "src/res/achievement-check.png", PACK / "images/charm.png")
    items, item_map, location_map, roots = [], {}, {}, []
    abbreviations = ("HAT", "ORB", "GLV", "RNG", "CLK", "MIR", "SHO", "NEC", "EAR", "OPT", "CAP", "BRC", "BOK", "CAT", "BAG", "EYE", "DOL")
    upgrade_codes = []
    for i, ((key, label, cap), short) in enumerate(zip(C["UPGRADES"], abbreviations)):
        code = key.lower()
        upgrade_codes.append(code)
        filename = code + ".png"
        tile(filename, short)
        items.append({"name": label + f" (AP ranks, max {cap})", "type": "consumable",
                      "codes": code, "img": "images/" + filename, "max_quantity": cap,
                      "overlay_font_size": 18, "overlay_background": "#dd101626"})
        item_map[BASE + 2000 + i] = {"code": code, "kind": "consumable", "cap": cap}
    for i, (difficulty, label) in enumerate((("hard", "Hard Wedding"), ("nightmare", "Nightmare Wedding"))):
        tile(difficulty + ".png", "HARD" if i == 0 else "NGHT", "#e997b5")
        items.append({"name": label, "type": "toggle", "codes": difficulty, "img": "images/" + difficulty + ".png"})
        item_map[BASE + 2050 + i] = {"code": difficulty, "kind": "toggle"}
    exp_codes, exp_by_name = [], {}
    for i, (exp, label, color) in enumerate(zip(C["EXP_TYPES"], ("BIG", "SML", "COR", "BST", "MUS", "HIP"), COLORS)):
        code = "exp_" + exp.lower()
        exp_codes.append(code)
        exp_by_name[exp] = code
        tile(code + ".png", label, color)
        items.append({"name": "EXP Unlock: " + exp, "type": "toggle", "codes": code,
                      "img": "images/" + code + ".png", "disabled_img": "images/charm.png",
                      "disabled_img_mods": "none", "initial_active_state": exp == "Beast"})
        item_map[BASE + 2100 + i] = {"code": code, "kind": "toggle"}
    for filename, short in (("status.png", "LINK"), ("progress.png", "CHK"), ("goal.png", "GOAL")):
        tile(filename, short, "#91ccad")
    dump("items/items.json", items)

    def child(root, name, entries, map_name, x, y, access=None, visibility=None):
        sections = []
        for ident, label, kind, meta, section_access, section_visibility in entries:
            sections.append({"name": label, "item_count": 1, "clear_as_group": False,
                             "access_rules": section_access or [], "visibility_rules": section_visibility or []})
            location_map[ident] = {"code": "@" + root["name"] + "/" + name + "/" + label,
                                   "kind": kind, **meta}
        root["children"].append({"name": name, "access_rules": access or [], "visibility_rules": visibility or [],
                                 "sections": sections, "map_locations": [{"map": map_name, "x": x, "y": y}]})

    runs, draw = canvas((660, 480))
    text(draw, (22, 20), "WEDDING WITCH  /  RUNS", 24, GOLD)
    text(draw, (22, 54), "Karten einzeln · Blumen kumulativ · Ziel laut AP-Seed", 15, MUTED)
    for name, d, count in C["DIFFICULTIES"]:
        x = 18 + 214 * d
        draw.rounded_rectangle((x, 91, x + 196, 343), 12, fill=PANEL)
        text(draw, (x + 16, 104), name.upper(), 19, GOLD)
        text(draw, (x + 16, 136), "KARTEN", 13, MUTED)
        root = {"name": name, "access_rules": ["$ww_difficulty|" + name.lower()],
                "visibility_rules": ["$ww_supported"], "children": []}
        for n in range(1, count + 1):
            mx, my = x + 40 + ((n - 1) % 3) * 57, 184 + ((n - 1) // 3) * 47
            text(draw, (mx, my + 16), str(n), 14, MUTED, "mt")
            child(root, "Map " + str(n), [(BASE + d * 100 + n, name + " Map " + str(n), "map", {}, [], [])], "runs", mx, my)
        text(draw, (x + 16, 313), "BLUMEN", 13, MUTED)
        entries = [(BASE + 5000 + d * 1000 + n, name + " Flower " + str(n), "flower", {"difficulty": d + 1, "number": n}, [], [f"$ww_flower|{d + 1}|{n}"]) for n in range(1, 18)]
        child(root, "Flowers", entries, "runs", x + 164, 326, visibility=[f"$ww_flower|{d + 1}|1"])
        roots.append(root)
    text(draw, (24, 356), "VERSCHIEDENE ERFOLGREICHE ENDINGS", 18, GOLD)
    text(draw, (24, 387), "Nur auf der Ziel-Schwierigkeit · normale Form zaehlt mit", 14, MUTED)
    ending_root = {"name": "Endings", "children": [], "visibility_rules": ["$ww_supported"]}
    for n in range(1, 8):
        x, y = 61 + (n - 1) * 86, 436
        child(ending_root, "Ending " + str(n), [(BASE + 8000 + n, "Transform Ending " + str(n), "ending", {"number": n}, [], [])], "runs", x, y,
              [f"$ww_ending|{n}"], [f"$ww_ending_visible|{n}"])
    roots.append(ending_root)
    save_artwork(runs, "runs.png")

    forms, draw = canvas((660, 480))
    text(draw, (22, 20), "TRANSFORMATIONEN", 24, GOLD)
    text(draw, (22, 57), "Freigabe links · volle Transformation als separater Check", 15, MUTED)
    root = {"name": "Transformations", "children": [], "visibility_rules": ["$ww_supported"]}
    for i, ((label, body), exp) in enumerate(zip(C["FORMS"], C["EXP_TYPES"])):
        x, y = 18 + (i % 3) * 214, 105 + (i // 3) * 166
        draw.rounded_rectangle((x, y, x + 196, y + 142), 12, fill=PANEL, outline=COLORS[i], width=1)
        text(draw, (x + 98, y + 20), label, 19, COLORS[i], "mt")
        text(draw, (x + 98, y + 52), "Vollstaendig verwandeln", 13, MUTED, "mt")
        child(root, label, [(BASE + 400 + body, "Full Transformation " + label, "form", {}, [], [])], "forms", x + 98, y + 98, [exp_by_name[exp]])
    text(draw, (22, 450), "Anhaenger = Tranktyp gesperrt · Farbsymbol = freigeschaltet", 14, MUTED)
    roots.append(root)
    save_artwork(forms, "forms.png")

    achievements, draw = canvas((760, 704))
    text(draw, (22, 20), "ERRUNGENSCHAFTEN  /  38 AP-CHECKS", 23, GOLD)
    text(draw, (22, 56), "Abgeschlossen laut Server · Teilzähler bleiben im Spiel", 14, MUTED)
    root = {"name": "Achievements", "children": [], "visibility_rules": ["$ww_supported"]}
    for i, (_, label, difficulty, exp) in enumerate(A, 1):
        x, y = 28 + ((i - 1) // 19) * 374, 97 + ((i - 1) % 19) * 31
        text(draw, (x + 23, y), label, 13, INK, "lm")
        access = (["hard" if difficulty == "Hard Wedding" else "nightmare"] if difficulty else []) + ([exp_by_name[exp]] if exp else [])
        child(root, label, [(BASE + 9000 + i, "Achievement: " + label, "achievement", {}, [], [])], "achievements", x, y, [",".join(access)] if access else [])
    text(draw, (22, 678), "Gruen: erreichbar laut AP-Logik · Rot: Freigabe fehlt · Grau: erledigt", 13, MUTED)
    roots.append(root)
    save_artwork(achievements, "achievements.png")
    dump("locations/locations.json", roots)
    dump("maps/maps.json", [{"name": name, "img": "images/" + name + ".png", "location_size": 22,
                              "location_border_thickness": 2, "location_shape": "diamond"}
                             for name in ("runs", "forms", "achievements")])

    def grid(rows, size=48):
        return {"type": "itemgrid", "rows": rows, "item_size": f"{size},{size}", "item_margin": "4,4", "h_alignment": "left"}

    def group(header, content):
        return {"type": "group", "header": header, "content": content}

    sidebar = {"type": "array", "orientation": "vertical", "content": [
        group("AP / Checks / Goal", grid([["ww_status", "ww_progress", "ww_goal"]], 72)),
        group("Schwierigkeiten", grid([["hard", "nightmare"]])),
        group("Tranktypen", grid([exp_codes[:3], exp_codes[3:]])),
        group("AP-Upgrades", grid([upgrade_codes[i:i + 4] for i in range(0, 17, 4)])),
        {"type": "text", "text": "Details: Maus über Symbol\nSkills: normaler Level-up-Pool"}]}
    tabs = {"type": "tabbed", "tabs": [{"title": title, "content": {"type": "map", "maps": [name]}}
                                        for title, name in (("Runs", "runs"), ("Formen", "forms"), ("Erfolge", "achievements"))]}
    board = {"type": "canvas", "width": 760, "height": 740, "margin": 0, "content": [tabs]}
    dump("layouts/tracker.json", {
        "tracker_default": {"type": "array", "orientation": "horizontal", "background": BG, "content": [sidebar, board]},
        "tracker_broadcast": {"type": "array", "orientation": "vertical", "background": BG, "content": [
            grid([["ww_status", "ww_progress", "ww_goal", "hard", "nightmare"]]),
            grid([exp_codes]), grid([upgrade_codes[i:i + 6] for i in range(0, 17, 6)])]}})
    data = {"items": item_map, "locations": location_map, "exp_codes": exp_codes,
            "exp_by_name": exp_by_name, "achievement_keys": [a[0] for a in A]}
    (PACK / "scripts/generated.lua").write_text("-- Generated by tools/build_tracker.py; IDs come from our APWorld.\nreturn " + lua(data) + "\n", encoding="utf-8", newline="\n")
    dump("contract.json", data)
    version = json.loads((PACK / "manifest.json").read_text(encoding="utf-8"))["package_version"]
    output = ROOT / "tracker-release" / f"WeddingWitch-PopTracker-{version}.zip"
    output.parent.mkdir(exist_ok=True)
    with zipfile.ZipFile(output, "w", zipfile.ZIP_DEFLATED, compresslevel=9) as archive:
        for path in sorted(PACK.rglob("*")):
            if path.is_file():
                info = zipfile.ZipInfo(path.relative_to(PACK).as_posix(), (2026, 1, 1, 0, 0, 0))
                info.create_system = 3
                info.compress_type = zipfile.ZIP_DEFLATED
                info.compress_level = 9
                info.external_attr = 0o100644 << 16
                content = path.read_text(encoding="utf-8").encode("utf-8") if path.suffix in (".json", ".lua", ".md") else path.read_bytes()
                archive.writestr(info, content)
    digest = hashlib.sha256(output.read_bytes()).hexdigest()
    print(f"{output.name}: {output.stat().st_size} bytes, SHA256 {digest}")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--render-artwork", action="store_true", help="Regenerate our own PNG diagrams/tiles (font rasterization can vary by OS).")
    RENDER_ARTWORK = parser.parse_args().render_artwork
    build()
