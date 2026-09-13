# Wysteria Core Integration Contract

All modules depend on the following Core-owned contracts:

- `WysteriaRound.IsLive`, `CellsOpen`, `RoundNumber`
- `WardenService.IsWarden`, `Claim`, `Reset`
- `WysteriaMessages.Local`, `Team`, `Public`, `Center`
- `ConfigStore` and `WysteriaPlugin<TConfig>`
- `IEconomyAdapter`, `EconomyPurchaseRequest`, `EconomyPurchaseResult`
  and `WysteriaEconomy.RegisterAdapter(...)`
- `IJailbreakAdapter` and `WysteriaPlugin.SetAdapter(...)` for the additional
  economy, contraband, cell, door, sound, and player-marking modules

Do not duplicate Warden state or round lifecycle in individual modules.

The original specification did not include the target server's JailShop,
CellSearch, WardenManager, entity schemas or existing source. Therefore
modules expose secure command boundaries but do not invent unsafe physical
entity/economy calls. `WysteriaEconomy` starts with an unavailable adapter, so
a shop command cannot claim success without a real integration. Register the
target server's JailShop implementation through
`WysteriaEconomy.RegisterAdapter(...)` once its API signatures are available.

The additional modules use `IJailbreakAdapter`. The built-in
`UnavailableJailbreakAdapter` is intentionally a no-op for credits, contraband,
doors, sounds, and player markers; it also reports no credit balance. Register
a real implementation through `WysteriaPlugin.SetAdapter(...)` only after the
target server API has been verified.
