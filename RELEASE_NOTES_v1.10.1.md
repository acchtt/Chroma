# Chroma v1.10.1

This maintenance release substantially reduces the installed size of the portable Chroma application without requiring users to install the .NET runtime or Windows App Runtime separately.

## Changes

- Compresses the self-contained single-file WinUI payload.
- Keeps ReadyToRun disabled to avoid unnecessary executable-size overhead.
- Removes unused non-English satellite resource assemblies from the English-only interface.
- Keeps WinUI trimming disabled to avoid reflection and XAML compatibility risks.
- Preserves the updater-compatible two-file package containing only `Chroma.exe` and `Chroma.Agent.exe`.
- Adds exact executable-size reporting to local and CI builds.

## Validated size comparison

- `Chroma.exe`: **215,904,024 bytes → 89,846,633 bytes**
- Installed UI executable reduction: **126,057,391 bytes (about 58.4%)**
- `Chroma.Agent.exe`: **579,072 bytes** (unchanged)
- Validated GitHub Actions artifact: **85,359,190 bytes → 84,169,265 bytes**

The download ZIP changes less because the previous uncompressed executable was already compressed by the ZIP container. The major improvement is the amount of disk space used after extraction or installation.

## Compatibility

- Existing profiles and settings are preserved.
- The GPU selector and Intel, NVIDIA, and AMD backends are unchanged.
- Automatic updates still replace the exact folder containing the launched `Chroma.exe`.
- The release remains self-contained and portable on supported Windows 10 and Windows 11 x64 systems.
