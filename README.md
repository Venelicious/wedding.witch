# Wedding Witch Archipelago

Entwicklungsstand / Development build: **0.5.2** · Mod: **0.5.2** · APWorld: **0.5.2**

Archipelago-Integration für Wedding Witch (Windows/Steam). Solo und Multiworld mit 80 Checks einschließlich 38 Errungenschaften, dauerhaften passiven Stufen, Tranktyp- und Schwierigkeitsfreigaben. Standard-Skills sind normale Level-up-Auswahlen für den aktuellen Run.

English: Wedding Witch Archipelago for Windows/Steam, with solo and multiworld support, 80 checks and permanent passive, potion-type and difficulty unlocks. Standard skills use normal level-up choices for the current run. See the [English installation guide](docs/installation-en.md) for setup, connecting, updates and troubleshooting.

## Downloads

Aktuelles Testrelease / Current test release: [0.5.2](https://github.com/Venelicious/wedding.witch/releases/tag/v0.5.2).

- [Windows-Paket / Windows package](https://github.com/Venelicious/wedding.witch/releases/download/v0.5.2/WeddingWitch-AP-0.5.2-Windows.zip)
- [APWorld](https://github.com/Venelicious/wedding.witch/releases/download/v0.5.2/wedding_witch.apworld)
- [Beispiel-YAML / Example YAML](https://github.com/Venelicious/wedding.witch/releases/download/v0.5.2/WeddingWitch.yaml)

Das Release stellt WeddingWitch-AP-0.5.2-Windows.zip, wedding_witch.apworld und WeddingWitch.yaml bereit. Das Windows-Paket enthält den Mod, seine Client-Abhängigkeiten, Install.cmd und WeddingWitchLauncher.exe. Spiel-DLLs, Spielstände, Zugangsdaten und Test-Seeds gehören nicht zum Download.

Build 0.5.2: `release/WeddingWitch-AP-0.5.2-Windows.zip` enthält den neuen Client, Steam-Launcher und die Schema-4-APWorld 0.5.2. Für den 80er-Pool und Standard-Skills bei Level-ups einen neuen Seed mit dieser APWorld erzeugen. Bestehende Schema-2-/Schema-3-Seeds behalten ihre 142 Checks und AP-Skill-Stufen und bleiben mit dem Client spielbar.

**Installation / Setup:** [Deutsch](docs/installation-de.md) · [English](docs/installation-en.md). Launcher-Auswahl: Original oder Archipelago; Verbindung im Spiel mit F8.

**PopTracker:** [TrackPack 0.2.3 herunterladen](https://github.com/Venelicious/wedding.witch/releases/download/tracker-v0.2.3/WeddingWitch-PopTracker-0.2.3.zip) · [Anleitung](poptracker/README.md). Für PopTracker 0.35.4+ und unsere Schema-4-Seeds mit 80 Checks; mit Originalsymbolen aus dem Spiel, kompakten horizontalen/vertikalen Ansichten, reiner Itemansicht, Seed-Einstellungen und AP-Hinweisen. Synchronisiert AP-Items, Errungenschaften, Runs, Transformationen und das Ending-Ziel. ZIP unverändert in den `packs`-Ordner legen und über **AP** mit demselben Spielslot verbinden.

Beide Launcher-Modi starten über Steam, damit die gewohnte Steam-Controller-Konfiguration verfügbar ist. Der Loader wird über Startargumente pro Spielprozess ein- oder ausgeschaltet.

Im Transformationsmenü zeigen gesperrte AP-Tranktypen den sechsfarbigen Anhänger. Freigeschaltete Typen zeigen ihre Originalsymbole; der zusätzliche grüne AP-Rahmen entfällt. Die Anzeige aktualisiert sich auch bei einem empfangenen Item, während das Menü offen ist.

AP-Levelaufstiege verwenden wieder den normalen Pool: 13 Standard-Skills und die vom Spiel aktivierten Startzauber. Die 62 bisherigen Skill-Unlock-Items entfallen vollständig im neuen Seed. Trankbedingungen, maximale Skill-Stufen und Zauber-Slotgrenzen gelten weiterhin. Alte Seeds behalten den erweiterten Startzauber-Pool für ihre AP-Skill-Regeln.

## Optionen und Regeln

Die kommentierte YAML erklärt jede Einstellung und unterstützt gewichtete Auswahlmöglichkeiten. Progression Balancing steht auf normal (50); disabled entspricht 0, extreme 99. Allgemeine AP-Optionen umfassen lokale/externe Items, Startinventar, Hinweise und priorisierte/ausgeschlossene Checks. `start_inventory_from_pool` entnimmt Start-Items aus dem Pool und ersetzt sie durch Witch’s Orb, bei weiterhin 80 Checks.

- transformEnd: 1–7 verschiedene erfolgreiche Endings, normale Form eingeschlossen.
- difficulty: normal, hard oder nightmare für das Goal.
- starting_exp_type: auto (zufällig) oder einer der sechs Tranktypen.
- flower_checks_normal/hard/nightmare: -1 für automatische Verteilung, sonst kumulative Anzahl pro Schwierigkeit. Insgesamt müssen 18 minus transformEnd Blumenchecks entstehen.
- Alle 38 nativen Errungenschaftschecks bleiben erhalten. Mit den 62 Skill-Unlock-Items entfallen zusätzlich 62 Blumenchecks. Bei transformEnd 3 sind es 15 Blumenchecks; automatisch 4/5/6 auf Normal/Schwer/Albtraum.
- 18 Kartenchecks (5/6/7), sechs volle Transformationen und Ending-Meilensteine ergänzen Errungenschaften und Blumen auf genau 80 Checks.
- Errungenschaften zählen automatisch, sobald ihre native Bedingung in diesem Seed erfüllt ist. Der sechsfarbige AP-Anhänger ersetzt Goldsymbol und Betrag; diese Errungenschaften geben ausschließlich den AP-Check und keine Münzen. Bereits erreichte Erfolge aus anderen Seeds oder dem Original-Spielstand geben keine AP-Checks.
- 73 passive Stufen ersetzen den Shopkauf; zwei Items öffnen Schwer/Albtraum. Ein Tranktyp startet frei, die anderen fünf kommen über Checks.
- Die 13 Standard-Skills werden bei Level-ups gewählt und verbessert. Ihre Stufen gelten nur für den aktuellen Run. Meisterschaften bleiben vanilla.
- Tränke und natürliche Blumenboni behalten ihre native Stärke; gesperrte Tranktypen werden bei gleicher Trankstärke ersetzt.
- DeathLink ist optional und standardmäßig aus. Mit `death_link: true` in der YAML nehmen neue Seeds teil. Im F8-Fenster lässt sich DeathLink auch für bestehende Seeds für die aktuelle Verbindung umschalten; nach dem Wiederverbinden gilt wieder die Seed-Einstellung. Ein endgültiger eigener Tod wird gesendet; ein empfangener Tod beendet den aktiven Run ohne Wiederbelebung und ohne Rücksendung. Meldungen ohne aktiven Run werden verworfen.

AP-Profile werden getrennt vom Original-Save nach Seed, Team und Slot gespeichert. Ab Schema 3 werden auch Errungenschaftszustände dort gespeichert; Steam-Erfolge werden weiterhin ausgelöst, der normale Errungenschaftsspielstand bleibt erhalten. Updates erhalten den Fortschritt. Prototype-0.2.x- und upstream-Seeds benötigen ihren jeweiligen Client; IDs/Regeln sind nicht austauschbar.

## Entwicklung

Mod: .NET Framework 4.7.2, BepInEx 5 / Unity Mono. Spiel- und BepInEx-Referenzen lokal in src/lib bereitstellen; nicht veröffentlichen. `dotnet build src/WeddingWitchArchipelago.csproj -c Release`.

Launcher: `powershell -File launcher/Build.ps1` mit .NET Framework 4.x unter Windows. Keine zusätzliche Launcher-Runtime nötig.

APWorld: Python-Code unter apworld/wedding_witch; Archipelago Core 0.6.7. Tests: `python tests/test_world.py` in der echten AP-Umgebung und `dotnet run --project tests/ProgressTests.csproj`. Release-Paket: `python tools/package_release.py` nach Mod- und Launcher-Build.

Auswahltests: `dotnet run --project tests/SkillChoiceTests/SkillChoiceTests.csproj` prüft die Harmony-Hooks mit einem kleinen Modell der nativen API; Darstellung und Szeneninitialisierung müssen zusätzlich im Spiel geprüft werden.

Errungenschaften und Slot-Daten: `dotnet run --project tests/AchievementTests/AchievementTests.csproj` prüft die echten Hooks, den Profil-Speicher und die Schema-2-/Schema-3-/Schema-4-Verträge mit nachgebildeten nativen Zählern und Steam-Aufrufen.

DeathLink: `dotnet run --project tests/DeathLinkTests/DeathLinkTests.csproj` prüft Duplikate, Verbindungswechsel und Run-Grenzen. Mit zusätzlichen Argumenten `-- HOST PORT` verbindet der Test die Slots `YamlTest1` und `YamlTest2` eines eigenen Zwei-Spieler-Testservers und prüft das echte AP-Protokoll sowie die Mod-Hooks gegen ein Modell der Spiel-API. Für einen Seed mit aktiviertem DeathLink im zweiten Slot `-- HOST PORT on` verwenden. Vollständige native Spieltests werden dadurch nicht ersetzt.

## Herkunft

Grundgerüst: [chickentuna/WeddingWitchMod](https://github.com/chickentuna/WeddingWitchMod), Commit e25c120f5bada2058af73f955e1122c502e20c77. Die ursprüngliche Autorenangabe bleibt erhalten. Upstream enthält zum geprüften Stand keine LICENSE; diese Variante behauptet keine pauschale Neulizenzierung übernommener Dateien. Drittanbieter-Abhängigkeiten behalten ihre jeweiligen Lizenzen. Das Spiel selbst und seine Assets werden nicht mitgeliefert.

Diese Version ist ein Testrelease. Generator, Profile und Verbindung wurden geprüft; vollständige native Solo- und Multiworld-Abnahmen aller Endings stehen noch aus.
