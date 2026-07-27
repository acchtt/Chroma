# Chroma v1.10.2

This transparency-focused maintenance release explains how Chroma interacts with games and anti-cheat systems without making unsupported certification claims.

## Changes

- Adds an **Anti-cheat safety** card to the About page.
- Documents that Chroma does not inject code or DLLs into games.
- Documents that Chroma does not read or write game memory.
- Documents that Chroma does not provide an internal overlay, modify game files, automate input, or install a kernel driver.
- Explains that foreground detection uses only a limited Windows executable-path query.
- Explains that saturation is applied through Intel IGCL, NVIDIA NVAPI, or AMD ADLX.
- Adds a matching **Anti-cheat safety** section and navigation link to the Chroma website.
- Updates the website and structured metadata to v1.10.2.

## Assessment wording

Based on the current source code, Chroma is considered low risk for VAC, BattlEye, Easy Anti-Cheat, and Riot Vanguard. This is an architectural assessment—not an official certification, endorsement, or allowlisting.

No third-party utility can guarantee approval by every game or anti-cheat provider. Game-publisher policies and anti-cheat detection rules can change.

## Compatibility

- Existing profiles and settings are preserved.
- GPU backends and the GPU selector are unchanged.
- The compressed self-contained two-file portable package is unchanged.
- Automatic updates remain compatible with existing v1.10 and v1.10.1 installations.
