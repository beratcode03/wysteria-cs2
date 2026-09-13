# Wysteria CS2 release checklist

- [ ] Repository root is `Wysteria CS2 Plugins/`; no extra nested repository
      folder is pushed.
- [ ] `Plugins/` contains `Wysteria.Core.dll` plus the 30 module DLL files.
- [ ] `Wysteria Core/` contains `Wysteria.Core.dll`.
- [ ] `Configs/` contains exactly the 30 module JSON files.
- [ ] `Configs/Wysteria.Advertiser.json` remains disabled unless an explicit
      URL announcement is wanted.
- [ ] GitHub Actions build is green.
- [ ] The build artifact contains `plugins/Wysteria/Wysteria.Core.dll`,
      all module DLLs, and all 30 JSON files.
- [ ] Target CS2 server has the matching CounterStrikeSharp With Runtime and
      Metamod:Source 2.x.
- [ ] Core and selected modules appear in `meta list` and
      `css_plugins list` after a full restart.
- [ ] Unauthorized LOCAL, TEAM, CENTER, and PUBLIC messages were tested.
- [ ] Real JailShop, CellSearch, WardenManager, entity, stun, teleport, and
      door adapters were tested before advertising those physical gameplay
      effects as production-ready.
