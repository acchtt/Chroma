# Chroma v1.11.15

## Footer redesign and update status

- Rebuilt the Profiles footer as a cleaner three-zone utility bar.
- Reduced the brand block to the Chroma logo, product name, version, and creator credit.
- Removed the slogan chip, decorative underline, and vertical divider from the footer.
- Simplified the anti-cheat badge to a compact single-line status with the detailed disclaimer preserved in its tooltip.
- Rebalanced the Website, GitHub, and Updates controls with tighter spacing and consistent sizing.
- Kept the update action labelled **Updates** during normal and update-available states.
- Added an update-available visual state: the Updates button switches to Chroma's bright gradient with white text and icon only when a newer stable release is detected.
- Normalized updater progress labels so checking, preparing, downloading, installing, and percentage states remain compact and do not truncate.
- Reapplies the correct Updates-button state after light or dark theme changes.

No GPU-control, profile, native-agent, or release-download behavior was changed.
