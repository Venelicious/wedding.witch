"""Verify published assets before GitHub Release upload."""
from pathlib import Path
import hashlib
import json
import zipfile

root = Path(__file__).resolve().parents[1]
release = root / 'release'
manifest = json.loads((release / 'manifest.json').read_text())
for name, spec in manifest['artifacts'].items():
    path = release / name
    assert path.stat().st_size == spec['size'], name
    assert hashlib.sha256(path.read_bytes()).hexdigest() == spec['sha256'], name
    if path.suffix in {'.zip', '.apworld'}:
        with zipfile.ZipFile(path) as archive:
            assert archive.testzip() is None
            names = archive.namelist()
            assert all(not n.startswith('/') and '..' not in Path(n).parts for n in names)
            assert all(not any(part in n.lower() for part in ['gamedata.es3', '.apsave', 'src/lib/', 'assembly-csharp', 'connection.cfg']) for n in names)
            if path.suffix == '.apworld':
                data = json.loads(archive.read('wedding_witch/archipelago.json'))
                assert data['world_version'] == manifest.get('world_version', manifest['version'])
                assert data['minimum_ap_version'] == '0.6.7'
            else:
                for required in ['WeddingWitchLauncher.exe', 'docs/installation-de.md', 'docs/installation-en.md', 'plugins/WeddingWitchArchipelago.dll']:
                    assert required in names, required
print('Release hashes, archives, manifest and required files verified.')
