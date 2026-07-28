# Chroma v1.11.8

This release restructures the profile editor based on the real v1.11.7 layout screenshot.

## Profile editor polish

- Rebalances the editor width from 480 px to 450 px so the profile list keeps more room.
- Compresses the selected-game header and limits the executable path to one ellipsized line.
- Groups saturation into a dedicated compact card with the numeric value control in the header.
- Collapses width, height, help, and status details while custom resolution is disabled.
- Keeps the selected resolution controls visible only when the profile override is enabled.
- Shortens the active-resolution summary and tightens the fixed action row.

## Footer correction

- Defers the compact utility-footer conversion until the Profiles XAML is loaded.
- Restores the single-row Chroma/version, Website, GitHub, Check updates, and logo layout.
- Reduces the footer row height to return more room to the main workspace.

## Compatibility

- Keeps profile storage, saturation, custom-resolution mode enumeration, refresh-rate preservation, desktop restoration, GPU selection, updater, tray, startup, and anti-cheat behavior unchanged.
- Remains a compressed self-contained two-file Windows x64 package.
