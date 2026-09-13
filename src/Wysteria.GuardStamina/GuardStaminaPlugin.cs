/* ==========================================================================
 * Wysteria Works - Jailbreak module.
 * Official Website: https://wysteriaworks.com
 * Powered by Wysteria Framework (CS2 / CounterStrikeSharp)
 * ========================================================================== */

using System.Collections.Generic;
using System.Linq;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Wysteria.Core;

namespace Wysteria.GuardStamina;

public sealed class GuardStaminaPlugin : WysteriaPlugin<GuardStaminaConfig>
{
    private readonly Dictionary<ulong, int> _charges = new();
    private readonly Dictionary<uint, (CHandle<CCSPlayerPawn> Pawn, CounterStrikeSharp.API.Modules.Timers.Timer Timer)> _activeStuns = new();
    private readonly object _stateLock = new();

    static GuardStaminaPlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.GuardStamina";
    public override string ModuleVersion => "1.5.3";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Limited guard stun ability.";

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.GuardStamina.json");
        
        RegisterEventHandler<EventRoundStart>(OnRoundStart);
        RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);

        AddCommand("css_stamina", "Gardiyanlara öldürmeyen, sınırlı şarjlı stun yeteneği.", OnStaminaCommand);
    }

    public override void Unload(bool hotReload)
    {
        ClearAllStuns();
        base.Unload(hotReload);
    }

    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        _charges.Clear();
        ClearAllStuns();
        return HookResult.Continue;
    }

    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        ClearAllStuns();
        return HookResult.Continue;
    }

    private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        if (@event.Userid != null)
        {
            ClearStun(@event.Userid.Index);
        }
        return HookResult.Continue;
    }

    private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        if (@event.Userid != null)
        {
            ClearStun(@event.Userid.Index);
        }
        return HookResult.Continue;
    }

    private void ClearAllStuns()
    {
        lock (_stateLock)
        {
            foreach (var kvp in _activeStuns)
            {
                var stun = kvp.Value;
                stun.Timer?.Kill();
                
                if (stun.Pawn.IsValid && stun.Pawn.Value != null)
                {
                    stun.Pawn.Value.VelocityModifier = 1.0f;
                }
            }
            _activeStuns.Clear();
        }
    }

    private void ClearStun(uint playerIndex)
    {
        lock (_stateLock)
        {
            if (_activeStuns.TryGetValue(playerIndex, out var stun))
            {
                stun.Timer?.Kill();
                
                if (stun.Pawn.IsValid && stun.Pawn.Value != null)
                {
                    stun.Pawn.Value.VelocityModifier = 1.0f;
                }
                
                _activeStuns.Remove(playerIndex);
            }
        }
    }

    private void OnStaminaCommand(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;
        if (!Config.Enabled || !IsMapAllowed()) return;
        if (!WysteriaRound.IsLive) { WysteriaMessages.Local(player, "Bu özellik yalnızca canlı round sırasında kullanılabilir."); return; }
        if (!Permit(info, p => p.TeamNum == (int)CsTeam.CounterTerrorist, "Bu komutu yalnızca gardiyanlar kullanabilir.", Config.GlobalCooldown)) return;

        if (!_charges.TryGetValue(player.SteamID, out var charges))
            charges = Config.TaserCharges;

        if (charges <= 0)
        {
            WysteriaMessages.Local(player, "Şok tabancası şarjı bitti.");
            return;
        }

        var origin = player.PlayerPawn?.Value?.AbsOrigin;
        if (origin == null) return;

        var target = Utilities.GetPlayers()
            .Where(p => p.TeamNum == (int)CsTeam.Terrorist && p.PlayerPawn?.Value != null && p.PawnIsAlive)
            .Where(t => t.PlayerPawn?.Value?.AbsOrigin != null && WysteriaGeometry.DistanceSquared(t.PlayerPawn.Value.AbsOrigin, origin) < 300.0f * 300.0f)
            .FirstOrDefault();

        if (target != null)
        {
            var pawn = target.PlayerPawn?.Value;
            if (pawn != null) 
            {
                var hPawn = new CHandle<CCSPlayerPawn>((nint)pawn.Index);
                uint tIndex = target.Index;

                lock (_stateLock)
                {
                    if (_activeStuns.TryGetValue(tIndex, out var existingStun))
                    {
                        existingStun.Timer?.Kill();
                    }

                    pawn.VelocityModifier = Config.SlowdownMultiplier;
                    
                    var timer = AddTimer(Config.StunDurationSeconds, () => {
                        lock (_stateLock)
                        {
                            if (_activeStuns.TryGetValue(tIndex, out var currentStun))
                            {
                                if (currentStun.Pawn.IsValid && currentStun.Pawn.Value != null)
                                {
                                    currentStun.Pawn.Value.VelocityModifier = 1.0f;
                                }
                                _activeStuns.Remove(tIndex);
                            }
                        }
                    });

                    _activeStuns[tIndex] = (hPawn, timer);
                }
            }
            
            _charges[player.SteamID] = charges - 1;
            WysteriaMessages.Local(player, $"Şok tabancası ateşlendi! {target.PlayerName} sersemletildi. Kalan şarj: {charges - 1}");
            WysteriaMessages.Local(target, "Bir gardiyan seni sersemletti!");
        }
        else
        {
            WysteriaMessages.Local(player, $"Yakınlarda hedef bulunamadı. Şarj: {charges}");
        }
    }
}

public sealed class GuardStaminaConfig : WysteriaConfig
{
    public int TaserCharges { get; set; } = 2;
    public float StunDurationSeconds { get; set; } = 3.0f;
    public float SlowdownMultiplier { get; set; } = 0.4f;
}
