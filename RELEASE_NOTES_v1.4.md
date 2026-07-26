# Chroma v1.4

Chroma v1.4 expands the application from an Intel-only utility into a multi-GPU saturation-profile tool while preserving the lightweight two-executable portable package.

## Highlights

- Added native NVIDIA NVAPI Digital Vibrance support.
- Added native AMD ADLX custom-color support.
- Retained the verified Intel IGCL saturation backend.
- Added primary-display GPU identification to the monitoring card.
- Updated the About page, Settings text, website, and documentation for Intel, NVIDIA, and AMD graphics.
- Generalized agent status messages and logging so they no longer assume an Intel backend.
- Added a reproducible release build that checks out a pinned AMD ADLX revision.
- Added repository cleanup tooling and stronger ignore rules for generated files and local SDK checkouts.

## GPU validation status

- **Intel:** verified on Intel Arc hardware.
- **NVIDIA:** backend implemented; broader hardware and driver validation is still needed.
- **AMD:** backend implemented; broader hardware and driver validation is still needed.

Display routing on hybrid-GPU laptops may affect which backend controls the connected monitor.

## Package

Extract both files from `Chroma-v1.4-win-x64.zip` into the same folder before launching:

- `Chroma.exe`
- `Chroma.Agent.exe`

The release also includes `Chroma-v1.4-win-x64.zip.sha256` for integrity verification.
