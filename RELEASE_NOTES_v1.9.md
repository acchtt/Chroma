# Chroma v1.9

Chroma v1.9 refreshes the Windows executable and tray branding using the current shared Chroma icon asset.

## Updated

- Rebuilt `Chroma.exe` with the current `Chroma.WinUI/Assets/Chroma.ico` asset.
- Rebuilt `Chroma.Agent.exe` and its system-tray icon from the same shared ICO resource.
- Confirmed the current branding artwork is already present in the repository and displays correctly after a clean build.
- Removed the temporary icon-generation workflow used during investigation.
- Updated application, README, website, and release metadata to v1.9.

## Test from v1.8

1. Launch Chroma v1.8 and accept the v1.9 update.
2. Confirm the visible updater progress window appears.
3. Confirm Chroma restarts from the same installation folder and reports v1.9.
4. Confirm the window, taskbar, executable, agent, and system-tray icons use the current multicolor Chroma mark.
5. If Explorer temporarily shows an older cached icon, restart Explorer or sign out and back in.
6. Wait about 15 seconds and confirm `%LOCALAPPDATA%\Chroma\Updates` contains only `last-update.log`.
