# Chroma v1.9

Chroma v1.9 refreshes the Windows application, agent, taskbar, and system-tray icons from the current Chroma branding source.

## Updated

- Added a deterministic .NET icon builder that converts the current 1024 × 1024 `Chroma.png` artwork into a Windows ICO.
- Generates 16, 20, 24, 32, 40, 48, 64, 128, and 256 pixel PNG-compressed icon layers.
- Runs icon generation before the native agent compiles, so `Chroma.Agent.exe` and its system-tray icon embed the refreshed artwork.
- Runs icon generation before WinUI compiles, so `Chroma.exe`, its window icon, and its taskbar icon embed the same artwork.
- Keeps `NativeAgent/Chroma.rc` and the WinUI project pointed to one shared `Chroma.ico` asset.
- Updated the app and repository metadata to v1.9.

## Test from v1.8

1. Launch the correctly installed Chroma v1.8 copy and accept the v1.9 update.
2. Confirm Chroma restarts from the same folder and reports v1.9.
3. Confirm `Chroma.exe` shows the current multicolor Chroma mark in Explorer, the window, and the taskbar.
4. Confirm `Chroma.Agent.exe` and the system-tray icon use the same mark.
5. Wait about 15 seconds and confirm old updater artifacts are removed as introduced in v1.8.

Windows may temporarily display a cached executable icon after replacement. Restarting Windows Explorer or signing out refreshes the shell icon cache without changing the installed files.
