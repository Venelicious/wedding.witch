# Wedding Witch AP · PopTracker 0.1.0

Für Venelicious/wedding.witch, APWorld **0.5.0/0.5.1, Schema 4**, mit 80 Checks.
Benötigt **PopTracker 0.35.4 oder neuer**. Alte 142-Check-Seeds und andere Wedding-Witch-APWorlds werden mit einer Fehlermeldung abgewiesen.

## Installation und Verbindung

1. `WeddingWitch-PopTracker-0.1.0.zip` unverändert in PopTrackers `packs`-Ordner kopieren. Nicht entpacken.
2. PopTracker starten und **Wedding Witch AP → Archipelago · 80 Checks** laden.
3. Oben die **AP**-Verbindung aktivieren. Dieselbe Serveradresse, denselben Slotnamen und gegebenenfalls dasselbe Raumpasswort wie im Mod eintragen. Die Verbindung funktioniert parallel zum Spiel.
4. Sobald die Verbindung steht, übernimmt das Pack Ziel, Schwierigkeit, Start-Tranktyp und Blumenverteilung aus den Slotdaten. Empfangene Items und abgeschlossene Checks werden synchronisiert.

Das Pack hat eine eigene Versionsnummer; der Mod bleibt unverändert. Updates des Packs ersetzen nur die Tracker-ZIP.

## Anzeigen

- **AP / Checks / Goal:** Verbindung, gesamte erledigte Checks und verschiedene erfolgreiche Endings auf der Seed-Zielschwierigkeit. Mit der Maus über das Verbindungssymbol fahren, um Blumenverteilung und Start-Tranktyp zu sehen. Das Goal-Symbol nennt die Zielschwierigkeit.
- **Schwierigkeiten:** Hard Wedding und Nightmare Wedding. Nightmare benötigt laut APWorld keine zusätzliche Hard-Freigabe.
- **Tranktypen:** sechs Freigaben. Gesperrte Typen zeigen den bereitgestellten sechsfarbigen Anhänger; freie Typen ein eigenes farbiges Kürzelsymbol. Der Starttyp wird sofort gesetzt.
- **AP-Upgrades:** 17 passive Upgrades mit ihren empfangenen Stufen, einschließlich Maximalstufen. Namen und Maximalstufen stehen im Tooltip. Die 13 Standard-Skills gehören zum normalen Level-up-Pool und haben keine AP-Anzeige.
- **Runs:** 18 Kartenchecks, die Blumenchecks der drei Schwierigkeiten und die Ending-Meilensteine. Den Blumenmarker öffnen, um die einzelnen kumulativen Blumenchecks zu sehen. Die Anzahl passt sich dem Seed an, auch bei null Blumen auf einer Schwierigkeit.
- **Formen:** sechs volle Transformationen als separate Checks. Ein freier Tranktyp bedeutet noch keinen abgeschlossenen Transformationscheck.
- **Erfolge:** alle 38 nativen Errungenschaftschecks. Die Farben folgen den AP-Zugangsregeln; ein grüner Check bedeutet, dass seine Freigaben vorliegen. Die Bedingung muss weiterhin im Spiel erfüllt werden.
- **Broadcast:** zusätzliche kompakte Itemansicht für PopTrackers Broadcast-Fenster.

Die Gesamtzahl ist immer **80**: 18 Karten, 6 volle Transformationen, 38 Errungenschaften, `transformEnd` Endings und `18 − transformEnd` Blumenchecks. Karten und Blumen zählen getrennt je Schwierigkeit; Endings zählen auf der Zielschwierigkeit. Wiederholte identische Endings erhöhen das Ziel nicht.

## Grenzen und manueller Betrieb

Die AP-Verbindung liest Serverzustand. Klicks im Tracker senden **keine** Checks, Items oder Spielaktionen an den AP-Server. AP-Itemanzeigen sind nach einer Verbindung gegen manuelles Verstellen gesperrt. Manuell abgehakte lokale Checkanzeigen werden bei der nächsten AP-Verbindung wieder vom Serverzustand ersetzt.

Ohne Verbindung zeigt ein neu geladenes Pack die Referenzkonfiguration: drei Normal-Endings, Blumen 4/5/6, Starttyp Beast. Diese Werte sind kein erkannter Seed. PopTracker speichert das zuletzt empfangene Layout zusammen mit seinem lokalen Zustand; ein erneutes Verbinden setzt alles zurück und übernimmt den Serverzustand neu.

Native Teilzähler, etwa 347/1000 besiegte Gegner, aktuelle HP oder Run-Skill-Stufen sind über AP nicht verfügbar. Der Tracker zeigt abgeschlossene Checks und dauerhafte AP-Items. Die Ending-Anzeige zählt die entsprechenden AP-Meilensteine, nicht eine zusätzliche Server-Goal-Statusmeldung.

## Quellen und Build

Pack-Format und APIs: [PopTracker](https://github.com/black-sliver/PopTracker), [Pack-Dokumentation](https://github.com/black-sliver/PopTracker/blob/master/doc/PACKS.md), [Archipelago-Autotracking](https://github.com/black-sliver/PopTracker/blob/master/doc/AUTOTRACKING.md#archipelago-interface).

`python tools/build_tracker.py` erzeugt JSON/Lua-Verträge, eigene Übersichtsbilder und die reproduzierbare ZIP aus `apworld/wedding_witch/constants.py` und `achievements.py`. Benötigt Pillow 11.3.0. `python -m unittest discover -s tests -p test_tracker.py -v` prüft die Lua-Callbacks mit lupa 2.6 / Lua 5.4.

Die Diagramme und Kürzelsymbole wurden für dieses Pack erstellt. `images/charm.png` ist der unverändert kopierte, vom Projektinhaber bereitgestellte Anhänger aus `src/res/achievement-check.png`. Es werden keine extrahierten Spielgrafiken oder PopTracker-Programmdateien verteilt. Rechte an den bereitgestellten Assets verbleiben bei ihren jeweiligen Inhabern.
