/* ==========================================================================
 * Wysteria Works - Jailbreak module.
 * Official Website: https://wysteriaworks.com
 * Powered by Wysteria Framework (CS2 / CounterStrikeSharp)
 * ========================================================================== */

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using Wysteria.Core;

namespace Wysteria.VoiceControl;

public sealed class VoiceControlConfig : WysteriaConfig
{
    // AutoMute feature removed as it is unimplemented and unsupported natively
}

public sealed class VoiceControlPlugin : WysteriaPlugin<VoiceControlConfig>
{
    static VoiceControlPlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.VoiceControl";
    public override string ModuleVersion => "1.1.5";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Jailbreak voice management.";

    private bool _isMuteActive = false;
    private readonly object _stateLock = new();

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.VoiceControl.json");

        AddCommand("css_wysteria_mutets", "Mutes all T players", OnMuteTsCommand);
        AddCommand("css_wysteria_unmutets", "Unmutes all T players", OnUnmuteTsCommand);

        AddCommand("wysteria_mutets", "Mutes all T players", OnMuteTsCommand);
        AddCommand("wysteria_unmutets", "Unmutes all T players", OnUnmuteTsCommand);
        
        RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        RegisterEventHandler<EventRoundStart>(OnRoundStart);
    }

    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        ResetMute();
        return HookResult.Continue;
    }

    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        ResetMute();
        return HookResult.Continue;
    }

    private void ResetMute()
    {
        lock (_stateLock)
        {
            if (!_isMuteActive) return;
            _isMuteActive = false;
        }

        foreach (var p in Utilities.GetPlayers())
        {
            if (WysteriaMessages.IsUsable(p) && p.TeamNum == (byte)CsTeam.Terrorist)
            {
                p.VoiceFlags = VoiceFlags.Normal;
            }
        }
    }

    private void OnMuteTsCommand(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null) return;
        if (!Config.Enabled || !IsMapAllowed()) return;

        // Warden OR Admin can use this
        if (!AdminManager.PlayerHasPermissions(player, "@css/root", "@css/generic") && !Adapter.IsWarden(player))
        {
            WysteriaMessages.Local(player, "Bu komutu yalnızca aktif Warden veya adminler kullanabilir.");
            return;
        }

        lock (_stateLock)
        {
            _isMuteActive = true;
        }

        foreach (var p in Utilities.GetPlayers())
        {
            if (WysteriaMessages.IsUsable(p) && p.TeamNum == (byte)CsTeam.Terrorist)
            {
                p.VoiceFlags = VoiceFlags.Muted;
            }
        }

        WysteriaMessages.Public($"{ChatColors.Red}T'lerin sesi Warden/Admin tarafından kapatıldı.");
        player.ExecuteClientCommand("play sounds/ui/menu_accept.vsnd");
    }

    private void OnUnmuteTsCommand(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null) return;
        if (!Config.Enabled || !IsMapAllowed()) return;

        // Warden OR Admin can use this
        if (!AdminManager.PlayerHasPermissions(player, "@css/root", "@css/generic") && !Adapter.IsWarden(player))
        {
            WysteriaMessages.Local(player, "Bu komutu yalnızca aktif Warden veya adminler kullanabilir.");
            return;
        }

        lock (_stateLock)
        {
            _isMuteActive = false;
        }

        foreach (var p in Utilities.GetPlayers())
        {
            if (WysteriaMessages.IsUsable(p) && p.TeamNum == (byte)CsTeam.Terrorist)
            {
                p.VoiceFlags = VoiceFlags.Normal;
            }
        }

        WysteriaMessages.Public($"{ChatColors.Green}T'lerin sesi açıldı.");
        player.ExecuteClientCommand("play sounds/buttons/blip1.vsnd");
    }
}
