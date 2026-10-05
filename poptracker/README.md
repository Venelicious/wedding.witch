# Wedding Witch AP · PopTracker 0.2.2

Für Venelicious/wedding.witch, APWorld **0.5.0–0.5.2, Schema 4**, mit 80 Checks.
Benötigt **PopTracker 0.35.4 oder neuer**. Alte 142-Check-Seeds und andere Wedding-Witch-APWorlds werden mit einer Fehlermeldung abgewiesen.

## Installation und Verbindung

1. `WeddingWitch-PopTracker-0.2.2.zip` unverändert in PopTrackers `packs`-Ordner kopieren. Nicht entpacken. Die alte Pack-ZIP aus diesem Ordner entfernen.
2. PopTracker starten und **Wedding Witch AP** mit einer der unten beschriebenen Ansichten laden.
3. Oben die **AP**-Verbindung aktivieren. Dieselbe Serveradresse, denselben Slotnamen und gegebenenfalls dasselbe Raumpasswort wie im Mod eintragen. Die Verbindung funktioniert parallel zum Spiel.
4. Sobald die Verbindung steht, übernimmt das Pack Ziel, Schwierigkeit, Start-Tranktyp und Blumenverteilung aus den Slotdaten. Empfangene Items und abgeschlossene Checks werden synchronisiert.

Das Pack hat eine eigene Versionsnummer; der Mod bleibt unverändert. Updates des Packs ersetzen nur die Tracker-ZIP.

## Ansichten und Seed-Einstellungen

- **Kompakt · Horizontal:** Items links, Checks rechts; kleinere Errungenschaftsseiten mit jeweils 19 Checks.
- **Kompakt · Vertikal:** Checks oben, Items darunter; dieselben Checkseiten wie horizontal.
- **Nur Items:** kompakte Anzeige der Freigaben, Upgrade-Stufen und Fortschrittswerte.
- **Vollständig · 80 Checks:** ursprüngliche Ansicht mit allen 38 Errungenschaften auf einer Seite.

Über PopTrackers **Pack-Einstellungen** öffnet sich ein eigenes Fenster. Der Tab **Seed-Einstellungen** zeigt Ending-Ziel, Zielschwierigkeit, Start-Tranktyp, Blumenverteilung und Schema. Die Werte werden automatisch aus AP übernommen und können dort nicht verändert werden. Der Tab **AP-Hinweise** ist auch in der reinen Itemansicht verfügbar.

## AP-Hinweise

Der Tab **Hinweise** liest die bekannten AP-Hinweise für deinen Slot: sowohl Items für dich in anderen Welten als auch Items für andere Spieler in deiner Welt. Jede Anzeige nennt Item, Empfänger, Fundort, Finder, Priorität/Fundstatus und gegebenenfalls Eingang. Offene Hinweise stehen vor gefundenen, priorisierte zuerst. Mit **Weiter / Zurück** wechselst du zwischen Seiten mit je sechs Hinweisen. Lange Zeilen werden gekürzt; der Tooltip zeigt den vollständigen Text.

Neue Hinweise und Statusänderungen werden automatisch vom Server übernommen. Beim Verbinden werden alte Hinweise gelöscht und für den verbundenen Slot neu geladen. Ohne Verbindung bleiben bereits geladene Hinweise während dieser Sitzung als letzter Stand sichtbar. Sie werden nicht in lokalen Tracker-Saves gespeichert. Das Pack erstellt keine neuen Hinweise und verändert keine Prioritäten auf dem Server.

## Anzeigen

- **AP / Checks / Goal:** Verbindung, gesamte erledigte Checks und verschiedene erfolgreiche Endings auf der Seed-Zielschwierigkeit. Mit der Maus über das Verbindungssymbol fahren, um Blumenverteilung und Start-Tranktyp zu sehen. Das Goal-Symbol nennt die Zielschwierigkeit.
- **Schwierigkeiten:** Hard Wedding und Nightmare Wedding mit ihren Originalsymbolen aus der Schwierigkeitsauswahl. Nightmare benötigt laut APWorld keine zusätzliche Hard-Freigabe.
- **Tranktypen:** sechs Freigaben mit den Originalsymbolen aus dem Transformationsmenü. Gesperrte Typen sind ausgegraut, freigeschaltete farbig. Der Starttyp wird sofort gesetzt. Dieselben Symbole erscheinen auf der Formenübersicht.
- **AP-Upgrades:** 17 passive Upgrades mit den Originalgrafiken aus dem Shop und ihren empfangenen Stufen, einschließlich Maximalstufen. Namen und Maximalstufen stehen im Tooltip. Die 13 Standard-Skills gehören zum normalen Level-up-Pool und haben keine AP-Anzeige.
- **Runs:** 18 Kartenchecks, die Blumenchecks der drei Schwierigkeiten und die Ending-Meilensteine. Den Blumenmarker öffnen, um die einzelnen kumulativen Blumenchecks zu sehen. Die Anzahl passt sich dem Seed an, auch bei null Blumen auf einer Schwierigkeit.
- **Formen:** sechs volle Transformationen als separate Checks. Ein freier Tranktyp bedeutet noch keinen abgeschlossenen Transformationscheck.
- **Erfolge:** alle 38 nativen Errungenschaftschecks. Die Farben folgen den AP-Zugangsregeln; ein grüner Check bedeutet, dass seine Freigaben vorliegen. Die Bedingung muss weiterhin im Spiel erfüllt werden.
- **Broadcast:** zusätzliche kompakte Itemansicht für PopTrackers Broadcast-Fenster, in jeder Variante verfügbar.

