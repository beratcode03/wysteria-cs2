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

namespace Wysteria.TeamBalance;

public sealed class TeamBalancePlugin : WysteriaPlugin<TeamBalanceConfig>
{
    static TeamBalancePlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.TeamBalance";
    public override string ModuleVersion => "1.0.9";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Warden team balance reporting.";

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.TeamBalance.json");
        AddCommand("css_balance", "Aşırı dengesiz roundlarda Warden’a özel durum raporu.", (_, info) =>
        {
            if (!TryGetPlayer(info, out var player)) return;
            if (!Config.Enabled || !IsMapAllowed()) return;
        if (!WysteriaRound.IsLive) { WysteriaMessages.Local(info.CallingPlayer!, "Bu özellik yalnızca canlı round sırasında kullanılabilir."); return; }
if (!Permit(info, player => WardenService.IsWarden(player), "Only the active Warden can use this.", Config.CooldownSeconds)) return;
            var cts = Utilities.GetPlayers().Count(p => p.IsValid && !p.IsBot && p.TeamNum == (byte)CsTeam.CounterTerrorist && p.PawnIsAlive);
            var ts = Utilities.GetPlayers().Count(p => p.IsValid && !p.IsBot && p.TeamNum == (byte)CsTeam.Terrorist && p.PawnIsAlive);
            var diff = Math.Abs(ts - cts);
            if (diff > Config.MaxTeamDifference)
            {
                WysteriaMessages.Local(player, $"Takımlar DENGESİZ! CT: {cts} | T: {ts} (Fark: {diff} > İzin verilen: {Config.MaxTeamDifference})");
            }
            else
            {
                WysteriaMessages.Local(player, $"Takımlar dengeli görünüyor. CT: {cts} | T: {ts} (İzin verilen fark: {Config.MaxTeamDifference})");
            }
        });
    }

}

public sealed class TeamBalanceConfig : WysteriaConfig
{
    public int MaxTeamDifference { get; set; } = 3;
    public int CooldownSeconds { get; set; } = 20;
}
