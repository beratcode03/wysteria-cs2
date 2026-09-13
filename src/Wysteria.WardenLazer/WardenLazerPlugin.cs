/* ==========================================================================
 * Wysteria Works - Jailbreak module.
 * Official Website: https://wysteriaworks.com
 * Powered by Wysteria Framework (CS2 / CounterStrikeSharp)
 * ========================================================================== */

using System;
using System.Collections.Generic;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Timers;
using Wysteria.Core;

namespace Wysteria.WardenLazer;

public sealed class WardenLazerConfig : WysteriaConfig
{
    public float LazerDuration { get; set; } = 30.0f;
    public string LazerColor { get; set; } = "128 0 128"; // Purple
    public float CooldownSeconds { get; set; } = 2.0f;
}

public sealed class WardenLazerPlugin : WysteriaPlugin<WardenLazerConfig>
{
    static WardenLazerPlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.WardenLazer";
    public override string ModuleVersion => "1.6.5";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Warden laser marker system.";

    private readonly object _stateLock = new();
    private Dictionary<uint, List<CHandle<CBaseEntity>>> _lazerEntities = new();
    private Dictionary<uint, float> _cooldowns = new();

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.WardenLazer.json");

        AddCommand("css_wysteria_lazer", "Draws a laser point at aim", OnLazerCommand);
        AddCommand("css_wysteria_lazer_clear", "Clears all your lasers", OnLazerClearCommand);
        AddCommand("css_wysteria_lazer_menu", "Opens the secret config menu for root admins", OnSecretMenuCommand);

        AddCommand("wysteria_lazer", "Draws a laser point at aim", OnLazerCommand);
        AddCommand("wysteria_lazer_clear", "Clears all your lasers", OnLazerClearCommand);
        AddCommand("wysteria_lazer_menu", "Opens the secret config menu for root admins", OnSecretMenuCommand);

        RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
    }

    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        ClearAllLazers();
        return HookResult.Continue;
    }

    private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        if (@event.Userid != null)
        {
            ClearPlayerLazers(@event.Userid.Index);
        }
        return HookResult.Continue;
    }

    private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        if (@event.Userid != null)
        {
            ClearPlayerLazers(@event.Userid.Index);
        }
        return HookResult.Continue;
    }

    private void ClearAllLazers()
    {
        lock (_stateLock)
        {
            foreach (var kvp in _lazerEntities)
            {
                foreach (var handle in kvp.Value)
                {
                    if (handle.IsValid && handle.Value != null)
                    {
                        handle.Value.AcceptInput("Kill");
                    }
                }
            }
            _lazerEntities.Clear();
            _cooldowns.Clear();
        }
    }

    private void ClearPlayerLazers(uint playerIndex)
    {
        lock (_stateLock)
        {
            if (_lazerEntities.TryGetValue(playerIndex, out var list))
            {
                foreach (var handle in list)
                {
                    if (handle.IsValid && handle.Value != null)
                    {
                        handle.Value.AcceptInput("Kill");
                    }
                }
                list.Clear();
            }
        }
    }

    private void OnLazerCommand(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;
        if (!Config.Enabled || !IsMapAllowed()) return;

        if (!WardenService.IsWarden(player) && !AdminManager.PlayerHasPermissions(player, "@css/root"))
        {
            WysteriaMessages.Local(player, "Lazer çizmek için Warden veya Root yetkisine sahip olmalısınız.");
            return;
        }

        float currentTime = Server.CurrentTime;
        lock (_stateLock)
        {
            if (_cooldowns.TryGetValue(player.Index, out float lastTime))
            {
                if (currentTime - lastTime < Config.CooldownSeconds)
                {
                    WysteriaMessages.Local(player, $"Çok hızlı kullanıyorsunuz. Lütfen bekleyin.");
                    return;
                }
            }
            _cooldowns[player.Index] = currentTime;
        }

        var pawn = player.PlayerPawn.Value;
        if (pawn == null || !pawn.IsValid || !player.PawnIsAlive) return;

        Vector startPos = pawn.AbsOrigin != null ? new Vector(pawn.AbsOrigin.X, pawn.AbsOrigin.Y, pawn.AbsOrigin.Z + 64.0f) : new Vector(0,0,0);
        QAngle angles = pawn.EyeAngles != null ? new QAngle(pawn.EyeAngles.X, pawn.EyeAngles.Y, pawn.EyeAngles.Z) : new QAngle(0,0,0);
        
        double pitch = angles.X * Math.PI / 180.0;
        double yaw = angles.Y * Math.PI / 180.0;
        float fwdX = (float)(Math.Cos(pitch) * Math.Cos(yaw));
        float fwdY = (float)(Math.Cos(pitch) * Math.Sin(yaw));
        float fwdZ = (float)(-Math.Sin(pitch));

        float distance = 800.0f; // Fixed distance since CS2 native RayTrace is not standard yet
        Vector endPos = new Vector(startPos.X + fwdX * distance, startPos.Y + fwdY * distance, startPos.Z + fwdZ * distance);

        var endTarget = Utilities.CreateEntityByName<CBaseEntity>("info_target");
        if (endTarget != null)
        {
            endTarget.Teleport(endPos, angles, null);
            endTarget.DispatchSpawn();
            
            string targetName = $"lazer_end_{player.Index}_{Server.TickCount}";
            endTarget.AcceptInput("AddOutput", null, null, $"targetname {targetName}");

            var beam = Utilities.CreateEntityByName<CBeam>("env_beam");
            if (beam != null)
            {
                beam.Teleport(startPos, angles, null);
                beam.AcceptInput("AddOutput", null, null, $"LightningEnd {targetName}");
                beam.DispatchSpawn();
                beam.AcceptInput("AddOutput", null, null, $"rendercolor {Config.LazerColor}");
                
                var hBeam = new CHandle<CBaseEntity>((nint)beam.Index);
                var hTarget = new CHandle<CBaseEntity>((nint)endTarget.Index);

                lock (_stateLock)
                {
                    if (!_lazerEntities.ContainsKey(player.Index))
                    {
                        _lazerEntities[player.Index] = new List<CHandle<CBaseEntity>>();
                    }
                    
                    _lazerEntities[player.Index].Add(hBeam);
                    _lazerEntities[player.Index].Add(hTarget);

                    AddTimer(Config.LazerDuration, () =>
                    {
                        if (hBeam.IsValid && hBeam.Value != null) hBeam.Value.AcceptInput("Kill");
                        if (hTarget.IsValid && hTarget.Value != null) hTarget.Value.AcceptInput("Kill");
                    });
                }
                
                WysteriaMessages.Center(player, "Lazer Noktası Eklendi!");
                player.ExecuteClientCommand("play sounds/buttons/blip1.vsnd");
            }
            else
            {
                // Partial creation failure, cleanup target
                endTarget.AcceptInput("Kill");
            }
        }
    }

    private void OnLazerClearCommand(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;
        if (!Config.Enabled || !IsMapAllowed()) return;

        ClearPlayerLazers(player.Index);

        WysteriaMessages.Local(player, "Çizdiğiniz lazerler temizlendi.");
        player.ExecuteClientCommand("play sounds/buttons/blip1.vsnd");
    }

    private void OnSecretMenuCommand(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !AdminManager.PlayerHasPermissions(player, "@css/root")) return;

        var menu = new ChatMenu("Gizli Lazer Ayarları");
        menu.AddMenuOption($"Süre (+5s): {Config.LazerDuration}s", (p, option) =>
        {
            Config.LazerDuration += 5.0f;
            WysteriaMessages.Local(p, $"Lazer süresi {Config.LazerDuration}s olarak güncellendi.");
        });
        menu.AddMenuOption($"Süre (-5s)", (p, option) =>
        {
            Config.LazerDuration = Math.Max(5.0f, Config.LazerDuration - 5.0f);
            WysteriaMessages.Local(p, $"Lazer süresi {Config.LazerDuration}s olarak güncellendi.");
        });

        MenuManager.OpenChatMenu(player, menu);
    }
}
