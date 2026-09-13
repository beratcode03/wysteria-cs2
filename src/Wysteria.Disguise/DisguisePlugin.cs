/* ==========================================================================
 * Wysteria Works - Jailbreak module.
 * Official Website: https://wysteriaworks.com
 * Powered by Wysteria Framework (CS2 / CounterStrikeSharp)
 * ========================================================================== */

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using Wysteria.Core;

namespace Wysteria.Disguise;

public sealed class DisguisePlugin : WysteriaPlugin<DisguiseConfig>
{
    private readonly Dictionary<ulong, string> _active = new();

    static DisguisePlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.Disguise";
    public override string ModuleVersion => "1.0.8";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Temporary prisoner disguise system.";

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.Disguise.json");
        RegisterEventHandler<EventRoundEnd>((_, _) =>
        {
            foreach (var player in Utilities.GetPlayers())
                Restore(player);
            _active.Clear();
            return HookResult.Continue;
        });
        RegisterEventHandler<EventPlayerDeath>((@event, _) =>
        {
            if (@event.Userid is { IsValid: true } player)
                Restore(player);
            return HookResult.Continue;
        });
        RegisterEventHandler<EventPlayerDisconnect>((@event, _) =>
        {
            if (@event.Userid is { IsValid: true } player)
                _active.Remove(player.SteamID);
            return HookResult.Continue;
        });
        AddCommand("css_disguise", "Mahkûm için geçici gardiyan kılığı satın alır.", (_, info) =>
        {
            if (!TryGetPlayer(info, out var player)) return;
            if (!Config.Enabled || !IsMapAllowed()) return;
            if (!WysteriaRound.IsLive)
            {
                WysteriaMessages.Local(player, "Bu özellik yalnızca canlı round sırasında kullanılabilir.");
                return;
            }
            if (!Permit(info, IsPrisoner, "Bu kılığı yalnızca mahkûmlar satın alabilir."))
                return;
            if (_active.ContainsKey(player.SteamID))
            {
                WysteriaMessages.Local(player, "Zaten kılık değiştirmiş durumdasın.");
                return;
            }

            var pawn = player.PlayerPawn?.Value;
            if (pawn is null || !pawn.IsValid)
            {
                WysteriaMessages.Local(player, "Kılık şu anda uygulanamıyor.");
                return;
            }

            var result = WysteriaEconomy.Adapter.TryPurchase(
                new EconomyPurchaseRequest(player.SteamID, Config.ProductId, Math.Max(0, Config.DisguiseCost)));
            if (!result.Succeeded)
            {
                WysteriaMessages.Local(player, "Ekonomi entegrasyonu hazır değil veya yeterli krediniz yok.");
                return;
            }

            _active[player.SteamID] = Config.PrisonerModel;
            pawn.SetModel(Config.GuardModel);
            UseCooldown(player, Math.Max(0, Config.GlobalCooldown));
            WysteriaMessages.Local(player, $"Kılık aktif. Süre: {Config.DisguiseDuration:0.0} saniye.");
            AddTimer(Math.Max(0.1f, Config.DisguiseDuration), () => Restore(player));
        });
    }

    private void Restore(CCSPlayerController player)
    {
        if (!_active.Remove(player.SteamID) || !player.IsValid) return;
        var pawn = player.PlayerPawn?.Value;
        if (pawn is { IsValid: true }) pawn.SetModel(Config.PrisonerModel);
        WysteriaMessages.Local(player, "Kılık sona erdi.");
    }
}

public sealed class DisguiseConfig : WysteriaConfig
{
    public int DisguiseCost { get; set; } = 3000;
    public float DisguiseDuration { get; set; } = 45.0f;
    public string ProductId { get; set; } = "disguise";
    public string GuardModel { get; set; } = "characters/models/ctm_sas/ctm_sas.vmdl";
    public string PrisonerModel { get; set; } = "characters/models/tm_phoenix/tm_phoenix.vmdl";
}
