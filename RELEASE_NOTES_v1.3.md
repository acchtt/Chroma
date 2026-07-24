# Chroma v1.3

This maintenance release fixes application and tray-agent lifecycle behavior.

## Fixes

- Launches `Chroma.Agent.exe` directly from the folder containing `Chroma.exe`
  instead of relying on shell execution or a runtime extraction directory.
- Reports a clear error when the two portable executables have not been
  extracted into the same folder.
- Makes the title-bar X button close the interface normally by default.
- Prevents minimize-to-tray mode from trapping the window when the tray agent
  is unavailable.
- Preserves minimize-to-tray as an optional setting after the new close default
  has been applied once.

## Package

Extract both files from `Chroma-v1.3-win-x64.zip` before launching:

- `Chroma.exe`
- `Chroma.Agent.exe`