Die Gesamtzahl ist immer **80**: 18 Karten, 6 volle Transformationen, 38 Errungenschaften, `transformEnd` Endings und `18 − transformEnd` Blumenchecks. Karten und Blumen zählen getrennt je Schwierigkeit; Endings zählen auf der Zielschwierigkeit. Wiederholte identische Endings erhöhen das Ziel nicht.

## Grenzen und manueller Betrieb

Die AP-Verbindung liest Serverzustand. Klicks im Tracker senden **keine** Checks, Items oder Spielaktionen an den AP-Server. AP-Itemanzeigen sind nach einer Verbindung gegen manuelles Verstellen gesperrt. Manuell abgehakte lokale Checkanzeigen werden bei der nächsten AP-Verbindung wieder vom Serverzustand ersetzt.

Ohne Verbindung zeigt ein neu geladenes Pack die Referenzkonfiguration: drei Normal-Endings, Blumen 4/5/6, Starttyp Beast. Diese Werte sind kein erkannter Seed. PopTracker speichert das zuletzt empfangene Layout zusammen mit seinem lokalen Zustand; ein erneutes Verbinden setzt alles zurück und übernimmt den Serverzustand neu.

Native Teilzähler, etwa 347/1000 besiegte Gegner, aktuelle HP oder Run-Skill-Stufen sind über AP nicht verfügbar. Der Tracker zeigt abgeschlossene Checks und dauerhafte AP-Items. Die Ending-Anzeige zählt die entsprechenden AP-Meilensteine, nicht eine zusätzliche Server-Goal-Statusmeldung.

## Quellen und Build

Pack-Format und APIs: [PopTracker](https://github.com/black-sliver/PopTracker), [Pack-Dokumentation](https://github.com/black-sliver/PopTracker/blob/master/doc/PACKS.md), [Archipelago-Autotracking](https://github.com/black-sliver/PopTracker/blob/master/doc/AUTOTRACKING.md#archipelago-interface).

`python tools/build_tracker.py` erzeugt JSON/Lua-Verträge und die reproduzierbare ZIP aus `apworld/wedding_witch/constants.py`, `achievements.py` und den eingecheckten PNGs. Benötigt Python 3.13+ und Pillow 11.3.0; eine Spielinstallation ist dafür nicht erforderlich. Die eigenen Übersichtsbilder werden mit `--render-artwork` neu erzeugt, wobei die Originalsymbole erhalten bleiben; die Schriftrasterung kann zwischen Betriebssystemen variieren. `python -m unittest discover -s tests -p test_tracker.py -v` prüft die Lua-Callbacks mit lupa 2.6 / Lua 5.4.

Zum erneuten Extrahieren der Originalsymbole in einer eigenen Python-Umgebung: `python -m pip install -r tools/requirements-tracker-assets.txt` installiert UnityPy 1.25.4 mit TypeTree-Unterstützung und die geprüften Bild-Abhängigkeiten. Dann `python tools/extract_tracker_assets.py --game-data "C:\Program Files (x86)\Steam\steamapps\common\Wedding Witch\Wedding Witch_Data"` ausführen. Das Werkzeug liest die installierten Spieldateien und exportiert ausschließlich die 27 benötigten UI-Symbole. Upgrade- und Transformationsgrafiken werden anhand der nativen Asset-Referenzen zugeordnet, ebenso die Symbole der beiden Schwierigkeitsknöpfe. `assets.json` nennt Quelle, Sprite, Bildgröße und Prüfsumme. Transparente Ränder werden zugeschnitten; die Proportionen bleiben auf quadratischen transparenten Tracker-Bildern erhalten.

Die 27 Originalsymbole gehören zu Wedding Witch / CHOWBIE. Diese Grafiken werden für die Anzeige des entsprechenden Spiels im Tracker verwendet; dieses Projekt vergibt dafür keine eigene Lizenz. Spielassemblies, Asset-Bundles, Charakterillustrationen und PopTracker-Programmdateien sind nicht Bestandteil des Packs. Die Diagramme und das AP-Verbindungssymbol wurden für dieses Pack erstellt. `images/charm.png` ist der unverändert kopierte, vom Projektinhaber bereitgestellte Anhänger aus `src/res/achievement-check.png`. Rechte an den jeweiligen Assets verbleiben bei ihren Inhabern.
