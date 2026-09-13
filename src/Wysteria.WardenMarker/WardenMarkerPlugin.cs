/* ==========================================================================
 * Wysteria Works - Jailbreak module.
 * Official Website: https://wysteriaworks.com
 * Powered by Wysteria Framework (CS2 / CounterStrikeSharp)
 * ========================================================================== */

using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using Wysteria.Core;

namespace Wysteria.WardenMarker;

public sealed class WardenMarkerPlugin : WysteriaPlugin<WardenMarkerConfig>
{
    private readonly Dictionary<ulong, (Color Color, RenderMode_t Mode)> _marked = new();
    private readonly Dictionary<ulong, int> _markVersions = new();

    static WardenMarkerPlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.WardenMarker";
    public override string ModuleVersion => "1.1.8";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Visible prisoner marking tool.";

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.WardenMarker.json");
        RegisterEventHandler<EventRoundEnd>((_, _) =>
        {
            foreach (var player in Utilities.GetPlayers()) Unmark(player);
            _markVersions.Clear();
            return HookResult.Continue;
        });
        RegisterEventHandler<EventPlayerDeath>((@event, _) =>
        {
            if (@event.Userid is { IsValid: true } player)
                Unmark(player);
            return HookResult.Continue;
        });
        RegisterEventHandler<EventPlayerDisconnect>((@event, _) =>
        {
            if (@event.Userid is { IsValid: true } player)
            {
                Unmark(player);
                _markVersions.Remove(player.SteamID);
            }
            return HookResult.Continue;
        });
        AddCommand("css_isaret", "Aktif Warden bir mahkûmu guardlar için işaretler.", (_, info) =>
        {
            if (!TryGetPlayer(info, out var player)) return;
            if (!Config.Enabled || !IsMapAllowed()) return;
            if (!Permit(info, WardenService.IsWarden, "Bu komutu yalnızca aktif Warden kullanabilir."))
                return;
            if (info.ArgCount < 2)
            {
                WysteriaMessages.Local(player, "Kullanım: !isaret <oyuncu>");
                return;
            }

            var target = WysteriaMessages.FindPlayer(info.GetArg(1));
            if (target is null || !IsPrisoner(target))
            {
                WysteriaMessages.Local(player, "Geçerli bir mahkûm bulunamadı.");
                return;
            }

            if (!Mark(target))
            {
                WysteriaMessages.Local(player, "Bu mahkûm şu anda işaretlenemiyor.");
                return;
            }
            UseCooldown(player, Math.Max(0, Config.GlobalCooldown));
            WysteriaMessages.Team(Utilities.GetPlayers().Where(IsGuard),
                $"{target.PlayerName} Warden tarafından işaretlendi.");
            var version = _markVersions[target.SteamID];
            AddTimer(Math.Max(0.1f, Config.MarkerDuration), () => Unmark(target, version));
        });
    }

    private bool Mark(CCSPlayerController player)
    {
        var pawn = player.PlayerPawn?.Value;
        if (pawn is null || !pawn.IsValid) return false;
        if (!_marked.ContainsKey(player.SteamID))
            _marked[player.SteamID] = (pawn.Render, pawn.RenderMode);
        var color = Color.FromName(Config.GlowColor);
        pawn.Render = color.IsEmpty ? Color.Red : color;
        pawn.RenderMode = RenderMode_t.kRenderGlow;
        Adapter.MarkPlayer(player, "warden-marker", Config.MarkerDuration);
        _markVersions[player.SteamID] =
            _markVersions.TryGetValue(player.SteamID, out var current) ? current + 1 : 1;
        return true;
    }

    private void Unmark(CCSPlayerController player)
        => Unmark(player, null);

    private void Unmark(CCSPlayerController player, int? expectedVersion)
    {
        if (expectedVersion.HasValue &&
            (!_markVersions.TryGetValue(player.SteamID, out var current) || current != expectedVersion))
            return;
        if (!_marked.Remove(player.SteamID, out var previous)) return;
        _markVersions.Remove(player.SteamID);
        var pawn = player.PlayerPawn?.Value;
        if (pawn is { IsValid: true })
        {
            pawn.Render = previous.Color;
            pawn.RenderMode = previous.Mode;
        }
        Adapter.UnmarkPlayer(player);
    }
}

public sealed class WardenMarkerConfig : WysteriaConfig
{
    public float MarkerDuration { get; set; } = 30.0f;
    public string GlowColor { get; set; } = "Red";
}
