# Chroma v1.8

Chroma v1.8 completes the corrected automatic-update test path and adds automatic cleanup of updater artifacts.

## Updated

- Starts a delayed cleanup task whenever Chroma opens, allowing the external installer to finish first.
- Preserves the newest updater log as `%LOCALAPPDATA%\Chroma\Updates\last-update.log`.
- Deletes all versioned update directories after the installer has finished.
- Removes downloaded ZIP files, extracted staging copies, rollback backups, and obsolete updater scripts.
- Future updater runs also remove older sibling update directories immediately after a successful installation.
- Continues to replace and SHA-256 verify the exact folder containing the running `Chroma.exe`.

## Test from v1.7

1. Launch the correctly installed v1.7 copy.
2. Accept the v1.8 update prompt.
3. Confirm the visible progress window appears.
4. Confirm Chroma restarts from the same folder and reports v1.8.
5. Wait about 15 seconds.
6. Check `%LOCALAPPDATA%\Chroma\Updates`; only `last-update.log` should remain.
