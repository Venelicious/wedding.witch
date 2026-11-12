# Repository-Prüfung – wedding.witch

Datum: 13.11.2026. Geprüfter Ausgangscommit: `02ab0796b6076360992969a733fada9abbdfd572`.

## Belegte Feststellungen

- Quellstand und Manifestversion 0.5.2 passen zur README.
- Mod-Build benötigt nicht versionierte lokale Spiel-/BepInEx-Referenzen.
- PopTracker enthält ausgewählte originale UI-Sprites; die pauschale Asset-Aussage wird in der README präzisiert.
- APWorld hat einen eigenen Lizenztext; keine pauschale Neulizenzierung des Mods.

## Tatsächlich ausgeführte Prüfungen

- 12 ausgewählte JSON-Dateien mit Python `json.loads` geparst: erfolgreich.
- 12 ausgewählte Python-Dateien mit `ast.parse` syntaktisch geprüft: erfolgreich; kein Projektcode ausgeführt.
- Git-Baum, Remote-Zuordnung, Defaultbranch `main`, Ausgangs-HEAD und sauberer Status geprüft; keine AGENTS.md im erfassten Repository-Baum vorhanden.
- Nur ausgewählte Textdateien in einem partiellen Sparse-Klon gelesen; vollständigen Dateibaum über Git/GitHub-Metadaten abgeglichen. Binärdateien und Archive nicht bezogen oder entpackt.

## Grenzen und weiterer Einstieg

Build-, Start-, Installations- und Testbefehle der README sind dokumentierte
Entwicklungswege, soweit dort nicht ausdrücklich anders angegeben. Sie wurden
bei dieser Bestandsaufnahme nicht ausgeführt. Keine Spiel-, Foundry-, Netzwerk-,
Browser- oder Wiederherstellungsabnahme, keine Produktionsänderung.
Die Änderungen betreffen ausschließlich Dokumentation; `git diff --check`
wird vor dem Commit als Formatprüfung ausgeführt.
