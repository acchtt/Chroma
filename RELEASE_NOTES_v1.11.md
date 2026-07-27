# Chroma v1.11

This release adds optional per-profile custom resolution switching while preserving CHROMA's compact portable design.

## Custom resolutions

- Replaces the disabled custom-resolution preview with a working profile editor.
- Adds an enable switch plus manual width and height fields.
- Validates entries from 640–16384 pixels wide and 480–8640 pixels high.
- Stores resolution overrides separately from existing saturation profiles for backward compatibility.
- Targets the monitor containing the foreground game window.
- Applies only display modes already exposed by Windows and the graphics driver.
- Preserves the monitor's current refresh rate instead of silently dropping to a lower rate.
- Restores the exact previous desktop mode when the game closes, another profile activates, profiles reload, or the agent exits.
- Continues applying saturation when a requested resolution is unsupported.

## Safety and compatibility

- Uses documented Windows display-mode APIs; no game injection, memory access, overlay, or kernel driver is introduced.
- Existing profiles, custom names, GPU selection, saturation settings, updater behavior, and anti-cheat disclosure are preserved.
- Intel IGCL, NVIDIA NVAPI, and AMD ADLX saturation backends are unchanged.
- The release remains a compressed self-contained two-file Windows x64 package.

## Notes

CHROMA v1.11 does not create new GPU-driver timings. The requested width and height must already appear as a supported mode for the target monitor at its current refresh rate.
