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

namespace Wysteria.GuardJail;

public sealed class GuardJailPlugin : WysteriaPlugin<GuardJailConfig>
{
    static GuardJailPlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.GuardJail";
    public override string ModuleVersion => "1.0.8";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Temporary guard jail punishment.";

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.GuardJail.json");
        AddCommand("css_punish", "Warden’ın hatalı bir CT oyuncusunu geçici hücre cezasına göndermesi.", (_, info) =>
        {
            if (!TryGetPlayer(info, out var player)) return;
            if (!Config.Enabled || !IsMapAllowed()) return;
        if (!WysteriaRound.IsLive) { WysteriaMessages.Local(info.CallingPlayer!, "Bu özellik yalnızca canlı round sırasında kullanılabilir."); return; }
if (!Permit(info, player => WardenService.IsWarden(player), "Only the active Warden can use this.", Config.GlobalCooldown)) return;
            if (info.ArgCount < 2) { WysteriaMessages.Local(player, "Kullanım: !punish <oyuncu>"); return; }
            var target = WysteriaMessages.FindPlayer(info.GetArg(1));
            if (target is null || target.TeamNum != (int)CsTeam.CounterTerrorist || target.SteamID == player.SteamID) {
                WysteriaMessages.Local(player, "Geçerli bir CT hedefi bulunamadı."); return;
            }

            if (Config.StripWeapons)
            {
                target.RemoveWeapons();
                target.GiveNamedItem("weapon_knife");
            }

            if (target.PlayerPawn.Value != null)
            {
                target.PlayerPawn.Value.MoveType = MoveType_t.MOVETYPE_NONE;
            }

            WysteriaMessages.Public($"{player.PlayerName}, {target.PlayerName} adlı oyuncuyu {Config.JailDurationSeconds} saniyeliğine hücre cezasına çarptırdı.");

            AddTimer(Config.JailDurationSeconds, () =>
            {
                if (WysteriaMessages.IsUsable(target) && target.PlayerPawn.Value != null)
                {
                    target.PlayerPawn.Value.MoveType = MoveType_t.MOVETYPE_WALK;
                    WysteriaMessages.Public($"{target.PlayerName} hücresinden çıkarıldı.");
                }
            });
        });
    }

}

public sealed class GuardJailConfig : WysteriaConfig
{
    public int JailDurationSeconds { get; set; } = 60;
    public bool StripWeapons { get; set; } = true;
}
