# Install Wedding Witch Archipelago

Windows package 0.5.1 includes mod and APWorld 0.5.1, the Steam launcher, native standard skills at level-up and 38 achievement checks. New seeds have 80 items and checks.

## Requirements

- The Windows Steam version of Wedding Witch (app ID 2529820), launched normally at least once.
- Windows 10/11 x64 with Steam installed and signed in.
- Extract the complete WeddingWitch-AP-0.5.1-Windows.zip.
- An internet connection for first-time installation. If needed, the installer downloads the official BepInEx 5.4.23.5 x64 release and verifies its SHA-256 digest.

## Installation

1. Close Wedding Witch. The installer will refuse to interrupt a running game.
2. Double-click Install.cmd. It searches your Steam library folders. If the game directory requires elevated write permissions, run the installer as administrator.
3. If automatic detection fails, open PowerShell in the extracted package and run `./Install.ps1 -GameDir 'D:\SteamLibrary\steamapps\common\Wedding Witch'`.
4. The installer places WeddingWitchLauncher.exe in the game directory and the mod in BepInEx/plugins/WeddingWitchCustom. Existing AP profiles and replaced mod files are backed up; saves are not deleted.
5. Optionally create a desktop shortcut to WeddingWitchLauncher.exe.

## Launch the original game or Archipelago

Keep Steam signed in. Open WeddingWitchLauncher.exe and choose:

- **Original starten / Launch original:** disables the entire BepInEx loader for this process, starting the original game without AP.
- **Archipelago starten / Launch Archipelago:** starts the game with the AP mod. Press F8 to open the connection panel.

The launcher does not modify doorstop_config.ini or your saves when switching modes. Shortcuts can use `--original` or `--ap`. Launching from Steam normally loads BepInEx after installation; use the launcher for an unmodified session. If Steam is closed, the launcher opens Steam first. Once you are signed in, click your desired launch button again.

The launcher starts both modes through Steam, using `--doorstop-enabled true` for AP and `--doorstop-enabled false` for the original game. This makes the Steam controller configuration available when launching either mode. If a controller works with Steam's Play button but not with the previous launcher, use the updated launcher.

## YAML and server connection

The included German YAML explains every option and supports weighted selections. A weight of 0 disables a selection; a fixed value such as difficulty: normal is also valid. Progression balancing defaults to normal (setting 50), with disabled (0) and extreme (99) available. Common AP settings include start inventory from the pool, hints and item/location placement. YAML changes apply when generating a new seed.

1. Select Wedding Witch on ap.dsatool.org and configure your player YAML: slot name, number of different endings (transformEnd 1–7), goal difficulty and starting potion type. Flower checks can be allocated automatically or explicitly within the total budget.
2. Download the YAML or assign it to your lobby slot. Generate the seed and start the room.
3. Launch Archipelago, press F8, and enter the room host, port, exact slot name and password if required. Use the local server address for local tests.
4. Connect and start a new run. Your AP profile is bound to seed, team and slot. In new schema 4 seeds, standard skills are selected and upgraded at level-up; their ranks last for the current run.
5. Real progress appears only while the game client is connected and sends checks.

Seed generation and connections through ap.dsatool.org have been verified. For the 80-item pool and standard skills at level-up, the portal must use APWorld 0.5.1 and generate a new schema 4 seed. Existing schema 2 and 3 seeds retain their 142 checks and AP skill ranks and remain playable with the new client.

## APWorld for local hosts

Players joining a portal room do not need a local Archipelago installation. To generate or host locally, install Archipelago 0.6.7 and copy wedding_witch.apworld into custom_worlds. Alternatively run `./Install.ps1 -InstallWorld -ArchipelagoDir 'C:\ProgramData\Archipelago'`. Enable only one Wedding Witch world. This custom variant is not interchangeable with upstream or prototype 0.2.x seeds.

## Rules

80 items and 80 addressed locations: 73 permanent passive ranks, two difficulty unlocks and five potion-type unlocks. The 62 Skill Unlock items are removed; all 13 standard skills are normal level-up choices for the current run. One potion type starts unlocked. Masteries remain vanilla gameplay choices.

Locked AP potion types show the six-colour charm in the transformation menu. Unlocked types show their original icons; the extra green AP outline is removed. Receiving an unlock item also refreshes the open menu.

Schema 4 seeds restore the native level-up pool of standard skills and the starter spells enabled by the game. Spells with potion conditions still require those potions; fully upgraded skills and the native equipped-spell limit restrict the available pool. The passive fourth-choice upgrade is retained.

Locations include five/six/seven map clears on Normal/Hard/Nightmare, six unique full transformations, 38 achievements, one milestone for each new successful ending up to the goal, and cumulative flower pickups. Flower budget = 18 minus transformEnd. With three endings, that means 15 flower checks, allocated automatically as 4/5/6 by difficulty. Full transformation requires six potion points of one type. Strong potions give two points and can appear on Normal elite missions.

Each achievement automatically sends one check when its native condition is fulfilled in the current seed. The six-colour AP charm replaces the gold icon and amount. These achievements give no coins; claiming acknowledges the check already sent. Counters and completed/claimed states are stored per seed/team/slot; previous vanilla achievements and other seeds do not count. The 100-flower achievement includes all natural flower pickups even after the reduced AP flower-check budget is exhausted. The difficulty selection progress card also shows achievement checks.

The goal counts different endings on the selected difficulty: the normal form plus six transformation forms. Repeated wins with the same ending increase the win count but not goal progress. Transforming without defeating the final boss only awards the transformation check.

## Troubleshooting

- No F8 panel: choose Archipelago in the launcher and check BepInEx/LogOutput.log for Wedding Witch Archipelago 0.5.1.
- Original mode still loads mods: use the current launcher and its Original button. Steam's normal Play button loads the mod loader after installation.
- Still seeing 142 checks or AP skill items: generate a new seed with APWorld 0.5.1. Existing seeds cannot be converted to the 80-item pool and native level-up skills.
- Connection failure: verify host, port, slot name and password against the running room. Keep cheats disabled.
- Duplicate world: move the old Wedding Witch variant out of custom_worlds.
- Missing launcher or DLL: extract the entire Windows ZIP, not individual files.
- Installation refused: close the game and check directory write permissions.

## Updates and removal

Close the game and run the new package's Install.cmd. AP profiles, configuration and original saves are retained. Backups are stored in BepInEx/WeddingWitch-backups inside the game directory.

To remove only AP, delete BepInEx/plugins/WeddingWitchCustom and WeddingWitchLauncher.exe. If BepInEx was installed solely for AP, removing winhttp.dll also disables the loader; account for other installed mods first. Keep AP profiles if you might return later. Steam can verify original game files if needed.
