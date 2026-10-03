# Wedding Witch Archipelago installieren

Windows-Paket 0.3.7 enthält Mod 0.3.5 und APWorld 0.3.6 sowie den im Spiel geprüften Launcher-Fix.

## Voraussetzungen

- Wedding Witch für Windows über Steam, App-ID 2529820, einmal normal gestartet.
- Windows 10/11 x64, Steam installiert und angemeldet.
- Das vollständige WeddingWitch-AP-0.3.7-Windows.zip herunterladen und entpacken.
- Für die Erstinstallation Internetzugriff auf GitHub. Der Installer lädt bei Bedarf BepInEx 5.4.23.5 x64 von dessen offiziellem Release und prüft SHA-256.

## Installation

1. Wedding Witch schließen. Eine laufende Runde wird vom Installer nicht beendet.
2. Install.cmd doppelklicken. Der Steam-Spielordner wird einschließlich zusätzlicher Steam-Bibliotheken gesucht. Bei fehlenden Schreibrechten das Fenster als Administrator starten.
3. Falls der Ordner nicht automatisch gefunden wird: PowerShell im entpackten Paket öffnen und `./Install.ps1 -GameDir 'D:\SteamLibrary\steamapps\common\Wedding Witch'` ausführen.
4. Der Installer legt WeddingWitchLauncher.exe im Spielordner an und installiert die Mod-Dateien in BepInEx/plugins/WeddingWitchCustom. Vorhandene AP-Profile und ersetzte Mod-Dateien werden gesichert; Spielstände werden nicht gelöscht.
5. Optional eine Verknüpfung auf WeddingWitchLauncher.exe auf dem Desktop erstellen.

## Original oder Archipelago starten

Steam muss angemeldet sein. WeddingWitchLauncher.exe öffnen und auswählen:

- **Original starten:** deaktiviert den gesamten BepInEx-Mod-Loader ausschließlich für diesen Spielprozess. Das Original startet ohne AP-Mod.
- **Archipelago starten:** startet das Spiel mit BepInEx und dem AP-Mod. F8 öffnet das Verbindungsfenster.

Die Auswahl verändert weder doorstop_config.ini noch deine Spielstände. Als Verknüpfungsargument sind `--original` und `--ap` möglich. Der normale Steam-Start lädt nach der Installation BepInEx; für Original den Launcher verwenden. Falls Steam noch geschlossen ist, öffnet der Launcher Steam. Sobald die Anmeldung fertig ist, den Startknopf erneut drücken.

## YAML und Verbindung

1. Auf ap.dsatool.org Wedding Witch auswählen und die YAML konfigurieren: Slotname, Anzahl verschiedener Enden (transformEnd 1–7), Zielschwierigkeit und Start-Tranktyp. Blumenchecks können automatisch verteilt oder passend zum Gesamtbudget eingestellt werden.
2. YAML herunterladen oder einem eigenen Lobby-Slot zuordnen. Nach der Seed-Erzeugung den Raum starten.
3. Im AP-Spiel F8 drücken. Host, Port, exakten Slotnamen und gegebenenfalls Raumpasswort aus dem Raum eintragen. Für lokale Tests die lokale Serveradresse benutzen.
4. Verbinden und einen neuen Run starten. Das AP-Profil ist an Seed, Team und Slot gebunden. Empfangene Standard-Skill-Stufen werden automatisch aktiv und bei neuen Runs wieder angewandt. Sie müssen nicht gekauft oder beim Level-up ausgewählt werden.
5. Ein Raum zeigt erst dann echten Spielfortschritt, wenn der Client verbunden ist und Checks sendet.

Die Website-Konfiguration steht erst nach Veröffentlichung der Portal-Integration bereit. Bis dahin WeddingWitch.yaml aus dem Release verwenden.

## APWorld für lokale Hosts

Spieler, die nur auf einem Portal-Raum spielen, benötigen keine lokale Archipelago-Installation. Wer selbst lokal Seeds erzeugt oder hostet, installiert Archipelago 0.6.7 und kopiert wedding_witch.apworld nach custom_worlds. Alternativ `./Install.ps1 -InstallWorld -ArchipelagoDir 'C:\ProgramData\Archipelago'` ausführen. Es darf nur eine Wedding-Witch-World aktiviert sein. Diese eigene Variante ist nicht mit upstream- oder Prototype-0.2.x-Seeds austauschbar.

## Regeln

142 Items und 142 adressierte Checks: 73 permanente passive Stufen, zwei Schwierigkeitsfreigaben, fünf Tranktyp-Freigaben und 62 Standard-Skill-Stufen. Ein Tranktyp ist von Anfang an freigeschaltet. Meisterschaften bleiben normale Spielinhalte.

Checks: fünf/sechs/sieben Kartenabschlüsse auf Normal/Schwer/Albtraum, sechs verschiedene volle Transformationen, ein Check je neuem erfolgreichen Ending bis zum Goal sowie kumulative Blumenchecks. Blumenbudget = 118 minus transformEnd. Eine volle Transformation benötigt sechs Trankpunkte derselben Art. Starke Tränke geben zwei Punkte und können auf Normal bei Elite-Missionen angeboten werden.

Das Goal zählt verschiedene Endings auf der gewählten Schwierigkeit: die normale Form und sechs Transformationsformen. Wiederholte Siege mit demselben Ending erhöhen den Siegzähler, aber nicht den Goal-Fortschritt. Transformation ohne Boss-Sieg erfüllt nur den Form-Check.

## Fehlerbehebung

- Kein F8-Fenster: im Launcher Archipelago starten; BepInEx/LogOutput.log prüfen. Dort muss Wedding Witch Archipelago 0.3.5 geladen sein.
- Original startet mit Mods: ignore_disable_switch in doorstop_config.ini muss false sein; Install.cmd setzt diesen Wert. Den Launcher verwenden, nicht den Steam-Startknopf.
- Verbindung scheitert: Host/Port/Slotname/Raumpasswort mit dem laufenden Raum abgleichen. Keine Cheats aktivieren.
- Andere APWorld bereits vorhanden: alte Variante in custom_worlds auslagern, nicht parallel laden.
- Fehlender Launcher oder DLL: das ganze Windows-ZIP entpacken, nicht nur einzelne Dateien herunterladen.
- Installation verweigert: laufendes Spiel schließen; bei geschützten Steam-Ordnern Schreibrechte prüfen.

## Updates und Deinstallation

Für Updates das Spiel schließen und Install.cmd aus dem neuen Paket ausführen. AP-Profile, Konfiguration und Original-Spielstände bleiben erhalten. Backups liegen im Spielordner unter BepInEx/WeddingWitch-backups.

Um nur AP zu entfernen, den Ordner BepInEx/plugins/WeddingWitchCustom und WeddingWitchLauncher.exe löschen. Für ein vollständig unmodifiziertes Spiel bei ausschließlich für AP installiertem BepInEx außerdem winhttp.dll entfernen; vorhandene andere Mods vorher berücksichtigen. AP-Profile kannst du für eine spätere Rückkehr behalten. Steam prüft bei Bedarf die Original-Spieldateien.
