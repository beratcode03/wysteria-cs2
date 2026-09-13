/* ==========================================================================
 * Wysteria Works - Jailbreak module.
 * Official Website: https://wysteriaworks.com
 * Powered by Wysteria Framework (CS2 / CounterStrikeSharp)
 * ========================================================================== */

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Wysteria.Core;

namespace Wysteria.MetalDetector;

public sealed class MetalDetectorConfig : WysteriaPluginConfig
{
    public float DetectorCooldown { get; set; } = 15.0f;
    public float FlashDuration { get; set; } = 4.0f;
    public float DetectorRadius { get; set; } = 256.0f;
    public string DetectorCenter { get; set; } = string.Empty;
    public string AlarmSound { get; set; } = "cs2go_siren_alert";
}

[MinimumApiVersion(319)]
public sealed class MetalDetectorPlugin : WysteriaPlugin
{
    public override string ModuleName => "Wysteria.MetalDetector";
    public override string ModuleVersion => "1.0.7";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Configurable prison metal detector.";

    private MetalDetectorConfig _config = new();
    private readonly Dictionary<ulong, DateTime> _alarmCooldown = new();
    private Vector? _triggerCenter;
    private bool _cellsWereOpen;
    private bool _hasValidCenter;

    public override void Load(bool hotReload)
    {
        _config = ConfigStore.Load<MetalDetectorConfig>("Wysteria.MetalDetector.json");
        _hasValidCenter = WysteriaGeometry.TryParseVector(_config.DetectorCenter, out var center);
        _triggerCenter = _hasValidCenter ? center : null;
        if (!_hasValidCenter)
            Console.WriteLine("[WYSTERIA] Wysteria.MetalDetector: DetectorCenter ayarlanmadı; dedektör devre dışı.");

        RegisterListener<Listeners.OnTick>(OnTick);
        Console.WriteLine("[WYSTERIA] Wysteria.MetalDetector loaded.");
    }

    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        _triggerCenter = _hasValidCenter &&
            WysteriaGeometry.TryParseVector(_config.DetectorCenter, out var center)
            ? center
            : null;
        _cellsWereOpen = false;
        _alarmCooldown.Clear();
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        if (@event.Userid is { IsValid: true } player)
            _alarmCooldown.Remove(player.SteamID);
        return HookResult.Continue;
    }

    private void OnTick()
    {
        if (!_config.Enabled || !_hasValidCenter || !IsMapAllowed(_config.AllowedMaps)) return;
        if (WysteriaPlugin.CurrentPhase != RoundPhase.Live) return;

        if (!WysteriaPlugin.Adapter.AreCellsOpen()) { _cellsWereOpen = false; return; }
        if (!_cellsWereOpen)
        {
            _cellsWereOpen = true;
            _triggerCenter = WysteriaGeometry.TryParseVector(_config.DetectorCenter, out var center)
                ? center
                : null;
        }

        if (_triggerCenter is null) return;
        var radius = Math.Max(0, _config.DetectorRadius);
        var radiusSq = radius * radius;

        foreach (var player in Utilities.GetPlayers())
        {
            if (!IsPrisoner(player) || !IsAlive(player)) continue;
            var pawn = player.PlayerPawn?.Value;
            if (pawn?.AbsOrigin is null) continue;

            var dx = pawn.AbsOrigin.X - _triggerCenter.X;
            var dy = pawn.AbsOrigin.Y - _triggerCenter.Y;
            var dz = pawn.AbsOrigin.Z - _triggerCenter.Z;
            if (dx * dx + dy * dy + dz * dz > radiusSq) continue;

            if (!WysteriaPlugin.Adapter.HasContraband(player)) continue;
            if (!CheckCooldown(_alarmCooldown, player.SteamID, Math.Max(0, _config.DetectorCooldown))) continue;

            TripAlarm(player);
        }
    }

    private void TripAlarm(CCSPlayerController carrier)
    {
        WysteriaPlugin.Adapter.PlaySound(null, _config.AlarmSound);
        Server.PrintToChatAll($"[WYSTERIA] *** KAÇAK EŞYA TESPİT EDİLDİ! {carrier.PlayerName} üzerinde silah tespit edildi! ***");
        foreach (var guard in Utilities.GetPlayers().Where(p => IsGuard(p)))
        {
            WysteriaMessages.Center(guard,
                $"CONTRABAND DETECTED: {carrier.PlayerName}");
            guard.PrintToChat($"[WYSTERIA] !!! CONTRABAND DETECTED: {carrier.PlayerName} !!!");
        }

        if (_config.FlashDuration > 0 && carrier.PlayerPawn?.Value != null)
        {
            carrier.PlayerPawn.Value.BlindUntilTime = Server.CurrentTime + _config.FlashDuration;
            carrier.PlayerPawn.Value.BlindStartTime = Server.CurrentTime;
            Utilities.SetStateChanged(carrier.PlayerPawn.Value, "CCSPlayerPawn", "m_flBlindUntilTime");
        }
    }
}
