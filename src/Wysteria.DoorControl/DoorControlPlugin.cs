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

namespace Wysteria.DoorControl;

public sealed class DoorControlPlugin : WysteriaPlugin<DoorControlConfig>
{
    private CounterStrikeSharp.API.Modules.Timers.Timer? _doorTimer;
    private readonly object _stateLock = new();

    static DoorControlPlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.DoorControl";
    public override string ModuleVersion => "1.3.0";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Warden door control commands.";

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.DoorControl.json");
        
        RegisterEventHandler<EventRoundStart>(OnRoundStart);
        RegisterEventHandler<EventRoundEnd>(OnRoundEnd);

        AddCommand("css_doors", "Warden’ın round içi kapı durumunu değiştirmesi.", OnDoorsCommand);
    }

    public override void Unload(bool hotReload)
    {
        ClearTimer();
        base.Unload(hotReload);
    }

    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        ClearTimer();
        return HookResult.Continue;
    }

    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        ClearTimer();
        return HookResult.Continue;
    }

    private void ClearTimer()
    {
        lock (_stateLock)
        {
            _doorTimer?.Kill();
            _doorTimer = null;
        }
    }

    private void OnDoorsCommand(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;
        if (!Config.Enabled || !IsMapAllowed()) return;
        if (!WysteriaRound.IsLive) { WysteriaMessages.Local(player, "Bu özellik yalnızca canlı round sırasında kullanılabilir."); return; }
        if (!Permit(info, p => WardenService.IsWarden(p), "Sadece aktif Warden bu komutu kullanabilir.", Config.CooldownSeconds)) return;
        
        int doorCount = 0;
        foreach (var door in Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("func_door"))
        {
            if (door != null && door.IsValid)
            {
                door.AcceptInput("Open");
                doorCount++;
            }
        }
        foreach (var door in Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("prop_door_rotating"))
        {
            if (door != null && door.IsValid)
            {
                door.AcceptInput("Open");
                doorCount++;
            }
        }

        WysteriaMessages.Public($"{doorCount} kapı açıldı. Kapılar {Config.OpenDurationSeconds} saniye sonra kapanacak.");

        lock (_stateLock)
        {
            _doorTimer?.Kill();
            
            _doorTimer = AddTimer(Config.OpenDurationSeconds, () =>
            {
                lock (_stateLock)
                {
                    _doorTimer = null;
                }
                
                foreach (var door in Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("func_door"))
                {
                    if (door != null && door.IsValid) door.AcceptInput("Close");
                }
                foreach (var door in Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("prop_door_rotating"))
                {
                    if (door != null && door.IsValid) door.AcceptInput("Close");
                }
            });
        }
    }
}

public sealed class DoorControlConfig : WysteriaConfig
{
    public int CooldownSeconds { get; set; } = 15;
    public int OpenDurationSeconds { get; set; } = 10;
}
