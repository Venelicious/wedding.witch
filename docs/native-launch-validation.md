# Native launch validation — 2026-10-03

Windows package 0.3.6 contains unchanged mod and APWorld 0.3.5.

- Installed the public GitHub package on Windows/Steam using its packaged installer after verifying SHA-256.
- Backups created; both existing AP profile files retained exactly the same SHA-256 before installation and after both launch tests.
- Fixed AP mode to remove DOORSTOP_DISABLE, including an inherited value. Native build and revised process-environment self-test passed.
- Actual AP launch produced a fresh BepInEx log: mod 0.3.5 loaded, chainloader startup completed, existing room/slot connected, game window responsive, no Error/Fatal entries in startup log.
- Actual original launch with the fixed launcher: game window responsive; BepInEx log and doorstop configuration unchanged.
- Closed both test game processes normally; existing AP server retained. Desktop shortcut created and launcher selection left open.
- Full gameplay acceptance of all endings and a native multiworld playthrough remain pending.
