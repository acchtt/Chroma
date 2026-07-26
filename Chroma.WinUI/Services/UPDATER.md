# Chroma updater behavior

The updater downloads and extracts release files under `%LOCALAPPDATA%\Chroma\Updates` only as temporary staging data.

Before installation, Chroma records and validates the absolute path of the currently running `Chroma.exe`. The installer then:

1. waits for that exact process to exit;
2. stops the companion agent from the same folder;
3. backs up and replaces `Chroma.exe` and `Chroma.Agent.exe` in that folder;
4. verifies the copied files by SHA-256;
5. restarts the replaced executable; and
6. removes the staged executable copies after success.

The updater refuses to run when the app is launched from its temporary update directory or when `Chroma.Agent.exe` is not beside the running application.
