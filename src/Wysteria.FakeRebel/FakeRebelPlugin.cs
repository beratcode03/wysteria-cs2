/* ==========================================================================
 * Wysteria Works - Jailbreak module.
 * Official Website: https://wysteriaworks.com
 * Powered by Wysteria Framework (CS2 / CounterStrikeSharp)
 * ========================================================================== */

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Wysteria.Core;

namespace Wysteria.FakeRebel;

public sealed class FakeRebelPlugin : WysteriaPlugin<FakeRebelConfig>
{
    static FakeRebelPlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.FakeRebel";
    public override string ModuleVersion => "1.1.6";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Local fake rebellion sound system.";

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.FakeRebel.json");
        AddCommand("css_fakerebel", "Mahkûmun bulunduğu noktada sahte isyan sesleri oluşturması.", OnFakeRebelCommand);
        AddCommand("css_wysteria_fakerebel", "Mahkûmun bulunduğu noktada sahte isyan sesleri oluşturması.", OnFakeRebelCommand);
    }

    private void OnFakeRebelCommand(CCSPlayerController? caller, CommandInfo info)
    {
        if (!TryGetPlayer(info, out var player)) return;
        if (!Config.Enabled || !IsMapAllowed()) return;
        
        if (!WysteriaRound.IsLive)
        {
            WysteriaMessages.Local(player, "Bu özellik yalnızca canlı round sırasında kullanılabilir.");
            return;
        }

        if (!player.PawnIsAlive)
        {
            WysteriaMessages.Local(player, "Bu komutu kullanmak için hayatta olmalısın.");
            return;
        }

        if (!Permit(info, prisoner => prisoner.TeamNum == (int)CsTeam.Terrorist,
                "Bu komutu yalnızca mahkûmlar kullanabilir."))
            return;
            
        if (info.ArgCount > 2)
        {
            WysteriaMessages.Local(player, "Kullanım: !fakerebel [gunshot|footstep|vent]");
            return;
        }

        var cue = info.ArgCount == 2 ? info.GetArg(1).ToLowerInvariant() : "gunshot";
        if (!Config.Cues.TryGetValue(cue, out var sound))
        {
            WysteriaMessages.Local(player,
                $"Geçersiz ses türü. Seçenekler: {string.Join(", ", Config.Cues.Keys)}.");
            return;
        }

        var origin = player.PlayerPawn?.Value?.AbsOrigin;
        if (origin is null)
        {
            WysteriaMessages.Local(player, "Ses şu anda oluşturulamıyor.");
            return;
        }

        var result = WysteriaEconomy.Adapter.TryPurchase(new EconomyPurchaseRequest(
            player.SteamID, Config.ProductId, Math.Max(0, Config.FakeRebelCost)));
        if (!result.Succeeded)
        {
            WysteriaMessages.Local(player, "Satın alma başarısız; ekonomi entegrasyonu hazır değil veya kredin yetersiz.");
            return;
        }

        UseCooldown(player, Math.Max(0, Config.GlobalCooldown));
        var radius = Math.Max(0, Config.AudioRadius);
        foreach (var target in Utilities.GetPlayers().Where(IsGuard).Where(IsAlive))
        {
            var targetOrigin = target.PlayerPawn?.Value?.AbsOrigin;
            if (targetOrigin is not null &&
                WysteriaGeometry.DistanceSquared(origin, targetOrigin) <=
                radius * radius)
                Adapter.PlaySound(target, sound);
        }
        WysteriaMessages.Local(player, $"{cue} ses ipucu oluşturuldu.");
    }
}

public sealed class FakeRebelConfig : WysteriaConfig
{
    public int FakeRebelCost { get; set; } = 100;
    public float AudioRadius { get; set; } = 500.0f;
    public string ProductId { get; set; } = "fake-rebel";
    public Dictionary<string, string> Cues { get; set; } = new()
    {
        ["gunshot"] = "Weapon_AK47.Single",
        ["footstep"] = "Player.Footsteps",
        ["vent"] = "Breakable.Metal"
    };
}
