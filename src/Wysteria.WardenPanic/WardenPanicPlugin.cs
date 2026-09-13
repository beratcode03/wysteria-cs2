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

namespace Wysteria.WardenPanic;

public sealed class WardenPanicPlugin : WysteriaPlugin<WardenPanicConfig>
{
    static WardenPanicPlugin() => WysteriaCoreGuard.Require();
    private int _lastUsedRound;


    public override string ModuleName => "Wysteria.WardenPanic";
    public override string ModuleVersion => "1.0.6";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - One-use Warden emergency alarm.";

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.WardenPanic.json");
        AddCommand("css_panic", "Yalnızca Warden’ın tek kullanımlık alarm ve Alpha Guard çağrısı.", (_, info) =>
        {
            if (!TryGetPlayer(info, out var player)) return;
            if (!Config.Enabled || !IsMapAllowed()) return;
        if (!WysteriaRound.IsLive) { WysteriaMessages.Local(info.CallingPlayer!, "Bu özellik yalnızca canlı round sırasında kullanılabilir."); return; }
        if (!WysteriaRound.CellsOpen) { WysteriaMessages.Local(info.CallingPlayer!, "Panik butonu hücreler açıldıktan sonra kullanılabilir."); return; }
        if (WysteriaMessages.AliveOn(CsTeam.Terrorist) < Config.MinPrisonersRequired) { WysteriaMessages.Local(info.CallingPlayer!, $"En az {Config.MinPrisonersRequired} canlı mahkûm gerekli."); return; }
        if (_lastUsedRound == WysteriaRound.RoundNumber) { WysteriaMessages.Local(info.CallingPlayer!, "Panik butonu bu round zaten kullanıldı."); return; }
if (!Permit(info, player => WardenService.IsWarden(player), "Only the active Warden can use this.", Config.GlobalCooldown)) return;
            _lastUsedRound = WysteriaRound.RoundNumber;
            
            var pawn = player.PlayerPawn?.Value;
            if (pawn != null && pawn.IsValid && player.PawnIsAlive)
            {
                pawn.Health = Config.AlphaGuardHealth;
                Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");
                player.ExecuteClientCommand("play sounds/ambient/alarms/klaxon1.vsnd");
                Adapter.MarkPlayer(player, "panic", Config.RadarDurationSeconds);
            }
            WysteriaMessages.Public($"Warden panik butonunu etkinleştirdi! Özel muhafız canı ({Config.AlphaGuardHealth} HP) verildi ve radarda işaretlendi.");
        });
    }

}

public sealed class WardenPanicConfig : WysteriaConfig
{
    public int AlphaGuardHealth { get; set; } = 125;
    public float RadarDurationSeconds { get; set; } = 5.0f;
    public int MinPrisonersRequired { get; set; } = 3;
}
