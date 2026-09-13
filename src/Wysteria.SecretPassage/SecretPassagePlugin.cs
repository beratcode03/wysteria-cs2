/* ==========================================================================
 * Wysteria Works - Jailbreak module.
 * Official Website: https://wysteriaworks.com
 * Powered by Wysteria Framework (CS2 / CounterStrikeSharp)
 * ========================================================================== */

using System;
using System.Collections.Generic;
using System.Linq;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Wysteria.Core;

namespace Wysteria.SecretPassage;

public sealed class SecretPassagePlugin : WysteriaPlugin<SecretPassageConfig>
{
    private readonly List<CHandle<CBaseEntity>> _vents = new();
    private readonly object _stateLock = new();

    static SecretPassagePlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.SecretPassage";
    public override string ModuleVersion => "1.5.3";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Hidden passage discovery system.";

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.SecretPassage.json");
        
        RegisterEventHandler<EventRoundStart>(OnRoundStart);
        
        RegisterListener<Listeners.OnEntitySpawned>(OnEntitySpawned);
        RegisterListener<Listeners.OnEntityDeleted>(OnEntityDeleted);
        
        AddCommand("css_ventstatus", "Yakındaki gizli geçitlerin durumunu gösterir.", OnVentStatusCommand);
        AddCommand("css_wysteria_ventstatus", "Yakındaki gizli geçitlerin durumunu gösterir.", OnVentStatusCommand);
    }

    public override void Unload(bool hotReload)
    {
        lock (_stateLock)
        {
            _vents.Clear();
        }
        base.Unload(hotReload);
    }

    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        lock (_stateLock)
        {
            _vents.Clear(); // They will be re-added by OnEntitySpawned
        }
        return HookResult.Continue;
    }

    private void OnEntitySpawned(CEntityInstance entity)
    {
        if (!Config.Enabled || !IsMapAllowed() || entity is not CBaseEntity breakable ||
            !Config.EntityClassNames.Any(name =>
                breakable.DesignerName.Contains(name, StringComparison.OrdinalIgnoreCase)))
            return;

        Server.NextFrame(() =>
        {
            if (breakable.IsValid)
            {
                breakable.MaxHealth = Math.Max(1, Config.VentHealth);
                breakable.Health = Math.Max(1, Config.VentHealth);
                
                lock (_stateLock)
                {
                    _vents.Add(new CHandle<CBaseEntity>((nint)breakable.Index));
                }
            }
        });
    }

    private void OnEntityDeleted(CEntityInstance entity)
    {
        if (entity is CBaseEntity breakable)
        {
            bool removed = false;
            lock (_stateLock)
            {
                removed = _vents.RemoveAll(h => h.Index == (uint)breakable.Index) > 0;
            }
            
            // Guard alert must only trigger if the round is actually live, otherwise round restarts cause spam
            if (removed && Config.GuardAlertEnabled && WysteriaRound.IsLive)
            {
                foreach (var p in Utilities.GetPlayers().Where(x => x.IsValid && x.TeamNum == (byte)CsTeam.CounterTerrorist))
                {
                    WysteriaMessages.Local(p, $"{ChatColors.Red}DİKKAT:{ChatColors.Default} Bir gizli geçit kırıldı!");
                }
            }
        }
    }

    private void OnVentStatusCommand(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !player.PawnIsAlive) return;
        if (!Config.Enabled || !IsMapAllowed()) return;
        
        if (!IsPrisoner(player))
        {
            WysteriaMessages.Local(player, "Bu komut yalnızca mahkûmlar için kullanılabilir.");
            return;
        }

        var origin = player.PlayerPawn?.Value?.AbsOrigin;
        if (origin == null) return;

        int nearby = 0;
        lock (_stateLock)
        {
            foreach (var handle in _vents)
            {
                if (handle.IsValid && handle.Value != null)
                {
                    var ventOrigin = handle.Value.AbsOrigin;
                    if (ventOrigin != null)
                    {
                        if (WysteriaGeometry.DistanceSquared(ventOrigin, origin) <= Config.StatusRadius * Config.StatusRadius)
                        {
                            nearby++;
                        }
                    }
                }
            }
        }

        WysteriaMessages.Local(player, nearby == 0
            ? "Yakında kayıtlı bir gizli geçit yok."
            : $"{nearby} gizli geçit algılandı. Dayanıklılık: {Config.VentHealth}.");
    }
}

public sealed class SecretPassageConfig : WysteriaConfig
{
    public int VentHealth { get; set; } = 250;
    public bool GuardAlertEnabled { get; set; } = false;
    public float StatusRadius { get; set; } = 256.0f;
    public string[] EntityClassNames { get; set; } = ["func_breakable", "func_physbox", "func_shatterglass"];
}
