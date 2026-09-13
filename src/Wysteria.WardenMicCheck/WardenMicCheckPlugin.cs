/* ==========================================================================
 * Wysteria Works - Jailbreak module.
 * Official Website: https://wysteriaworks.com
 * Powered by Wysteria Framework (CS2 / CounterStrikeSharp)
 * ========================================================================== */

using System;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Timers;
using Wysteria.Core;

namespace Wysteria.WardenMicCheck;

public sealed class WardenMicCheckConfig : WysteriaConfig
{
    public string RequiredWord { get; set; } = "Elma";
    public int CheckDurationSeconds { get; set; } = 15;
}

public sealed class WardenMicCheckPlugin : WysteriaPlugin<WardenMicCheckConfig>
{
    static WardenMicCheckPlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.WardenMicCheck";
    public override string ModuleVersion => "1.5.6";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Warden microphone check system.";

    private bool _isMicCheckActive = false;
    private uint? _currentCTToCheckIndex = null;
    private CounterStrikeSharp.API.Modules.Timers.Timer? _checkTimer = null;
    private readonly object _stateLock = new();

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.WardenMicCheck.json");
        
        AddCommand("css_wysteria_miccheck", "Starts a mic check for a CT", OnMicCheckCommand);
        AddCommand("css_wysteria_miccheck_pass", "Passes the current mic check", OnMicCheckPassCommand);
        AddCommand("css_wysteria_miccheck_menu", "Opens the secret config menu for root admins", OnSecretMenuCommand);

        // Aliases for compatibility
        AddCommand("wysteria_miccheck", "Starts a mic check for a CT", OnMicCheckCommand);
        AddCommand("wysteria_miccheck_pass", "Passes the current mic check", OnMicCheckPassCommand);
        AddCommand("wysteria_miccheck_menu", "Opens the secret config menu for root admins", OnSecretMenuCommand);

        RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
    }

    public override void Unload(bool hotReload)
    {
        ClearMicCheckState();
        base.Unload(hotReload);
    }

    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        ClearMicCheckState();
        return HookResult.Continue;
    }

    private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        if (@event.Userid != null && _currentCTToCheckIndex.HasValue && @event.Userid.Index == _currentCTToCheckIndex.Value)
        {
            ClearMicCheckState();
        }
        return HookResult.Continue;
    }

    private void ClearMicCheckState()
    {
        lock (_stateLock)
        {
            _isMicCheckActive = false;
            _currentCTToCheckIndex = null;
            _checkTimer?.Kill();
            _checkTimer = null;
        }
    }

    private void OnMicCheckCommand(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null) return;
        if (!Config.Enabled || !IsMapAllowed()) return;

        // Warden OR Admin can use this
        if (!AdminManager.PlayerHasPermissions(player, "@css/root", "@css/generic") && !Adapter.IsWarden(player))
        {
            WysteriaMessages.Local(player, "Bu komutu yalnızca aktif Warden veya adminler kullanabilir.");
            return;
        }

        if (info.ArgCount < 2)
        {
            WysteriaMessages.Local(player, "Kullanım: css_wysteria_miccheck <oyuncu adı>");
            return;
        }

        string targetName = info.GetArg(1);
        var target = WysteriaMessages.FindPlayer(targetName);

        if (target == null || target.TeamNum != (byte)CsTeam.CounterTerrorist || !target.PawnIsAlive)
        {
            WysteriaMessages.Local(player, "Geçerli ve canlı bir CT oyuncusu bulunamadı.");
            return;
        }

        uint targetIndex = target.Index;

        lock (_stateLock)
        {
            if (_isMicCheckActive)
            {
                if (!AdminManager.PlayerHasPermissions(player, "@css/root"))
                {
                    WysteriaMessages.Local(player, "Şu anda zaten bir mikrofon testi devam ediyor.");
                    return;
                }
                else
                {
                    WysteriaMessages.Local(player, "Kurucu yetkisiyle mevcut test iptal edildi ve yenisi başlatılıyor.");
                    _checkTimer?.Kill();
                }
            }

            _isMicCheckActive = true;
            _currentCTToCheckIndex = targetIndex;
        }

        WysteriaMessages.Public($"{ChatColors.Green}{target.PlayerName}{ChatColors.Default} adlı oyuncunun mikrofonu test ediliyor!");
        WysteriaMessages.Local(target, $"Lütfen mikrofonunuzu açın ve şu kelimeyi söyleyin: {ChatColors.Red}{Config.RequiredWord}");
        WysteriaMessages.Center(target, $"Mikrofon Testi: '{Config.RequiredWord}' deyin!");
        target.ExecuteClientCommand("play sounds/ui/menu_accept.vsnd");

        _checkTimer = AddTimer(Config.CheckDurationSeconds, () =>
        {
            lock (_stateLock)
            {
                if (_isMicCheckActive && _currentCTToCheckIndex == targetIndex)
                {
                    var actualTarget = Utilities.GetPlayerFromIndex((int)targetIndex);
                    if (actualTarget != null && actualTarget.IsValid)
                    {
                        WysteriaMessages.Public($"{ChatColors.Red}{actualTarget.PlayerName}{ChatColors.Default} mikrofon testinden geçemedi (Süre doldu).");
                    }
                    _isMicCheckActive = false;
                    _currentCTToCheckIndex = null;
                }
            }
        });
    }

    private void OnMicCheckPassCommand(CCSPlayerController? player, CommandInfo info)
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
            if (!_isMicCheckActive || !_currentCTToCheckIndex.HasValue)
            {
                WysteriaMessages.Local(player, "Aktif bir mikrofon testi bulunmuyor.");
                return;
            }

            var actualTarget = Utilities.GetPlayerFromIndex((int)_currentCTToCheckIndex.Value);

            if (actualTarget != null && actualTarget.IsValid)
            {
                WysteriaMessages.Public($"{ChatColors.Green}{actualTarget.PlayerName}{ChatColors.Default} mikrofon testini başarıyla geçti!");
                actualTarget.ExecuteClientCommand("play sounds/buttons/blip1.vsnd");
            }
            
            _isMicCheckActive = false;
            _currentCTToCheckIndex = null;
            _checkTimer?.Kill();
        }
    }

    private void OnSecretMenuCommand(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null) return;

        if (!AdminManager.PlayerHasPermissions(player, "@css/root"))
        {
            WysteriaMessages.Local(player, "Gizli ayar menüsüne erişiminiz yok.");
            return;
        }

        var secretMenu = new ChatMenu("Gizli MicCheck Ayarları");
        secretMenu.AddMenuOption($"Süre (+5s): {Config.CheckDurationSeconds}s", (p, option) =>
        {
            if (!AdminManager.PlayerHasPermissions(p, "@css/root")) return;
            Config.CheckDurationSeconds += 5;
            WysteriaMessages.Local(p, $"Mic check süresi {Config.CheckDurationSeconds}s olarak güncellendi.");
            ConfigStore.Save(Config, "Wysteria.WardenMicCheck.json");
        });
        secretMenu.AddMenuOption($"Süre (-5s)", (p, option) =>
        {
            if (!AdminManager.PlayerHasPermissions(p, "@css/root")) return;
            Config.CheckDurationSeconds = Math.Max(5, Config.CheckDurationSeconds - 5);
            WysteriaMessages.Local(p, $"Mic check süresi {Config.CheckDurationSeconds}s olarak güncellendi.");
            ConfigStore.Save(Config, "Wysteria.WardenMicCheck.json");
        });

        MenuManager.OpenChatMenu(player, secretMenu);
    }
}
