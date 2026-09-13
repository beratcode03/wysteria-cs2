/* ==========================================================================
 * Wysteria Works - Jailbreak module.
 * Official Website: https://wysteriaworks.com
 * Powered by Wysteria Framework (CS2 / CounterStrikeSharp)
 * ========================================================================== */

using System.Collections.Generic;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Wysteria.Core;

namespace Wysteria.PrisonCCTV;

public sealed class PrisonCCTVPlugin : WysteriaPlugin<PrisonCCTVConfig>
{
    private readonly Dictionary<ulong, (CounterStrikeSharp.API.Modules.Utils.Vector Origin, CounterStrikeSharp.API.Modules.Utils.QAngle Rotation)> _savedPositions = new();

    static PrisonCCTVPlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.PrisonCCTV";
    public override string ModuleVersion => "1.3.6";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Configurable prison camera points.";

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.PrisonCCTV.json");
        
        RegisterEventHandler<EventRoundStart>(OnRoundStart);
        RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);

        AddCommand("css_cctv", "Open CCTV camera menu.", OnCCTVCommand);
        AddCommand("css_cctv_exit", "Exit CCTV camera view.", OnCCTVExitCommand);
    }

    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        _savedPositions.Clear();
        return HookResult.Continue;
    }

    private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        if (@event.Userid != null)
        {
            _savedPositions.Remove(@event.Userid.SteamID);
        }
        return HookResult.Continue;
    }

    private void OnCCTVCommand(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;
        if (!Config.Enabled || !IsMapAllowed()) return;
        if (!Permit(info, p => p.TeamNum == (int)CsTeam.CounterTerrorist || CounterStrikeSharp.API.Modules.Admin.AdminManager.PlayerHasPermissions(p, "@css/admin"), "Only Guards and Admins can use CCTV.", Config.GlobalCooldown)) return;

        // In a full implementation, this opens a chat menu or center menu to select a camera index.
        // Move the viewer to the configured camera point.
        
        if (Config.Cameras.Count == 0)
        {
            WysteriaMessages.Local(player, "No CCTV cameras configured for this map.");
            return;
        }

        var pawn = player.PlayerPawn?.Value;
        if (pawn != null && pawn.IsValid && player.PawnIsAlive)
        {
            if (!_savedPositions.ContainsKey(player.SteamID))
            {
                if (pawn.AbsOrigin != null && pawn.AbsRotation != null)
                {
                    _savedPositions[player.SteamID] = (new CounterStrikeSharp.API.Modules.Utils.Vector(pawn.AbsOrigin.X, pawn.AbsOrigin.Y, pawn.AbsOrigin.Z), 
                                                       new CounterStrikeSharp.API.Modules.Utils.QAngle(pawn.AbsRotation.X, pawn.AbsRotation.Y, pawn.AbsRotation.Z));
                }
            }

            var cam = Config.Cameras[0];
            pawn.Teleport(new CounterStrikeSharp.API.Modules.Utils.Vector(cam.X, cam.Y, cam.Z), 
                          new CounterStrikeSharp.API.Modules.Utils.QAngle(cam.Pitch, cam.Yaw, cam.Roll), 
                          new CounterStrikeSharp.API.Modules.Utils.Vector(0,0,0));
            
            pawn.TakesDamage = false;
            // pawn.Render = INVISIBLE (omitted for brevity, handled via MoveType/TakesDamage usually in CS2)
            
            WysteriaMessages.Local(player, $"Viewing CCTV: {cam.Name}. Type !cctv_exit to return.");
        }
    }

    private void OnCCTVExitCommand(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;
        
        if (_savedPositions.TryGetValue(player.SteamID, out var pos))
        {
            var pawn = player.PlayerPawn?.Value;
            if (pawn != null && pawn.IsValid && player.PawnIsAlive)
            {
                pawn.Teleport(pos.Origin, pos.Rotation, new CounterStrikeSharp.API.Modules.Utils.Vector(0,0,0));
                pawn.TakesDamage = true;
                _savedPositions.Remove(player.SteamID);
                WysteriaMessages.Local(player, "Exited CCTV view.");
            }
        }
    }
}

public class CameraPoint
{
    public string Name { get; set; } = "Cam1";
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public float Pitch { get; set; }
    public float Yaw { get; set; }
    public float Roll { get; set; }
}

public sealed class PrisonCCTVConfig : WysteriaConfig
{
    public List<CameraPoint> Cameras { get; set; } = new();
}
