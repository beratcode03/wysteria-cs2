/* ==========================================================================
 * Wysteria Works - Jailbreak module.
 * Official Website: https://wysteriaworks.com
 * Powered by Wysteria Framework (CS2 / CounterStrikeSharp)
 * ========================================================================== */

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Wysteria.Core;

namespace Wysteria.GuardOrders;

public sealed class GuardOrdersPlugin : WysteriaPlugin<GuardOrdersConfig>
{
    static GuardOrdersPlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.GuardOrders";
    public override string ModuleVersion => "1.3.5";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Low-spam guard order system.";

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.GuardOrders.json");
        AddCommand("css_order", "Warden’ın CT ekibine tek satır, düşük spamli emir yayınlaması.", (_, info) =>
        {
            if (!TryGetPlayer(info, out var player)) return;
            if (!Config.Enabled || !IsMapAllowed()) return;
        if (!WysteriaRound.IsLive) { WysteriaMessages.Local(info.CallingPlayer!, "Bu özellik yalnızca canlı round sırasında kullanılabilir."); return; }
if (!Permit(info, player => WardenService.IsWarden(player), "Only the active Warden can use this.", Config.CooldownSeconds)) return;
            var order = string.Join(' ', Enumerable.Range(1, Math.Max(0, info.ArgCount - 1)).Select(i => info.GetArg(i)));
            if (string.IsNullOrWhiteSpace(order)) { WysteriaMessages.Local(player, "Kullanım: !order <emir>"); return; }
            if (order.Length > Config.MaxLength) order = order[..Config.MaxLength];
            
            var cts = Utilities.GetPlayers().Where(p => WysteriaMessages.IsUsable(p) && p.TeamNum == (int)CsTeam.CounterTerrorist);
            WysteriaMessages.Team(cts, $"[WARDEN EMRİ] {order}");
            foreach (var ct in cts)
            {
                WysteriaMessages.Center(ct, $"WARDEN EMRİ:\n{order}");
            }
        });
    }

}

public sealed class GuardOrdersConfig : WysteriaConfig
{
    public int CooldownSeconds { get; set; } = 8;
    public int MaxLength { get; set; } = 100;
}
