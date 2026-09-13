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

namespace Wysteria.FreeDay;

public sealed class FreeDayPlugin : WysteriaPlugin<FreeDayConfig>
{
    static FreeDayPlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.FreeDay";
    public override string ModuleVersion => "1.0.5";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Warden free-day assignment system.";

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.FreeDay.json");
        AddCommand("css_freeday", "Warden’ın tek bir mahkûma round içi serbest gün vermesi.", (_, info) =>
        {
            if (!TryGetPlayer(info, out var player)) return;
            if (!Config.Enabled || !IsMapAllowed()) return;
            if (!WysteriaRound.IsLive) { WysteriaMessages.Local(info.CallingPlayer!, "Bu özellik yalnızca canlı round sırasında kullanılabilir."); return; }
            if (!Permit(info, p => WardenService.IsWarden(p), "Only the active Warden can use this.", Config.CooldownSeconds)) return;

            if (info.ArgCount < 2)
            {
                WysteriaMessages.Local(player, "Kullanım: !freeday <oyuncu_adı>");
                return;
            }

            var targetName = info.GetArg(1).ToLower();
            var target = Utilities.GetPlayers().FirstOrDefault(p => p.IsValid && !p.IsBot && p.PlayerName.ToLower().Contains(targetName) && p.TeamNum == (int)CsTeam.Terrorist && p.PawnIsAlive);

            if (target == null)
            {
                WysteriaMessages.Local(player, "Geçerli, hayatta olan bir mahkûm bulunamadı.");
                return;
            }

            // Real logic: Apply freeday state to the target
            target.PlayerPawn.Value!.Render = System.Drawing.Color.Green;
            WysteriaMessages.Public($"{player.PlayerName}, {target.PlayerName} adlı mahkûma {Config.DurationSeconds} saniyelik serbest gün verdi!");

            AddTimer(Config.DurationSeconds, () =>
            {
                if (target.IsValid && target.PawnIsAlive)
                {
                    target.PlayerPawn.Value!.Render = System.Drawing.Color.White;
                    WysteriaMessages.Local(target, "Serbest günün sona erdi.");
                }
            });
        });
    }

}

public sealed class FreeDayConfig : WysteriaConfig
{
    public int DurationSeconds { get; set; } = 45;
    public int CooldownSeconds { get; set; } = 30;
}
