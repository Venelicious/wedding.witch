# Install Wedding Witch Archipelago

Windows package 0.3.7 includes mod 0.3.5 and APWorld 0.3.6 and the launcher fix tested in the actual game.

## Requirements

- The Windows Steam version of Wedding Witch (app ID 2529820), launched normally at least once.
- Windows 10/11 x64 with Steam installed and signed in.
- Download and extract the complete WeddingWitch-AP-0.3.7-Windows.zip.
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

## YAML and server connection

1. Select Wedding Witch on ap.dsatool.org and configure your player YAML: slot name, number of different endings (transformEnd 1–7), goal difficulty and starting potion type. Flower checks can be allocated automatically or explicitly within the total budget.
2. Download the YAML or assign it to your lobby slot. Generate the seed and start the room.
3. Launch Archipelago, press F8, and enter the room host, port, exact slot name and password if required. Use the local server address for local tests.
4. Connect and start a new run. Your AP profile is bound to seed, team and slot. Received standard skill ranks are automatically owned and active, and applied again in new runs. You do not buy or select them during level-ups.
5. Real progress appears only while the game client is connected and sends checks.

The website configurator becomes available after the portal integration is published. Until then, use WeddingWitch.yaml from the release.

## APWorld for local hosts

Players joining a portal room do not need a local Archipelago installation. To generate or host locally, install Archipelago 0.6.7 and copy wedding_witch.apworld into custom_worlds. Alternatively run `./Install.ps1 -InstallWorld -ArchipelagoDir 'C:\ProgramData\Archipelago'`. Enable only one Wedding Witch world. This custom variant is not interchangeable with upstream or prototype 0.2.x seeds.

## Rules

142 items and 142 addressed locations: 73 permanent passive ranks, two difficulty unlocks, five potion-type unlocks and 62 standard skill ranks. One potion type starts unlocked. Masteries remain vanilla gameplay choices.

Locations include five/six/seven map clears on Normal/Hard/Nightmare, six unique full transformations, one milestone for each new successful ending up to the goal, and cumulative flower pickups. Flower budget = 118 minus transformEnd. Full transformation requires six potion points of one type. Strong potions give two points and can appear on Normal elite missions.

The goal counts different endings on the selected difficulty: the normal form plus six transformation forms. Repeated wins with the same ending increase the win count but not goal progress. Transforming without defeating the final boss only awards the transformation check.

## Troubleshooting

- No F8 panel: choose Archipelago in the launcher and check BepInEx/LogOutput.log for Wedding Witch Archipelago 0.3.5.
- Original mode still loads mods: ignore_disable_switch must be false in doorstop_config.ini; Install.cmd sets it. Use the launcher instead of Steam's Play button.
- Connection failure: verify host, port, slot name and password against the running room. Keep cheats disabled.
- Duplicate world: move the old Wedding Witch variant out of custom_worlds.
- Missing launcher or DLL: extract the entire Windows ZIP, not individual files.
- Installation refused: close the game and check directory write permissions.

## Updates and removal

Close the game and run the new package's Install.cmd. AP profiles, configuration and original saves are retained. Backups are stored in BepInEx/WeddingWitch-backups inside the game directory.

To remove only AP, delete BepInEx/plugins/WeddingWitchCustom and WeddingWitchLauncher.exe. If BepInEx was installed solely for AP, removing winhttp.dll also disables the loader; account for other installed mods first. Keep AP profiles if you might return later. Steam can verify original game files if needed.
