using System.Text.Json;
using System.Globalization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Admin;

namespace Wysteria.Core;

public static class WysteriaMessages
{
    public static void Local(CCSPlayerController player, string text) =>
        player.PrintToChat($"{ChatColors.Purple}[WYSTERIA]{ChatColors.Default} {text}");

    public static void Center(CCSPlayerController player, string text) => player.PrintToCenter(text);

    public static void Public(string text) =>
        Server.PrintToChatAll($"{ChatColors.Purple}[WYSTERIA]{ChatColors.Default} {text}");

    public static void Team(IEnumerable<CCSPlayerController> players, string text)
    {
        foreach (var player in players.Where(IsUsable))
            Local(player, text);
    }

    public static bool IsUsable(CCSPlayerController? player) =>
        player is not null && player.IsValid;

    public static CCSPlayerController? FindPlayer(string query) =>
        Utilities.GetPlayers().FirstOrDefault(player => IsUsable(player) &&
            (player.PlayerName.Equals(query, StringComparison.OrdinalIgnoreCase) ||
             player.SteamID.ToString().Equals(query, StringComparison.OrdinalIgnoreCase) ||
             player.UserId?.ToString().Equals(query, StringComparison.OrdinalIgnoreCase) == true));

    public static int AliveOn(CsTeam team) =>
        Utilities.GetPlayers().Count(player => IsUsable(player) && player.Team == team && player.PawnIsAlive);
}

public static class WysteriaGeometry
{
    public static bool TryParseVector(string value, out Vector vector)
    {
        vector = new Vector(0, 0, 0);
        var parts = value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3 ||
            !float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x) ||
            !float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y) ||
            !float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var z))
            return false;

        vector = new Vector(x, y, z);
        return true;
    }

    public static float DistanceSquared(Vector left, Vector right)
    {
        var x = left.X - right.X;
        var y = left.Y - right.Y;
        var z = left.Z - right.Z;
        return x * x + y * y + z * z;
    }
}

public static class WysteriaRound
{
    public static bool IsLive { get; private set; }
    public static bool CellsOpen { get; private set; }
    public static int RoundNumber { get; private set; }

    public static void Start()
    {
        if (IsLive) return;
        IsLive = true;
        CellsOpen = false;
        RoundNumber++;
    }

    public static void OpenCells() => CellsOpen = true;
    public static void CloseCells() => CellsOpen = false;
    public static void End() { IsLive = false; CellsOpen = false; }
}

public static class WardenService
{
    private static ulong _warden;
    public static bool IsWarden(CCSPlayerController player) =>
        _warden != 0 && player.SteamID == _warden;

    public static bool Claim(CCSPlayerController player)
    {
        if (player.TeamNum != (int)CsTeam.CounterTerrorist) return false;
        if (_warden != 0 && _warden != player.SteamID) return false;
        _warden = player.SteamID;
        return true;
    }

    public static void Reset() => _warden = 0;

    public static void Release(CCSPlayerController player)
    {
        if (IsWarden(player))
            _warden = 0;
    }
}

public static class ConfigStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };

    public static T Load<T>(string fileName) where T : new()
    {
        var directory = Path.Combine(Server.GameDirectory, "addons", "counterstrikesharp", "configs");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, fileName);
        if (!File.Exists(path))
        {
            var fresh = new T();
            File.WriteAllText(path, JsonSerializer.Serialize(fresh, Options));
            return fresh;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(File.ReadAllText(path), Options) ?? new T();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WYSTERIA] Could not read {path}: {ex.Message}");
            return new T();
        }
    }
}

public abstract class WysteriaPlugin<TConfig> : WysteriaPlugin where TConfig : new()
{
    protected TConfig Config { get; private set; } = new();
    protected readonly Dictionary<ulong, DateTime> Cooldowns = new();

    protected void LoadWysteriaConfig(string fileName) => Config = ConfigStore.Load<TConfig>(fileName);

    protected bool IsMapAllowed()
    {
        if (Config is not WysteriaConfig common || !common.Enabled) return false;
        var map = Server.MapName ?? string.Empty;
        var allowed = common.AllowedMaps.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return allowed.Length == 0 || allowed.Any(rule => map.Equals(rule, StringComparison.OrdinalIgnoreCase) ||
                                                           map.StartsWith(rule, StringComparison.OrdinalIgnoreCase));
    }

    protected bool TryGetPlayer(CommandInfo info, out CCSPlayerController player)
    {
        player = info.CallingPlayer!;
        return WysteriaMessages.IsUsable(player);
    }

