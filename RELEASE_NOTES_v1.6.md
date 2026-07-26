# Chroma v1.6

Chroma v1.6 fixes the automatic-update experience discovered during the v1.4 to v1.5 validation test.

## Fixed

- Chroma now checks GitHub for a newer stable release whenever the application window opens.
- The first normal launch uses the startup update check and automatically displays the update-available dialog when a newer version exists.
- Reopening Chroma from the system tray or launching it again while it is already running performs a fresh update check.
- Restored the visible updater progress dialog during download, SHA-256 verification, file preparation, and restart.
- The footer continues to show the same update stage and percentage as the progress dialog.

## Update safety

- Release downloads remain protected by SHA-256 verification.
- Staged updates are validated before installed files are replaced.
- Existing rollback and automatic-restart behavior is retained.
- The release archive contains `Chroma.exe` and `Chroma.Agent.exe` directly at its root for updater compatibility.

## Test from v1.5

1. Fully close Chroma v1.5, then launch it again.
2. Confirm the v1.6 update-available dialog appears automatically.
3. Choose **Install and restart**.
4. Confirm a progress dialog remains visible while the archive downloads and is verified.
5. Allow Chroma to restart.
6. Confirm the footer and About page report **Version v1.6**.
7. Close Chroma to the tray and reopen it to confirm a fresh update check occurs.

The release includes `Chroma-v1.6-win-x64.zip` and its matching `.sha256` file.
