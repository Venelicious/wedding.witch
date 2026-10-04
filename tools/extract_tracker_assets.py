"""Export only the tracker UI icons from a local Wedding Witch installation.

Requires tools/requirements-tracker-assets.txt (UnityPy with ttgen). Game files are read only; only selected PNGs and their
provenance are exported, never assemblies, bundles, saves or other game artwork.
"""
import argparse
import hashlib
import json
import runpy
from pathlib import Path

import UnityPy
from PIL import Image, ImageOps
from UnityPy.classes.PPtr import PPtr
from UnityPy.helpers.TypeTreeGenerator import TypeTreeGenerator

ROOT = Path(__file__).resolve().parents[1]
C = runpy.run_path(str(ROOT / "apworld/wedding_witch/constants.py"))
EXP_FIELDS = {
    "BigBreast": "BigBreastSlot", "SmallBreast": "SmallBreastSlot",
    "Corruption": "CorruptionSlot", "Beast": "BeastSlot",
    "Muscle": "Muscle", "Hip": "HipSlot",
}


def deref(pointer, obj):
    return PPtr(**pointer, assetsfile=obj.assets_file).deref()


def class_name(obj):
    try:
        return obj.parse_monobehaviour_head().m_Script.deref().parse_as_object().m_ClassName
    except (AttributeError, KeyError, ValueError):
        return None


def button_icons(button):
    """Find the actual difficulty badge beneath a native button."""
    icons = []

    def walk(game_object):
        tree = game_object.parse_as_dict()
        for entry in tree["m_Component"]:
            component = deref(entry["component"], game_object)
            if component.type.name == "MonoBehaviour" and class_name(component) == "Image":
                data = component.parse_as_dict()
                if data["m_Sprite"]["m_PathID"]:
                    sprite = deref(data["m_Sprite"], component)
                    if sprite.parse_as_object().m_Name.startswith("Wedding_"):
                        icons.append(sprite)
            elif component.type.name == "RectTransform":
                for child in component.parse_as_dict()["m_Children"]:
                    transform = deref(child, component)
                    walk(deref(transform.parse_as_dict()["m_GameObject"], transform))

    walk(deref(button.parse_as_dict()["m_GameObject"], button))
    if len(icons) != 1:
        raise ValueError(f"Expected one native difficulty badge, found {len(icons)}")
    return icons[0]


def export(game_data, output, manifest, unity_version):
    if not (game_data / "data.unity3d").is_file() or not (game_data / "Managed").is_dir():
        raise ValueError("Pass the Wedding Witch_Data directory of the installed game")
    env = UnityPy.load(str(game_data / "data.unity3d"))
    generator = TypeTreeGenerator(unity_version)
    generator.load_local_dll_folder(str(game_data / "Managed"))
    env.typetree_generator = generator
    expected = {key for key, _, _ in C["UPGRADES"]}
    selected = {}
    for obj in env.objects:
        if obj.type.name != "MonoBehaviour":
            continue
        name = class_name(obj)
        if name == "UpgradeData":
            tree = obj.parse_as_dict()
            if tree["m_Name"] in expected:
                selected[tree["m_Name"].lower()] = (deref(tree["sprite"], obj),
                    "UpgradeData:" + tree["m_Name"] + ".sprite")
        elif name == "SelectModeBehaviour":
            tree = obj.parse_as_dict()
            for exp, field in EXP_FIELDS.items():
                image = deref(tree[field], obj)
                selected["exp_" + exp.lower()] = (deref(image.parse_as_dict()["m_Sprite"], image),
                    "SelectModeBehaviour." + field + ".m_Sprite")
        elif name == "SelectDifficultyCanvas":
            tree = obj.parse_as_dict()
            for code, field in (("hard", "hardMode"), ("nightmare", "nightmareMode")):
                selected[code] = (button_icons(deref(tree[field], obj)),
                    "SelectDifficultyCanvas." + field + ".Image.m_Sprite")
    for code, sprite_name in (("progress", "AchivementIcon"), ("goal", "Flowers_Icon")):
        matches = [obj for obj in env.objects if obj.type.name == "Sprite"
                   and obj.parse_as_object().m_Name == sprite_name]
        if len(matches) != 1:
            raise ValueError(f"Expected one sprite named {sprite_name}, found {len(matches)}")
        selected[code] = (matches[0], "Sprite:" + sprite_name)
    required = {key.lower() for key in expected} | {"exp_" + exp.lower() for exp in EXP_FIELDS}
    required |= {"hard", "nightmare", "progress", "goal"}
    if selected.keys() != required:
        raise ValueError(f"Incomplete tracker icons: missing {required - selected.keys()}")
    output.mkdir(parents=True, exist_ok=True)
    assets = {}
    for code, (obj, source) in sorted(selected.items()):
        sprite = obj.parse_as_object()
        original = sprite.image.convert("RGBA")
        bounds = original.getchannel("A").getbbox()
        if bounds is None:
            raise ValueError(f"Empty native sprite: {sprite.m_Name}")
        # Square transparent canvases preserve the sprite's proportions in item grids.
        icon = ImageOps.contain(original.crop(bounds), (112, 112), Image.Resampling.LANCZOS)
        image = Image.new("RGBA", (128, 128))
        image.alpha_composite(icon, ((128 - icon.width) // 2, (128 - icon.height) // 2))
        target = output / (code + ".png")
        image.save(target, optimize=True)
        assets[code] = {"image": "images/" + target.name, "sprite": sprite.m_Name,
            "source": source, "asset_file": obj.assets_file.name, "path_id": obj.path_id,
            "source_size": list(original.size), "size": list(image.size),
            "source_rgba_sha256": hashlib.sha256(original.tobytes()).hexdigest(),
            "sha256": hashlib.sha256(target.read_bytes()).hexdigest()}
    manifest.parent.mkdir(parents=True, exist_ok=True)
    manifest.write_text(json.dumps({"game": "Wedding Witch", "unity_version": unity_version,
        "extractor": "UnityPy " + UnityPy.__version__,
        "processing": "Native RGBA sprite, transparent bounds cropped; aspect preserved within 112x112 on a 128x128 transparent canvas.",
        "assets": assets}, ensure_ascii=False, indent=2) + "\n", encoding="utf-8", newline="\n")
    print(f"Exported {len(assets)} native tracker icons to {output}")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--game-data", type=Path, required=True, help="Installed Wedding Witch_Data directory")
    parser.add_argument("--output", type=Path, default=ROOT / "poptracker/images")
    parser.add_argument("--manifest", type=Path, default=ROOT / "poptracker/assets.json")
    parser.add_argument("--unity-version", default="6000.3.19f1")
    args = parser.parse_args()
    export(args.game_data.resolve(), args.output.resolve(), args.manifest.resolve(), args.unity_version)
