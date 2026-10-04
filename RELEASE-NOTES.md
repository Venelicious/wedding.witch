# Wedding Witch Archipelago 0.5.2

## Deutsch

Neu in 0.5.2: optionales DeathLink mit echtem AP-Senden und -Empfangen. DeathLink ist standardmäßig aus; `death_link: true` in der YAML aktiviert es für neue Seeds. Das F8-Verbindungsfenster zeigt AN/AUS und erlaubt das Umschalten für die aktuelle Verbindung, auch in bestehenden Seeds. Beim Wiederverbinden gilt erneut die Seed-Einstellung.

Ein eigener endgültiger Tod sendet DeathLink, nachdem natürliche Wiederbelebungen verbraucht sind. Ein empfangener DeathLink beendet den aktiven Run auch während Pause oder Levelauswahl und umgeht Wiederbelebungen und Unverwundbarkeit. Er wird nicht zurückgesendet. Duplikate werden unterdrückt; Meldungen ohne aktiven Run werden verworfen und gelangen nicht in einen späteren Run oder eine neue Verbindung.

APWorld, Mod, kommentierte YAML, Installationsanleitungen und Versionsanzeige sind auf 0.5.2 aktualisiert. Für die automatische YAML-Einstellung benötigt auch ap.dsatool.org APWorld 0.5.2. Der F8-Schalter benötigt keinen neuen Seed. Schema 4, Item-/Check-IDs und die 80 Checks bleiben unverändert; PopTracker-Pack 0.2.0 bleibt kompatibel. Bestehende Schema-2-/Schema-3-Seeds behalten ihre 142 Checks und AP-Skill-Stufen.

Die bisherigen Regeln bleiben erhalten: 13 Standard-Skills als normale Level-up-Auswahlen für den aktuellen Run, 73 permanente passive Stufen, zwei Schwierigkeitsfreigaben und fünf Tranktyp-Freigaben. Alle 38 Errungenschaften geben AP-Checks statt Münzen; gesperrte Transformationen zeigen den sechsfarbigen Anhänger. Das Blumenbudget bleibt 18 minus transformEnd. Steam-Start und Controller-Unterstützung sowie getrennte Original-Spielstände und AP-Profile bleiben erhalten.

Geprüft: Release-Build ohne Warnungen/Fehler, 27 Auswahl-, 33 Fortschritts-, 41 Errungenschafts-/Slot- und 13 DeathLink-Zustandsprüfungen; 14 Tests mit echtem Archipelago Core 0.6.7 und zehn Tracker-Tests. Zwei lokale AP-Netzwerktests mit jeweils 30 Prüfungen decken Senden/Empfangen, deaktiviertes DeathLink, Umschalten, doppelte Game-over-Aufrufe, Pause/Levelauswahl, Wiederbelebungen, Rücksendeschutz und erneutes Verbinden ab: einmal mit einem älteren Seed ohne DeathLink-Feld, einmal mit einem neu generierten Zwei-Spieler-Seed und aktivierter YAML-Option. Launcher-Selbsttest und Paket-/Hash-Prüfung bestanden. Die Netzwerktests verwenden den echten AP-Client und Mod-Hooks mit einem Modell der Spiel-API; eine interaktive DeathLink-Abnahme im nativen Spiel steht noch aus.

## English

New in 0.5.2: optional DeathLink with actual AP sending and receiving. DeathLink is off by default; set `death_link: true` in your YAML to enable it for new seeds. The F8 connection panel shows AN/AUS (ON/OFF) and lets you toggle participation for the current connection, including existing seeds. Reconnecting restores the seed setting.

A final local death sends DeathLink after native revives are exhausted. A received DeathLink ends the active run even during pause or level-up, bypassing revives and invulnerability. Received deaths are not sent back. Duplicates are suppressed; deaths outside an active run are discarded and cannot affect a later run or connection.

APWorld, mod, commented YAML, installation guides and version display are updated to 0.5.2. Automatic YAML configuration also requires ap.dsatool.org to use APWorld 0.5.2. The F8 toggle does not require a new seed. Schema 4, item/location IDs and the 80 checks are unchanged; PopTracker pack 0.2.0 remains compatible. Existing schema 2/3 seeds retain their original 142 checks and AP skill ranks.

Existing rules remain: 13 standard skills as native run-local level-up choices, 73 permanent passive ranks, two difficulty unlocks and five potion-type unlocks. All 38 achievements award AP checks instead of coins; locked transformations show the six-colour charm. Flower budget remains 18 minus transformEnd. Steam launching, controller support and separate original saves/AP profiles are retained.

Validated: a clean Release build; 27 selection, 33 progress, 41 achievement/slot and 13 DeathLink state assertions; 14 tests against real Archipelago Core 0.6.7 and ten tracker tests. Two local AP network tests with 30 assertions each cover sending/receiving, disabled participation, toggling, duplicate game-over callbacks, pause/level-up, revives, echo prevention and reconnecting: one with an older seed lacking the DeathLink field, one with a newly generated two-player seed and the YAML option enabled. Launcher self-test and archive/hash verification passed. Network tests use the real AP client and mod hooks with a native API model; interactive DeathLink acceptance in the native game remains pending.
