/* ==========================================================================
 * Wysteria Works - Jailbreak module.
 * Official Website: https://wysteriaworks.com
 * Powered by Wysteria Framework (CS2 / CounterStrikeSharp)
 * ========================================================================== */

using System.Collections.Generic;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Wysteria.Core;
using CounterStrikeSharp.API.Modules.Admin;

namespace Wysteria.WardenVote;

public sealed class WardenVotePlugin : WysteriaPlugin<WardenVoteConfig>
{
    private bool _voteActive = false;
    private readonly HashSet<uint> _votedPlayers = new();
    private readonly object _voteLock = new();

    static WardenVotePlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.WardenVote";
    public override string ModuleVersion => "1.0.8";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - End-of-round Warden voting.";

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.WardenVote.json");
        
        RegisterEventHandler<EventRoundStart>((@event, info) => {
            lock (_voteLock)
            {
                _voteActive = false;
                _votedPlayers.Clear();
            }
            return HookResult.Continue;
        });

        AddCommand("css_wardenvote", "Mahkûmların Warden performansını round sonunda oylaması.", OnVoteCommand);
        AddCommand("css_wysteria_wardenvote", "Mahkûmların Warden performansını round sonunda oylaması.", OnVoteCommand);
    }

    private void OnVoteCommand(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;
        if (!Config.Enabled || !IsMapAllowed()) return;
        
        bool isAdmin = AdminManager.PlayerHasPermissions(player, "@css/root", "@css/generic");
        if (!isAdmin && (!WysteriaRound.IsLive || player.TeamNum != (byte)CsTeam.Terrorist || !player.PawnIsAlive))
        {
            WysteriaMessages.Local(player, "Oylamayı yalnızca hayattaki mahkûmlar veya adminler başlatabilir.");
            return;
        }

        if (!Permit(info, p => true, "", Config.GlobalCooldown)) return;

        lock (_voteLock)
        {
            if (_voteActive)
            {
                WysteriaMessages.Local(player, "Şu anda zaten aktif bir oylama var.");
                return;
            }
            _voteActive = true;
            _votedPlayers.Clear();
        }

        WysteriaMessages.Public($"Warden oylaması başladı ({Config.VoteDurationSeconds:0.0}s). Oyun içi oylama menüsünü kullanın.");

        var voteMenu = new CounterStrikeSharp.API.Modules.Menu.ChatMenu("Warden Performansı Oylaması");
        int yesVotes = 0;
        int noVotes = 0;

        voteMenu.AddMenuOption("İyi (Yes)", (p, option) => {
            lock (_voteLock)
            {
                if (!_voteActive) return;
                if (!_votedPlayers.Add(p.Index))
                {
                    WysteriaMessages.Local(p, "Zaten oy kullandın!");
                    return;
                }
                yesVotes++;
                WysteriaMessages.Local(p, "Oy verdin: İyi");
            }
        });
        
        voteMenu.AddMenuOption("Kötü (No)", (p, option) => {
            lock (_voteLock)
            {
                if (!_voteActive) return;
                if (!_votedPlayers.Add(p.Index))
                {
                    WysteriaMessages.Local(p, "Zaten oy kullandın!");
                    return;
                }
                noVotes++;
                WysteriaMessages.Local(p, "Oy verdin: Kötü");
            }
        });

        foreach (var p in Utilities.GetPlayers())
        {
            if (p.IsValid && p.Team == CsTeam.Terrorist)
            {
                CounterStrikeSharp.API.Modules.Menu.MenuManager.OpenChatMenu(p, voteMenu);
            }
        }

        AddTimer(Config.VoteDurationSeconds, () => {
            lock (_voteLock)
            {
                _voteActive = false;
            }
            
            int totalVotes = yesVotes + noVotes;
            if (totalVotes < Config.MinimumVotes)
            {
                WysteriaMessages.Public($"Warden oylaması iptal edildi. Yeterli oy kullanılmadı ({totalVotes}/{Config.MinimumVotes}).");
            }
            else
            {
                string result = yesVotes >= noVotes ? "Başarılı" : "Başarısız";
                WysteriaMessages.Public($"Warden oylaması sonuçlandı! İyi: {yesVotes}, Kötü: {noVotes}. Sonuç: {result}");
            }
        });
    }
}

public sealed class WardenVoteConfig : WysteriaConfig
{
    public float VoteDurationSeconds { get; set; } = 15.0f;
    public int MinimumVotes { get; set; } = 3;
}
