# Wedding Witch Archipelago installieren

Windows-Paket 0.5.2 enthält Mod und APWorld 0.5.2, den Steam-Launcher, normale Standard-Skills bei Level-ups und 38 Errungenschaftschecks. Neue Seeds haben 80 Items und Checks.

## Voraussetzungen

- Wedding Witch für Windows über Steam, App-ID 2529820, einmal normal gestartet.
- Windows 10/11 x64, Steam installiert und angemeldet.
- Das vollständige WeddingWitch-AP-0.5.2-Windows.zip entpacken.
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

Der Launcher startet beide Modi über Steam und verwendet `--doorstop-enabled true` für AP beziehungsweise `--doorstop-enabled false` für Original. Damit steht auch beim Launcher-Start die Steam-Controller-Konfiguration zur Verfügung. Wenn ein Controller über den Steam-Startknopf funktioniert, aber mit dem bisherigen Launcher nicht, den aktualisierten Launcher verwenden.

## YAML und Verbindung

Die beiliegende YAML erklärt alle Optionen auf Deutsch. Ein Wert wie normal: 50 ist ein Auswahlgewicht; 0 deaktiviert die betreffende Auswahl. Ein fester Wert wie difficulty: normal ist weiterhin erlaubt. Progression Balancing ist mit normal (Einstellung 50) aktiv; disabled steht für 0 und extreme für 99. Startinventar aus dem Pool, Hinweise und Item-/Check-Verteilung stehen unter den allgemeinen AP-Optionen. Diese Konfiguration wirkt bei der nächsten Seed-Erzeugung.

1. Auf ap.dsatool.org Wedding Witch auswählen und die YAML konfigurieren: Slotname, Anzahl verschiedener Enden (transformEnd 1–7), Zielschwierigkeit und Start-Tranktyp. Blumenchecks können automatisch verteilt oder passend zum Gesamtbudget eingestellt werden.
2. YAML herunterladen oder einem eigenen Lobby-Slot zuordnen. Nach der Seed-Erzeugung den Raum starten.
3. Im AP-Spiel F8 drücken. Host, Port, exakten Slotnamen und gegebenenfalls Raumpasswort aus dem Raum eintragen. Für lokale Tests die lokale Serveradresse benutzen.
4. Verbinden und einen neuen Run starten. Das AP-Profil ist an Seed, Team und Slot gebunden. In neuen Schema-4-Seeds werden Standard-Skills normal beim Level-up gewählt und verbessert; ihre Stufen gelten für den aktuellen Run.
5. Ein Raum zeigt erst dann echten Spielfortschritt, wenn der Client verbunden ist und Checks sendet.

Seed-Erzeugung und Verbindung über ap.dsatool.org sind geprüft. Für den 80er-Pool und Standard-Skills bei Level-ups muss das Portal APWorld 0.5.2 verwenden und einen neuen Schema-4-Seed erzeugen. Bestehende Schema-2-/Schema-3-Seeds behalten ihre 142 Checks und AP-Skill-Stufen; der neue Client kann sie weiterhin spielen.

## DeathLink

DeathLink ist standardmäßig ausgeschaltet. Für neue Seeds `death_link: true` unter `Wedding Witch:` in der YAML setzen. Auch das Portal muss APWorld 0.5.2 verwenden, damit diese Option im Seed gespeichert wird.

Nach dem Verbinden zeigt das F8-Fenster **DeathLink: AN/AUS**. Der Knopf schaltet die Teilnahme für die aktuelle Verbindung um, auch bei bestehenden Seeds ohne DeathLink-Option. Beim Wiederverbinden gilt erneut der YAML-Wert des Seeds; bei älteren Seeds ist das AUS.

Ein eigener endgültiger Tod beendet den Run und sendet DeathLink an die anderen teilnehmenden Slots. Natürliche Wiederbelebungen werden vorher wie gewohnt verwendet. Ein empfangener DeathLink beendet einen aktiven Run sofort, auch während Pause oder Levelauswahl, und umgeht Wiederbelebungen und Unverwundbarkeit. Er wird nicht zurückgesendet. Meldungen ohne aktiven Run werden verworfen und nicht in den nächsten Run übernommen. DeathLink verändert weder Checks noch das Ending-Ziel.

## APWorld für lokale Hosts

Spieler, die nur auf einem Portal-Raum spielen, benötigen keine lokale Archipelago-Installation. Wer selbst lokal Seeds erzeugt oder hostet, installiert Archipelago 0.6.7 und kopiert wedding_witch.apworld nach custom_worlds. Alternativ `./Install.ps1 -InstallWorld -ArchipelagoDir 'C:\ProgramData\Archipelago'` ausführen. Es darf nur eine Wedding-Witch-World aktiviert sein. Diese eigene Variante ist nicht mit upstream- oder Prototype-0.2.x-Seeds austauschbar.

