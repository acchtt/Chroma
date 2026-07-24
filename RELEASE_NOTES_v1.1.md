# Chroma v1.1

Version 1.1 introduced Chroma's vendor-neutral visual identity while retaining
the established application internals for upgrade compatibility.

## What changed

- Introduced the Chroma name and multicolor C logo.
- Added matching application, taskbar, tray, About-page, footer, favicon, and website artwork.
- Recolored the WinUI application and website around Chroma's cyan, violet, magenta, and deep-navy palette.
- Updated user-facing dialogs, tray actions, status messages, documentation, and website content.
- Refreshed the website application preview and removed obsolete branding assets.

## Upgrade compatibility

This was a transitional release. The Windows archive and executable names
remained compatible with v1.0 so the updater, startup entry, saved profiles,
and settings continued to work without migration.

The portable package still contains only:

- `Chroma.exe`
- `Chroma.Agent.exe`

## Requirements

- Windows 10 or Windows 11, 64-bit
- Intel Arc GPU
- Current Intel graphics driver

Native AMD and NVIDIA support is planned for a future release and is not included in v1.1.
