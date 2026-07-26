# Chroma v1.7

Chroma v1.7 repairs the automatic updater installation target.

## Fixed

- The updater now resolves the installation from the absolute path of the currently running `Chroma.exe` instead of `AppContext.BaseDirectory`.
- The updater verifies that `Chroma.Agent.exe` is beside the running application before changing files.
- Updates are rejected when Chroma is launched from `%LOCALAPPDATA%\Chroma\Updates`.
- The exact running installation path is carried from download preparation into the external installer.
- Replaced files are verified against the staged files with SHA-256 before restart.
- Temporary staged executable copies, the downloaded ZIP, and rollback backups are removed after a successful update.
- The update log records the exact installation target and staging source.

## One-time upgrade from v1.6

Chroma v1.6 contains the old target-selection code, so it cannot reliably install v1.7 over the folder from which v1.6 was launched.

For this upgrade only:

1. Download `Chroma-v1.7-win-x64.zip` from GitHub Releases.
2. Close Chroma and `Chroma.Agent.exe`.
3. Extract the ZIP.
4. Copy `Chroma.exe` and `Chroma.Agent.exe` over the two files in your intended Chroma folder.
5. Launch that folder's `Chroma.exe` and confirm the About page reports `Version v1.7`.

After v1.7 is installed manually, future automatic updates will replace files in that same folder.

## Package

The release contains:

```text
Chroma.exe
Chroma.Agent.exe
```

The matching `Chroma-v1.7-win-x64.zip.sha256` file is included for integrity verification.
