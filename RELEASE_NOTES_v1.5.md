# Chroma v1.5

Chroma v1.5 is a validation release for the built-in automatic updater. It packages the current stable application as a newer signed-by-checksum release so installations running v1.4 can exercise the complete update flow.

## Highlights

- Bumped the application and assembly version to v1.5.
- Retained native saturation control for Intel IGCL, NVIDIA NVAPI, and AMD ADLX.
- Retained automatic per-game profile activation and desktop color restoration.
- Retained primary-display GPU identification in the monitoring card.
- Retained the updater progress interface, SHA-256 verification, rollback protection, and automatic restart.
- Refreshed the repository and website presentation with the latest Chroma interface screenshot.

## Automatic updater test

From an existing Chroma v1.4 installation:

1. Launch `Chroma.exe` and wait for the initial update check, or press **Up to date / Updates** in the footer.
2. Confirm that Chroma reports **v1.5** as available.
3. Start the update and verify that download progress is displayed.
4. Confirm that checksum verification and update preparation complete successfully.
5. Allow Chroma to restart.
6. Confirm that the footer and About page report **Version v1.5** and that existing profiles remain available.

## Package

The archive `Chroma-v1.5-win-x64.zip` contains:

```text
Chroma-v1.5/
├── Chroma.exe
└── Chroma.Agent.exe
```

The release also includes `Chroma-v1.5-win-x64.zip.sha256` for updater and manual integrity verification.

Extract the folder before launching Chroma manually. Keep `Chroma.Agent.exe` beside `Chroma.exe`.