    protected bool Permit(CommandInfo info, Func<CCSPlayerController, bool> predicate, string deniedMessage,
                          float cooldownSeconds = 0)
    {
        if (!TryGetPlayer(info, out var player)) return false;
        
        // CENTRALIZED ROOT BYPASS ARCHITECTURE
        bool isRoot = AdminManager.PlayerHasPermissions(player, "@css/root");
        
        if (!isRoot && !predicate(player))
        {
            WysteriaMessages.Local(player, deniedMessage);
            return false;
        }

        if (cooldownSeconds > 0 && !UseCooldown(player, cooldownSeconds))
        {
            WysteriaMessages.Local(player, "Bu özelliği tekrar kullanmak için beklemelisin.");
            return false;
        }

        return true;
    }

    protected bool UseCooldown(CCSPlayerController player, float seconds)
    {
        var key = player.SteamID;
        var now = DateTime.UtcNow;
        if (Cooldowns.TryGetValue(key, out var until) && until > now) return false;
        Cooldowns[key] = now.AddSeconds(seconds);
        return true;
    }

    protected void HookRoundLifecycle()
    {
        RegisterEventHandler<EventRoundStart>((_, _) => { WysteriaRound.Start(); WardenService.Reset(); return HookResult.Continue; });
        RegisterEventHandler<EventRoundEnd>((_, _) => { WysteriaRound.End(); WardenService.Reset(); return HookResult.Continue; });
    }
}

public class WysteriaConfig
{
    public bool Enabled { get; set; } = true;
    public string AllowedMaps { get; set; } = "jb_,jail_";
    public float GlobalCooldown { get; set; } = 5.0f;
}


public sealed class WysteriaCorePlugin : WysteriaPlugin
{
    public override string ModuleName => "Wysteria.Core";
    public override string ModuleVersion => "1.3.1";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Shared runtime for Wysteria modules.";

    public override void Load(bool hotReload)
    {
        SetPhase(RoundPhase.Off);
        SetActiveWarden(0);
        
        var realAdapter = new RealJailbreakAdapter();
        WysteriaPlugin.SetAdapter(realAdapter);
        WysteriaEconomy.RegisterAdapter(realAdapter);
        Adapter.SetWarden(null);

        RegisterEventHandler<EventRoundStart>((_, _) =>
        {
            WysteriaRound.Start();
            WardenService.Reset();
            SetActiveWarden(0);
            Adapter.SetWarden(null);
            SetPhase(RoundPhase.Freeze);
            return HookResult.Continue;
        });
        RegisterEventHandler<EventRoundFreezeEnd>((_, _) =>
        {
            SetPhase(RoundPhase.Live);
            return HookResult.Continue;
        });
        RegisterEventHandler<EventRoundEnd>((_, _) =>
        {
            WysteriaRound.End();
            WardenService.Reset();
            SetActiveWarden(0);
            Adapter.SetWarden(null);
            SetPhase(RoundPhase.PostRound);
            return HookResult.Continue;
        });

        AddCommand("css_wysteria_w", "Claim the Warden role for Wysteria modules.", (_, info) =>
        {
            if (info.CallingPlayer is not { IsValid: true } player) return;
            if (!WysteriaRound.IsLive)
            {
                WysteriaMessages.Local(player, "Round başlamadan Warden seçilemez.");
                return;
            }
            if (!WardenService.Claim(player))
            {
                WysteriaMessages.Local(player, "Warden rolü yalnızca ilk geçerli CT claim'ine açıktır.");
                return;
            }
            SetActiveWarden(player.SteamID);
            Adapter.SetWarden(player);
            WysteriaMessages.Public($"{player.PlayerName} Warden olarak doğrulandı.");
        });

        AddCommand("css_wysteria_opencells", "Mark the standalone Wysteria cell state as open.", (_, info) =>
        {
            if (info.CallingPlayer is not { IsValid: true } player) return;
            if (!WardenService.IsWarden(player))
            {
                WysteriaMessages.Local(player, "Only the active Warden can open the Wysteria cell state.");
                return;
            }
            WysteriaRound.OpenCells();
            Adapter.OpenCells();
            WysteriaMessages.Public("Hücre durumu Wysteria tarafından açık olarak işaretlendi.");
        });
    }
}

public static class WysteriaCoreGuard
{
    public static void Require()
    {
        // Every module calls this method so the Wysteria.Core dependency is explicit.
    }
}
