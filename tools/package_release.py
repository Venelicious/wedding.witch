"""Create deterministic releases, excluding game files, profiles and credentials."""
from pathlib import Path
import hashlib
import json
import zipfile

ROOT = Path(__file__).resolve().parents[1]
VERSION = "0.3.6"

def archive(path, files):
    with zipfile.ZipFile(path, "w", zipfile.ZIP_DEFLATED, compresslevel=9) as out:
        for name, data in sorted(files):
            info = zipfile.ZipInfo(name, (2026, 10, 2, 0, 0, 0))
            info.compress_type = zipfile.ZIP_DEFLATED
            info.external_attr = 0o100644 << 16
            out.writestr(info, data)

def main():
    output = ROOT / "release"
    output.mkdir(exist_ok=True)
    world = [(str(p.relative_to(ROOT / "apworld")).replace("\\", "/"), p.read_bytes())
             for p in (ROOT / "apworld" / "wedding_witch").rglob("*")
             if p.is_file() and p.suffix in {".py", ".md", ".json"} and "__pycache__" not in p.parts]
    archive(output / "wedding_witch.apworld", world)
    files = []
    for name in ["Install.cmd", "Install.ps1", "README.md", "WeddingWitch.yaml",
                 "docs/installation-de.md", "docs/installation-en.md", "THIRD-PARTY-NOTICES.md"]:
        data = (ROOT / name).read_bytes()
        # Windows PowerShell 5.1 needs BOM to read German UTF-8 source correctly.
        if name.endswith(".ps1") and not data.startswith(b"\xef\xbb\xbf"):
            data = b"\xef\xbb\xbf" + data
        files.append((name, data))
    files.append(("WeddingWitchLauncher.exe", (ROOT / "launcher/WeddingWitchLauncher.exe").read_bytes()))
    files.append(("wedding_witch.apworld", (output / "wedding_witch.apworld").read_bytes()))
    for name in ["WeddingWitchArchipelago.dll", "Archipelago.MultiClient.Net.dll", "Newtonsoft.Json.dll"]:
        files.append(("plugins/" + name, (ROOT / "src/bin/Release" / name).read_bytes()))
    archive(output / f"WeddingWitch-AP-{VERSION}-Windows.zip", files)
    (output / "WeddingWitch.yaml").write_bytes((ROOT / "WeddingWitch.yaml").read_bytes())
    manifest = {name: {"size": (output / name).stat().st_size,
                      "sha256": hashlib.sha256((output / name).read_bytes()).hexdigest()}
                for name in [f"WeddingWitch-AP-{VERSION}-Windows.zip", "wedding_witch.apworld", "WeddingWitch.yaml"]}
    world_version = json.loads((ROOT / "apworld/wedding_witch/archipelago.json").read_text())["world_version"]
    (output / "manifest.json").write_text(json.dumps({"version": VERSION, "world_version": world_version, "artifacts": manifest}, indent=2)+"\n")
    print(json.dumps(manifest))

if __name__ == "__main__":
    main()
