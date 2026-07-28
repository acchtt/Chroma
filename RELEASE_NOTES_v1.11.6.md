# Chroma v1.11.6

This release replaces manual custom-resolution entry with linked display-mode dropdowns.

## Custom resolution editor

- Replaces the Width and Height text boxes with ComboBox dropdown lists.
- Builds the available choices from modes exposed by active Windows displays at their current refresh rates.
- Filters the Height list when a Width is selected so only matching mode pairs are shown.
- Defaults to the current primary-display resolution for profiles without an override.
- Preserves an existing saved resolution in the selectors when its monitor is temporarily disconnected.
- Revalidates the selected mode on the actual game monitor when the profile activates.

## Compatibility

- Keeps the existing `resolutions.txt` profile format unchanged.
- Keeps native resolution switching, refresh-rate preservation, desktop restoration, saturation, GPU selection, updater, tray, startup, and anti-cheat behavior unchanged.
- Remains a compressed self-contained two-file Windows x64 package.