## Regeln

80 Items und 80 adressierte Checks: 73 permanente passive Stufen, zwei Schwierigkeitsfreigaben und fünf Tranktyp-Freigaben. Die 62 Skill-Unlock-Items sind entfernt; alle 13 Standard-Skills sind wieder normale Level-up-Auswahlen und gelten für den jeweiligen Run. Ein Tranktyp ist von Anfang an freigeschaltet. Meisterschaften bleiben normale Spielinhalte.

Im Transformationsmenü steht der sechsfarbige Anhänger für gesperrte AP-Tranktypen. Nur freigeschaltete Typen zeigen ihre Originalsymbole; der zusätzliche grüne AP-Rahmen entfällt. Ein empfangenes Freigabe-Item aktualisiert das offene Menü sofort.

In Schema-4-Seeds nutzt die Levelauswahl wieder den nativen Pool aus Standard-Skills und den vom Spiel aktivierten Startzaubern. Zauber mit Trankbedingungen benötigen weiterhin die entsprechenden Tränke; ausgebaute Skills und das native Zauber-Slotlimit begrenzen den verfügbaren Pool. Die vierte Auswahl durch das entsprechende passive Upgrade bleibt erhalten.

Checks: fünf/sechs/sieben Kartenabschlüsse auf Normal/Schwer/Albtraum, sechs verschiedene volle Transformationen, 38 Errungenschaften, ein Check je neuem erfolgreichen Ending bis zum Goal sowie kumulative Blumenchecks. Blumenbudget = 18 minus transformEnd. Bei drei Endings sind das 15 Blumenchecks, automatisch 4/5/6 pro Schwierigkeit. Eine volle Transformation benötigt sechs Trankpunkte derselben Art. Starke Tränke geben zwei Punkte und können auf Normal bei Elite-Missionen angeboten werden.

Eine Errungenschaft sendet automatisch genau einen Check, sobald die originale Spielbedingung im aktuellen Seed erfüllt ist. Der sechsfarbige AP-Anhänger ersetzt Goldsymbol und Betrag. Diese Errungenschaften geben keine Münzen; das Abholen bestätigt den bereits gesendeten Check. Zähler und abgeschlossene/abgeholte Zustände werden pro Seed/Team/Slot gespeichert. Originale Erfolge und andere Seeds zählen nicht mit. „100 Blumen“ zählt alle natürlich gesammelten Blumen, auch nachdem das kleinere AP-Blumenbudget ausgeschöpft ist. Die Statistik vor der Schwierigkeitsauswahl zeigt auch die erreichten Errungenschaftschecks.

Das Goal zählt verschiedene Endings auf der gewählten Schwierigkeit: die normale Form und sechs Transformationsformen. Wiederholte Siege mit demselben Ending erhöhen den Siegzähler, aber nicht den Goal-Fortschritt. Transformation ohne Boss-Sieg erfüllt nur den Form-Check.

## Fehlerbehebung

- Kein F8-Fenster: im Launcher Archipelago starten; BepInEx/LogOutput.log prüfen. Dort muss Wedding Witch Archipelago 0.5.2 geladen sein.
- Original startet mit Mods: den aktuellen Launcher und „Original starten“ verwenden. Der normale Steam-Start lädt nach der Installation den Mod-Loader.
- Weiterhin 142 Checks oder AP-Skill-Items: einen neuen Seed mit APWorld 0.5.2 erzeugen. Ein bestehender Seed lässt sich nicht nachträglich auf den 80er-Pool und normale Level-up-Skills umstellen.
- Verbindung scheitert: Host/Port/Slotname/Raumpasswort mit dem laufenden Raum abgleichen. Keine Cheats aktivieren.
- Andere APWorld bereits vorhanden: alte Variante in custom_worlds auslagern, nicht parallel laden.
- Fehlender Launcher oder DLL: das ganze Windows-ZIP entpacken, nicht nur einzelne Dateien herunterladen.
- Installation verweigert: laufendes Spiel schließen; bei geschützten Steam-Ordnern Schreibrechte prüfen.

## Updates und Deinstallation

Für Updates das Spiel schließen und Install.cmd aus dem neuen Paket ausführen. AP-Profile, Konfiguration und Original-Spielstände bleiben erhalten. Backups liegen im Spielordner unter BepInEx/WeddingWitch-backups.

Um nur AP zu entfernen, den Ordner BepInEx/plugins/WeddingWitchCustom und WeddingWitchLauncher.exe löschen. Für ein vollständig unmodifiziertes Spiel bei ausschließlich für AP installiertem BepInEx außerdem winhttp.dll entfernen; vorhandene andere Mods vorher berücksichtigen. AP-Profile kannst du für eine spätere Rückkehr behalten. Steam prüft bei Bedarf die Original-Spieldateien.
