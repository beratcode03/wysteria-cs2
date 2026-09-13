# Wysteria CS2 Jailbreak - Sound & Effect Audit

## Sounds
Wysteria avoids requiring the end-user to download custom assets unless absolutely necessary.
We rely heavily on native Counter-Strike 2/Source 2 soundscapes and UI sounds.
- Success / Menus: `sounds/ui/menu_accept.vsnd`
- Errors / Denies: `sounds/buttons/blip1.vsnd`
- Alarms: Native CS2 alarm entities or `sounds/ambient/alarms/`

## Effects (Particles & Overlays)
We refrain from spawning continuous heavy particles per frame.
- **WardenLazer**: Uses optimized Source 2 entity properties (`rendermode 1`, etc.) or native debug overlay rendering, avoiding the allocation of multiple particle entities that lead to FPS drops.
- **Cleanup**: `EventRoundStart`, `EventRoundEnd`, and `MapStart` hooks ensure that dynamic entities, lazers, and effects are securely cleaned up to prevent memory leaks or cross-round persistent ghosting.
