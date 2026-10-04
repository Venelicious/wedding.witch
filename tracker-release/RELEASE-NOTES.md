# Wedding Witch PopTracker 0.2.1

Originalgrafiken aus Wedding Witch ersetzen die bisherigen Kürzelsymbole. Für unsere APWorld 0.5.0–0.5.2 (Schema 4, 80 Checks), PopTracker 0.35.4+. Der Spielmod und bestehende Seeds bleiben unverändert.

- Alle 17 passiven Upgrades zeigen ihre Originalgrafik aus dem Shop; die empfangenen AP-Stufen bleiben als Zähler sichtbar.
- Alle sechs freigeschalteten Transformationstypen zeigen das Originalsymbol aus dem Spielmenü. Gesperrte Typen behalten den sechsfarbigen Anhänger.
- Hard Wedding und Nightmare Wedding verwenden die Symbole ihrer tatsächlichen Schwierigkeitsknöpfe. Die interne Sprite-Benennung wird anhand der UI-Referenzen zugeordnet.
- Das native Errungenschaftssymbol und die Story-Blume ersetzen die Check-/Goal-Kürzel. Die Übersichten für Runs, Formen und Errungenschaften verwenden ebenfalls die passenden Originalsymbole.
- 27 ausgewählte UI-Grafiken mit transparenten Rändern und erhaltenen Proportionen. Quellen und Prüfsummen stehen in assets.json; ein Extraktionswerkzeug erlaubt die Wiederholung mit einer lokalen Spielinstallation. Keine Spiel-DLLs, Asset-Bundles oder Charakterillustrationen im Pack.
- Vier Ansichten, Seed-Einstellungen, AP-Hinweise, Item-/Check-Synchronisation und Broadcast bleiben erhalten.

Installation: Die alte Pack-ZIP aus PopTrackers packs-Ordner entfernen und WeddingWitch-PopTracker-0.2.1.zip unverändert dort ablegen. Das Pack Wedding Witch AP neu laden. Kein neuer AP-Seed erforderlich.

Geprüft: elf Tracker-Tests einschließlich Bildprüfsummen, Transparenz, vollständiger Originalsymbol-Zuordnung und AP-Lua-Callbacks. Der native PopTracker 0.35.4 lädt und rendert alle vier Varianten sowie Hinweis- und Einstellungsansicht im automatisierten Test ohne Lua-/Referenzfehler. Originalsymbol- und Übersichtsbilder wurden visuell geprüft; wiederholte Builds ergeben dieselbe ZIP.

Originalgrafiken: Wedding Witch / CHOWBIE. Der bereitgestellte AP-Anhänger und die eigenen Übersichtsgrafiken behalten ihre jeweilige Herkunft.
