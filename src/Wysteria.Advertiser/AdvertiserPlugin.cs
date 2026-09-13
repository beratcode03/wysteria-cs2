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

namespace Wysteria.Advertiser;

public sealed class AdvertiserPlugin : WysteriaPlugin<AdvertiserConfig>
{
    static AdvertiserPlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.Advertiser";
    public override string ModuleVersion => "1.1.3";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Timed server-wide announcements.";

    private int _msgIndex = 0;

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.Advertiser.json");
        AddTimer(Config.IntervalSeconds, () => {
            if (!Config.Enabled || !IsMapAllowed()) return;
            
            if (Config.Messages != null && Config.Messages.Count > 0)
            {
                var msg = Config.Messages[_msgIndex % Config.Messages.Count].Replace("{StoreUrl}", Config.StoreUrl);
                CounterStrikeSharp.API.Server.PrintToChatAll($" \x04[Duyuru]\x01 {msg}");
                _msgIndex++;
            }
        }, TimerFlags.REPEAT);
    }

}

public sealed class AdvertiserConfig : WysteriaConfig
{
    public float IntervalSeconds { get; set; } = 600.0f;
    public string StoreUrl { get; set; } = "https://wysteria.example/store";
    public System.Collections.Generic.List<string> Messages { get; set; } = new() { "Mağaza ve Wysteria eklentileri: {StoreUrl}" };
}
