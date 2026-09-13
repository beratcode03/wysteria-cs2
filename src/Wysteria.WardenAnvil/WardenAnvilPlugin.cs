/* ==========================================================================
 * Wysteria Works - Jailbreak module.
 * Official Website: https://wysteriaworks.com
 * Powered by Wysteria Framework (CS2 / CounterStrikeSharp)
 * ========================================================================== */

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Wysteria.Core;

namespace Wysteria.WardenAnvil;

public sealed class WardenAnvilPlugin : WysteriaPlugin<WardenAnvilConfig>
{
    private bool _transferPending;

    static WardenAnvilPlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.WardenAnvil";
    public override string ModuleVersion => "1.3.1";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Automatic Warden role transfer.";

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.WardenAnvil.json");
        RegisterEventHandler<EventRoundStart>((_, _) =>
        {
            _transferPending = false;
            return HookResult.Continue;
        });
        RegisterEventHandler<EventRoundEnd>((_, _) =>
        {
            _transferPending = false;
            return HookResult.Continue;
        });
        RegisterEventHandler<EventPlayerDeath>((@event, _) =>
        {
            if (Config.Enabled && IsMapAllowed() && Config.AutoTransferEnabled &&
                @event.Userid is { IsValid: true } player && WardenService.IsWarden(player))
            {
                ClearWarden();
                ScheduleTransfer();
            }
            return HookResult.Continue;
        });
        RegisterEventHandler<EventPlayerDisconnect>((@event, _) =>
        {
            if (Config.Enabled && IsMapAllowed() && Config.AutoTransferEnabled &&
                @event.Userid is { IsValid: true } player && WardenService.IsWarden(player))
            {
                ClearWarden();
                ScheduleTransfer();
            }
            return HookResult.Continue;
        });
        RegisterEventHandler<EventPlayerSpawn>((_, _) =>
        {
            if (Config.Enabled && IsMapAllowed() && Config.AutoTransferEnabled &&
                WysteriaRound.IsLive && CurrentPhase == RoundPhase.Live &&
                !Utilities.GetPlayers().Any(WardenService.IsWarden))
                ScheduleTransfer();
            return HookResult.Continue;
        });
    }

    private void ScheduleTransfer()
    {
        if (_transferPending) return;
        _transferPending = true;
        AddTimer(Math.Max(0.1f, Config.TransferDelay), () =>
        {
            _transferPending = false;
            if (!WysteriaRound.IsLive || CurrentPhase != RoundPhase.Live || !Config.AutoTransferEnabled) return;

            var replacement = Utilities.GetPlayers()
                .Where(IsGuard)
                .Where(IsAlive)
                .OrderByDescending(player => player.Score)
                .ThenByDescending(player => player.CompetitiveRanking)
                .FirstOrDefault();
            if (replacement is null)
            {
                WysteriaMessages.Public("Yeni Warden atanamadı: uygun CT bulunamadı.");
                return;
            }

            WardenService.Reset();
            if (!WardenService.Claim(replacement)) return;
            SetActiveWarden(replacement.SteamID);
            Adapter.SetWarden(replacement);
            WysteriaMessages.Public($"{replacement.PlayerName} yedek Warden olarak görevi devraldı.");
        });
    }

    private static void ClearWarden()
    {
        WardenService.Reset();
        SetActiveWarden(0);
        Adapter.SetWarden(null);
    }
}

public sealed class WardenAnvilConfig : WysteriaConfig
{
    public bool AutoTransferEnabled { get; set; } = true;
    public float TransferDelay { get; set; } = 0.2f;
}
