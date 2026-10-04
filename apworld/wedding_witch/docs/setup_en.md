# Setup

Use the custom 0.5.2 BepInEx 5 Mono client and generate a new schema 4 seed with APWorld 0.5.2 for 80 checks and standard skills at level-up. This client also supports existing custom schema 2 and 3 seeds with their original 142-check layout and AP skill ranks. Keep upstream and additive prototype seeds with their original clients. Close the game and run Install.cmd from the bundle. Press F8 in the main menu to enter host, port and slot.

Enter the connection details and exact slot name of your room. Saves and achievement states remain separated by seed/team/slot. Returning to the main menu permits reconnecting after an outage. Portal generation requires the portal to have APWorld 0.5.2 installed.

DeathLink is optional and off by default. Set `death_link: true` in your YAML to enable it for a new seed. The F8 panel also lets you toggle DeathLink for the current connection, including older seeds; reconnecting restores the seed setting. A final local death is sent after native revives are exhausted. Received deaths end an active run, bypassing revives and invulnerability, without being sent back. Deaths outside an active run are discarded. The check budget and goal remain unchanged.
