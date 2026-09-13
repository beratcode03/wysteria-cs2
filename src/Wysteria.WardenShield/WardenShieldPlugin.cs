/* ==========================================================================
 * Wysteria Works - Jailbreak module.
 * Official Website: https://wysteriaworks.com
 * Powered by Wysteria Framework (CS2 / CounterStrikeSharp)
 * ========================================================================== */

using System;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Wysteria.Core;

namespace Wysteria.WardenShield;

public sealed class WardenShieldPlugin : WysteriaPlugin<WardenShieldConfig>
{
    static WardenShieldPlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.WardenShield";
    public override string ModuleVersion => "1.1.7";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Temporary Warden energy shield.";

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.WardenShield.json");
        AddCommand("css_shield", "Aktif Warden’ın süreli enerji kalkanı kurması.", (_, info) =>
        {
            if (!TryGetPlayer(info, out var player)) return;
            if (!Config.Enabled || !IsMapAllowed()) return;
            if (!WysteriaRound.IsLive) { WysteriaMessages.Local(info.CallingPlayer!, "Bu özellik yalnızca canlı round sırasında kullanılabilir."); return; }
            if (!Permit(info, p => WardenService.IsWarden(p), "Only the active Warden can use this.", Config.CooldownSeconds)) return;
            
            var pawn = player.PlayerPawn?.Value;
            if (pawn == null || !pawn.IsValid || !player.PawnIsAlive) return;

            pawn.Health += Config.ShieldHealth;
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");
            
            // Visual feedback
            Adapter.MarkPlayer(player, "shield", Config.ShieldDurationSeconds);
            player.ExecuteClientCommand("play sounds/items/suitchargeok1.vsnd");
            
            WysteriaMessages.Public($"{player.PlayerName} enerji kalkanını kurdu. +{Config.ShieldHealth} HP!");
            
            AddTimer(Config.ShieldDurationSeconds, () =>
            {
                if (player != null && player.IsValid)
                {
                    var p = player.PlayerPawn?.Value;
                    if (p != null && p.IsValid && p.Health > 100)
                    {
                        p.Health = 100;
                        Utilities.SetStateChanged(p, "CBaseEntity", "m_iHealth");
                        WysteriaMessages.Local(player, "Kalkanın süresi doldu.");
                        Adapter.UnmarkPlayer(player);
                    }
                }
            });
        });
    }
}

public sealed class WardenShieldConfig : WysteriaConfig
{
    public int ShieldHealth { get; set; } = 500;
    public int ShieldDurationSeconds { get; set; } = 30;
    public int CooldownSeconds { get; set; } = 60;
}
