/* ==========================================================================
 * Wysteria Works - Jailbreak module.
 * Official Website: https://wysteriaworks.com
 * Powered by Wysteria Framework (CS2 / CounterStrikeSharp)
 * ========================================================================== */

using System;
using System.Collections.Generic;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Wysteria.Core;

namespace Wysteria.WardenDraw;

public sealed class WardenDrawPlugin : WysteriaPlugin<WardenDrawConfig>
{
    static WardenDrawPlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.WardenDraw";
    public override string ModuleVersion => "1.0.7";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Warden direction marker tool.";

    private readonly Dictionary<uint, CHandle<CBaseEntity>> _activeLazer = new();

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.WardenDraw.json");
        
        RegisterEventHandler<EventRoundEnd>((_, _) =>
        {
            foreach (var kvp in _activeLazer)
            {
                if (kvp.Value.IsValid && kvp.Value.Value != null)
                {
                    kvp.Value.Value.AcceptInput("Kill");
                }
            }
            _activeLazer.Clear();
            return HookResult.Continue;
        });

        AddCommand("css_draw", "Aktif Warden’ın yönlendirme lazerini açıp kapatması.", (_, info) =>
        {
            if (!TryGetPlayer(info, out var player)) return;
            if (!Config.Enabled || !IsMapAllowed()) return;
            if (!WysteriaRound.IsLive) { WysteriaMessages.Local(info.CallingPlayer!, "Bu özellik yalnızca canlı round sırasında kullanılabilir."); return; }
            if (!Permit(info, p => WardenService.IsWarden(p), "Only the active Warden can use this.", Config.GlobalCooldown)) return;
            
            if (_activeLazer.TryGetValue(player.Index, out var existing))
            {
                if (existing.IsValid && existing.Value != null)
                {
                    existing.Value.AcceptInput("Kill");
                }
                _activeLazer.Remove(player.Index);
                WysteriaMessages.Local(player, "Lazer kapatıldı.");
                return;
            }

            var pawn = player.PlayerPawn?.Value;
            if (pawn == null || !pawn.IsValid || !player.PawnIsAlive) return;

            Vector startPos = new Vector(pawn.AbsOrigin!.X, pawn.AbsOrigin.Y, pawn.AbsOrigin.Z + 64.0f);
            QAngle angles = new QAngle(pawn.EyeAngles!.X, pawn.EyeAngles.Y, pawn.EyeAngles.Z);

            var endPos = new Vector(startPos.X + (float)(Math.Cos(pawn.EyeAngles!.X * Math.PI / 180.0) * Math.Cos(pawn.EyeAngles.Y * Math.PI / 180.0)) * 800.0f,
                                    startPos.Y + (float)(Math.Cos(pawn.EyeAngles.X * Math.PI / 180.0) * Math.Sin(pawn.EyeAngles.Y * Math.PI / 180.0)) * 800.0f,
                                    startPos.Z - (float)(Math.Sin(pawn.EyeAngles.X * Math.PI / 180.0)) * 800.0f);

            var endTarget = Utilities.CreateEntityByName<CBaseEntity>("info_target");
            if (endTarget != null)
            {
                endTarget.Teleport(endPos, angles, null);
                endTarget.DispatchSpawn();
                string targetName = $"draw_end_{player.Index}_{Server.TickCount}";
                endTarget.AcceptInput("AddOutput", null, null, $"targetname {targetName}");

                var beam = Utilities.CreateEntityByName<CBeam>("env_beam");
                if (beam != null)
                {
                    beam.Teleport(startPos, angles, null);
                    beam.AcceptInput("AddOutput", null, null, $"LightningEnd {targetName}");
                    beam.DispatchSpawn();
                    beam.AcceptInput("AddOutput", null, null, $"rendercolor {Config.LaserColor}");
                    
                    _activeLazer[player.Index] = new CHandle<CBaseEntity>((nint)beam.Index);
                    WysteriaMessages.Local(player, $"Lazer çizimi {Config.LaserColor} olarak açıldı ({Config.LaserDuration:0.0}s).");
                    
                    AddTimer(Config.LaserDuration, () =>
                    {
                        if (_activeLazer.TryGetValue(player.Index, out var h) && h.IsValid && h.Value != null)
                        {
                            h.Value.AcceptInput("Kill");
                            _activeLazer.Remove(player.Index);
                            if (player.IsValid) WysteriaMessages.Local(player, "Lazer süresi doldu.");
                        }
                        if (endTarget.IsValid) endTarget.AcceptInput("Kill");
                    });
                }
            }
        });
    }
}

public sealed class WardenDrawConfig : WysteriaConfig
{
    public float LaserDuration { get; set; } = 15.0f;
    public string LaserColor { get; set; } = "Red";
}
