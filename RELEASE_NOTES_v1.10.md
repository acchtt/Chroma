# Chroma v1.10

Chroma v1.10 adds a persistent GPU backend selector for multi-GPU and hybrid-GPU Windows systems.

## Added

- Added a **Graphics processor** selector to Settings.
- Added **Automatic**, **Intel**, **NVIDIA**, and **AMD** choices.
- The selector lists the active adapters Windows reports for each supported vendor.
- The selected backend is saved under the existing per-user Chroma settings key.
- Changing the selection safely restores desktop saturation, restarts `Chroma.Agent.exe`, and initializes the chosen backend.

## Improved

- Automatic mode now prioritizes the vendor driving the primary Windows display before trying other compatible backends.
- Explicit vendor choices no longer silently fall back to a different GPU vendor.
- A temporarily unavailable selected backend remains selected and is retried by the background agent.

## Test checklist

1. Open **Settings → Graphics processor**.
2. Confirm Automatic shows the primary active adapter.
3. Select the installed GPU vendor and confirm the agent reconnects.
4. Launch a configured game and confirm the saturation profile is applied.
5. Close the game and confirm desktop saturation returns to 100%.
6. Restart Chroma and confirm the selected GPU choice is preserved.

> The first selector targets GPU vendors. Selecting between two separate adapters from the same vendor will require a later per-adapter backend update.
