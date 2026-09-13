using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace Wysteria.Core;

public enum RoundPhase
{
    Off,
    Freeze,
    Live,
    PostRound
}

public interface IJailbreakAdapter
{
    bool AreCellsOpen();
    void OpenCells();
    void CloseCells();
    void LockDoors(float duration);
    void UnlockDoors();
    void DamageDoor(CBaseEntity door, int damage);
    int GetPlayerCredits(CCSPlayerController player);
    void AddPlayerCredits(CCSPlayerController player, int amount);
    void SetPlayerCredits(CCSPlayerController player, int amount);
    bool HasContraband(CCSPlayerController player);
    bool IsWarden(CCSPlayerController player);
    void SetWarden(CCSPlayerController? player);
    void PlaySound(CCSPlayerController? target, string sound);
    void PrintAlert(CCSPlayerController? target, string message);
    string GetActiveMapName();
    IEnumerable<CBaseEntity> GetMotorizedDoors();
    void MarkPlayer(CCSPlayerController player, string icon, float duration);
    void UnmarkPlayer(CCSPlayerController player);
}

public sealed class UnavailableJailbreakAdapter : IJailbreakAdapter
{
    public bool AreCellsOpen() => WysteriaRound.CellsOpen;
    public void OpenCells() => WysteriaRound.OpenCells();
    public void CloseCells() => WysteriaRound.CloseCells();

    public void LockDoors(float duration) { }
    public void UnlockDoors() { }
    public void DamageDoor(CBaseEntity door, int damage) { }

    public int GetPlayerCredits(CCSPlayerController player) => 0;
    public void AddPlayerCredits(CCSPlayerController player, int amount) { }
    public void SetPlayerCredits(CCSPlayerController player, int amount) { }

    public bool HasContraband(CCSPlayerController player) => false;
    public bool IsWarden(CCSPlayerController player) => WardenService.IsWarden(player);
    public void SetWarden(CCSPlayerController? player) { }

    public void PlaySound(CCSPlayerController? target, string sound) { }
    public void PrintAlert(CCSPlayerController? target, string message) { }
    public string GetActiveMapName() => Server.MapName ?? string.Empty;
    public IEnumerable<CBaseEntity> GetMotorizedDoors() => Enumerable.Empty<CBaseEntity>();
    public void MarkPlayer(CCSPlayerController player, string icon, float duration) { }
    public void UnmarkPlayer(CCSPlayerController player) { }
}

public abstract class WysteriaPlugin : BasePlugin
{
    public override string ModuleVersion => "1.0.0";

    public static IJailbreakAdapter Adapter { get; private set; } =
        new UnavailableJailbreakAdapter();

    protected static RoundPhase CurrentPhase { get; private set; } = RoundPhase.Off;
    protected static ulong ActiveWardenSteam { get; private set; }

    public static void SetAdapter(IJailbreakAdapter adapter) =>
        Adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));

    public new void AddCommand(string name, string description, CounterStrikeSharp.API.Modules.Commands.CommandInfo.CommandCallback handler)
    {
        base.AddCommand(name, description, handler);
        if (name.StartsWith("css_") && !name.StartsWith("css_wysteria_"))
        {
            base.AddCommand(name.Replace("css_", "wysteria_"), description, handler);
        }
    }

    protected static void SetPhase(RoundPhase phase) => CurrentPhase = phase;
    protected static void SetActiveWarden(ulong steam) => ActiveWardenSteam = steam;

    protected bool IsMapAllowed(string? allowed = null)
    {
        var list = (allowed ?? "jb_,jail_")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var map = Adapter.GetActiveMapName();
        return list.Length == 0 || list.Any(rule =>
            map.StartsWith(rule, StringComparison.OrdinalIgnoreCase));
    }

    protected bool CheckCooldown(Dictionary<ulong, DateTime> tracker, ulong steam, float cooldown)
    {
        if (cooldown <= 0) return true;

        var now = DateTime.UtcNow;
        if (tracker.TryGetValue(steam, out var last) &&
            (now - last).TotalSeconds < cooldown)
            return false;

        tracker[steam] = now;
        return true;
    }

    protected bool IsPrisoner(CCSPlayerController? player) =>
        player is { IsValid: true, Team: CounterStrikeSharp.API.Modules.Utils.CsTeam.Terrorist };

    protected bool IsGuard(CCSPlayerController? player) =>
        player is { IsValid: true, Team: CounterStrikeSharp.API.Modules.Utils.CsTeam.CounterTerrorist };

    protected bool IsAlive(CCSPlayerController? player) =>
        player is { IsValid: true } &&
        player.PlayerPawn?.Value is { IsValid: true, Health: > 0 };
}

public abstract class WysteriaPluginConfig
{
    public bool Enabled { get; set; } = true;
    public string AllowedMaps { get; set; } = "jb_,jail_";
    public float GlobalCooldown { get; set; } = 5.0f;
}
