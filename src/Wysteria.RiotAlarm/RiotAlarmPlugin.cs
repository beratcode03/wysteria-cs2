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

namespace Wysteria.RiotAlarm;

public sealed class RiotAlarmPlugin : WysteriaPlugin<RiotAlarmConfig>
{
    static RiotAlarmPlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.RiotAlarm";
    public override string ModuleVersion => "1.1.6";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Local riot alert for the Warden.";

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.RiotAlarm.json");
        AddCommand("css_riotalarm", "Cezaevinde isyan alarmı (Guardlara özel).", OnRiotAlarmCommand);
        AddCommand("css_wysteria_riotalarm", "Cezaevinde isyan alarmı (Guardlara özel).", OnRiotAlarmCommand);
    }

    private void OnRiotAlarmCommand(CCSPlayerController? caller, CommandInfo info)
    {
        if (!TryGetPlayer(info, out var player)) return;
        if (!Config.Enabled || !IsMapAllowed()) return;
        if (!WysteriaRound.IsLive) { WysteriaMessages.Local(player, "Bu özellik yalnızca canlı round sırasında kullanılabilir."); return; }
        if (!player.PawnIsAlive) { WysteriaMessages.Local(player, "Bu komutu kullanmak için hayatta olmalısın."); return; }
        if (!Permit(info, p => p.TeamNum == (int)CsTeam.CounterTerrorist, "Bu komutu yalnızca gardiyanlar kullanabilir.", Config.CooldownSeconds)) return;

        var cts = Utilities.GetPlayers().Where(p => p.TeamNum == (int)CsTeam.CounterTerrorist).ToList();
        WysteriaMessages.Team(cts, $"CT alarmı: {player.PlayerName} şüpheli isyan bildirdi.");
        foreach (var ct in cts)
        {
            if (ct.IsValid)
            {
                {
                    ct.ExecuteClientCommand("play sounds/ui/armsrace_level_up.vsnd_c");
                    ct.PrintToCenterHtml($"<font color='red'><b>DİKKAT: {player.PlayerName} isyan bildirdi!</b></font>");
                }
            }
        }
    }

}

public sealed class RiotAlarmConfig : WysteriaConfig
{
    public int CooldownSeconds { get; set; } = 30;
    public int MinimumPrisoners { get; set; } = 2;
}
