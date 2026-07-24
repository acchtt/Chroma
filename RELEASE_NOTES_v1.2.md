# Chroma v1.2

Chroma v1.2 completes the product-wide rename across the application,
repository, release packages, updater, website, and internal runtime identifiers.

## What changed

- Renamed the WinUI project, native runtime, namespaces, solution, executables,
  resources, IPC endpoints, logs, and build targets to Chroma.
- Renamed the portable release archive to `Chroma-v1.2-win-x64.zip`.
- Updated the updater, website, documentation, and GitHub links for the Chroma
  repository and releases.
- Kept the compact two-file Windows package:
  - `Chroma.exe`
  - `Chroma.Agent.exe`

## Upgrade migration

On first launch, Chroma imports existing profiles, custom game names, theme and
close settings, and Windows startup preferences into the new Chroma storage
locations. Legacy profile headers remain readable so saved profiles are not lost.

Because the executable and release-archive names change in this release,
install v1.2 manually over the existing portable folder. Future updates resume
through Chroma's built-in updater.

## Requirements

- Windows 10 or Windows 11, 64-bit
- Intel Arc GPU
- Current Intel graphics driver

Native AMD and NVIDIA support remains planned for a future release.
