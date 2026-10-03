# Wedding Witch Archipelago

Windows-Paket / Windows package: **0.3.6** · Mod & APWorld: **0.3.5**

Archipelago-Integration für Wedding Witch (Windows/Steam). Solo und Multiworld mit 142 Checks, dauerhaften passiven und Standard-Skill-Stufen, Tranktyp- und Schwierigkeitsfreigaben.

English: Wedding Witch Archipelago for Windows/Steam, with solo and multiworld support, 142 checks and permanent skill, passive, potion-type and difficulty unlocks. See the [English installation guide](docs/installation-en.md) for setup, connecting, updates and troubleshooting.

## Downloads

[Testrelease 0.3.6 / Test release 0.3.6](https://github.com/Venelicious/wedding.witch/releases/tag/v0.3.6)

- [Windows-Paket / Windows package](https://github.com/Venelicious/wedding.witch/releases/download/v0.3.6/WeddingWitch-AP-0.3.6-Windows.zip)
- [APWorld](https://github.com/Venelicious/wedding.witch/releases/download/v0.3.6/wedding_witch.apworld)
- [Beispiel-YAML / Example YAML](https://github.com/Venelicious/wedding.witch/releases/download/v0.3.6/WeddingWitch.yaml)

Das Release stellt WeddingWitch-AP-0.3.6-Windows.zip, wedding_witch.apworld und WeddingWitch.yaml bereit. Das Windows-Paket enthält den Mod, seine Client-Abhängigkeiten, Install.cmd und WeddingWitchLauncher.exe. Spiel-DLLs, Spielstände, Zugangsdaten und Test-Seeds gehören nicht zum Download.

**Installation / Setup:** [Deutsch](docs/installation-de.md) · [English](docs/installation-en.md). Launcher-Auswahl: Original oder Archipelago; Verbindung im Spiel mit F8.

## Optionen und Regeln

- transformEnd: 1–7 verschiedene erfolgreiche Endings, normale Form eingeschlossen.
- difficulty: normal, hard oder nightmare für das Goal.
- starting_exp_type: auto (zufällig) oder einer der sechs Tranktypen.
- flower_checks_normal/hard/nightmare: -1 für automatische Verteilung, sonst kumulative Anzahl pro Schwierigkeit. Insgesamt müssen 118 minus transformEnd Blumenchecks entstehen.
- 18 Kartenchecks (5/6/7), sechs volle Transformationen und Ending-Meilensteine ergänzen die Blumen auf genau 142 Checks.
- 73 passive Stufen ersetzen den Shopkauf; zwei Items öffnen Schwer/Albtraum. Ein Tranktyp startet frei, die anderen fünf kommen über Checks.
- 13 Standard-Skills mit 62 Stufen: jedes empfangene Item gewährt automatisch eine dauerhafte aktive Stufe bis zum nativen Maximum. Bei neuen Runs werden die besessenen Stufen erneut angewandt. Meisterschaften bleiben vanilla.
- Tränke und natürliche Blumenboni behalten ihre native Stärke; gesperrte Tranktypen werden bei gleicher Trankstärke ersetzt.

AP-Profile werden getrennt vom Original-Save nach Seed, Team und Slot gespeichert. Updates erhalten den Fortschritt. Prototype-0.2.x- und upstream-Seeds benötigen ihren jeweiligen Client; IDs/Regeln sind nicht austauschbar.

## Entwicklung

Mod: .NET Framework 4.7.2, BepInEx 5 / Unity Mono. Spiel- und BepInEx-Referenzen lokal in src/lib bereitstellen; nicht veröffentlichen. `dotnet build src/WeddingWitchArchipelago.csproj -c Release`.

Launcher: `powershell -File launcher/Build.ps1` mit .NET Framework 4.x unter Windows. Keine zusätzliche Launcher-Runtime nötig.

APWorld: Python-Code unter apworld/wedding_witch; Archipelago Core 0.6.7. Tests: `python tests/test_world.py` in der echten AP-Umgebung und `dotnet run --project tests/ProgressTests.csproj`. Release-Paket: `python tools/package_release.py` nach Mod- und Launcher-Build.

## Herkunft

Grundgerüst: [chickentuna/WeddingWitchMod](https://github.com/chickentuna/WeddingWitchMod), Commit e25c120f5bada2058af73f955e1122c502e20c77. Die ursprüngliche Autorenangabe bleibt erhalten. Upstream enthält zum geprüften Stand keine LICENSE; diese Variante behauptet keine pauschale Neulizenzierung übernommener Dateien. Drittanbieter-Abhängigkeiten behalten ihre jeweiligen Lizenzen. Das Spiel selbst und seine Assets werden nicht mitgeliefert.

Diese Version ist ein Testrelease. Generator, Profile und Verbindung wurden geprüft; vollständige native Solo- und Multiworld-Abnahmen aller Endings stehen noch aus.
