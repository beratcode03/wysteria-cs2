# Wysteria plugin rehberi / Plugin guide

Bu sayfa, panel yöneticisinin “hangi plugin ne yapıyor?” sorusuna hızlı cevap vermesi için hazırlanmıştır. Ayrıntılı ayarlar için ilgili plugin klasöründeki README dosyasını açın.

## Türkçe özet

| Plugin | Komut | Kim kullanır? | Config |
| --- | --- | --- | --- |
| Wysteria.Core | `!wysteria_w`, `!wysteria_opencells` | İlk geçerli CT / aktif Warden | Yok |
| AdminTools | `!wysteria` | `RequiredSteamIds` listesindeki yöneticiler | `Wysteria.AdminTools.json` |
| Advertiser | Otomatik | Yetki gerekmez | `Wysteria.Advertiser.json` |
| BountySystem | `!bounty` | Aktif Warden | `Wysteria.BountySystem.json` |
| CellSearch | `!cellsearch` | CT | `Wysteria.CellSearch.json` |
| CoinFlip | `!coinflip`, `!cfaccept` | Tüm geçerli oyuncular | `Wysteria.CoinFlip.json` |
| Contraband | `!stash` | T / mahkûmlar | `Wysteria.Contraband.json` |
| ContrabandScanner | `!scan` | CT | `Wysteria.ContrabandScanner.json` |
| DoorControl | `!doors` | Aktif Warden | `Wysteria.DoorControl.json` |
| FakeRebel | `!fakerebel` | T / mahkûmlar | `Wysteria.FakeRebel.json` |
| FreeDay | `!freeday <oyuncu>` | Aktif Warden | `Wysteria.FreeDay.json` |
| GuardJail | `!punish <oyuncu>` | Aktif Warden | `Wysteria.GuardJail.json` |
| GuardOrders | `!order <emir>` | Aktif Warden | `Wysteria.GuardOrders.json` |
| GuardStamina | `!stamina` | CT | `Wysteria.GuardStamina.json` |
| GuardTraining | `!train` | CT | `Wysteria.GuardTraining.json` |
| InmateRanks | `!rank` | Tüm geçerli oyuncular | `Wysteria.InmateRanks.json` |
| MetalDetector | Otomatik | Mahkûmlar / CT bildirimi | `Wysteria.MetalDetector.json` |
| PrisonBetrayal | `!snitch <oyuncu>` | T / mahkûmlar | `Wysteria.PrisonBetrayal.json` |
| PrisonJob | `!job` | T / mahkûmlar | `Wysteria.PrisonJob.json` |
| PrisonLockdown | `!lockdown` | Aktif Warden | `Wysteria.PrisonLockdown.json` |
| RiotAlarm | `!riot` | CT | `Wysteria.RiotAlarm.json` |
| RoundStats | `!stats` | Tüm geçerli oyuncular | `Wysteria.RoundStats.json` |
| TeamBalance | `!balance` | Aktif Warden | `Wysteria.TeamBalance.json` |
| WardenDraw | `!draw` | Aktif Warden | `Wysteria.WardenDraw.json` |
| WardenPanic | `!panic` | Aktif Warden | `Wysteria.WardenPanic.json` |
| WardenRules | `!rules` | Kurallar: herkes; ek durum: Warden | `Wysteria.WardenRules.json` |
| WardenShield | `!shield` | Aktif Warden | `Wysteria.WardenShield.json` |
| WardenVote | `!wardenvote` | Tüm geçerli oyuncular | `Wysteria.WardenVote.json` |

Oyuncu sohbetindeki `!komut` biçimi CounterStrikeSharp chat komutudur. Sunucu konsolunda aynı komut `css_komut` biçiminde yazılır.

### Ortak ayarlar

Çoğu modülün JSON dosyasında şu alanlar bulunur:

- `Enabled`: `false` yapıldığında plugin yüklenmiş olsa bile özelliği çalıştırmaz.
- `AllowedMaps`: `jb_,jail_` gibi harita adlarını veya başlangıçlarını virgülle kabul eder.
- `GlobalCooldown`: genel spam korumasıdır.

Bunlar cvar değildir. Dosyaları `game/csgo/addons/counterstrikesharp/configs/` klasöründe düzenleyin ve değişiklikten sonra sunucuyu yeniden başlatın.

### Entegrasyon sınırı

Bazı pluginler komutu, takım/rol kontrolünü, cooldown’u ve bildirim akışını hazırlar; fiziksel oyun davranışı ise sunucunun mevcut sistemlerine bağlanır. Entity oluşturma, gerçek kapı kontrolü, ses/lazer, stun, teleport ve JailShop kredi işlemleri için ilgili adapter’ın bağlanması gerekir. Her plugin README’sinde bu durum ayrıca yazılmıştır.

## English summary

| Plugin | Command | Who can use it? | Config |
| --- | --- | --- | --- |
| Wysteria.Core | `!wysteria_w`, `!wysteria_opencells` | First valid CT / active Warden | None |
| AdminTools | `!wysteria` | Administrators in `RequiredSteamIds` | `Wysteria.AdminTools.json` |
| Advertiser | Automatic | No permission required | `Wysteria.Advertiser.json` |
| BountySystem | `!bounty` | Active Warden | `Wysteria.BountySystem.json` |
| CellSearch | `!cellsearch` | CT players | `Wysteria.CellSearch.json` |
| CoinFlip | `!coinflip`, `!cfaccept` | All valid players | `Wysteria.CoinFlip.json` |
| Contraband | `!stash` | Terrorist / prisoners | `Wysteria.Contraband.json` |
| ContrabandScanner | `!scan` | CT players | `Wysteria.ContrabandScanner.json` |
| DoorControl | `!doors` | Active Warden | `Wysteria.DoorControl.json` |
| FakeRebel | `!fakerebel` | Terrorist / prisoners | `Wysteria.FakeRebel.json` |
| FreeDay | `!freeday <player>` | Active Warden | `Wysteria.FreeDay.json` |
| GuardJail | `!punish <player>` | Active Warden | `Wysteria.GuardJail.json` |
| GuardOrders | `!order <message>` | Active Warden | `Wysteria.GuardOrders.json` |
| GuardStamina | `!stamina` | CT players | `Wysteria.GuardStamina.json` |
| GuardTraining | `!train` | CT players | `Wysteria.GuardTraining.json` |
| InmateRanks | `!rank` | All valid players | `Wysteria.InmateRanks.json` |
| MetalDetector | Automatic | Prisoners / CT alerts | `Wysteria.MetalDetector.json` |
| PrisonBetrayal | `!snitch <player>` | Terrorist / prisoners | `Wysteria.PrisonBetrayal.json` |
| PrisonJob | `!job` | Terrorist / prisoners | `Wysteria.PrisonJob.json` |
| PrisonLockdown | `!lockdown` | Active Warden | `Wysteria.PrisonLockdown.json` |
| RiotAlarm | `!riot` | CT players | `Wysteria.RiotAlarm.json` |
| RoundStats | `!stats` | All valid players | `Wysteria.RoundStats.json` |
| TeamBalance | `!balance` | Active Warden | `Wysteria.TeamBalance.json` |
| WardenDraw | `!draw` | Active Warden | `Wysteria.WardenDraw.json` |
| WardenPanic | `!panic` | Active Warden | `Wysteria.WardenPanic.json` |
| WardenRules | `!rules` | Rules: everyone; extra status: Warden | `Wysteria.WardenRules.json` |
| WardenShield | `!shield` | Active Warden | `Wysteria.WardenShield.json` |
| WardenVote | `!wardenvote` | All valid players | `Wysteria.WardenVote.json` |

The `!command` form is for player chat. In the server console, use the matching `css_command` form. The common JSON fields are `Enabled`, `AllowedMaps`, and `GlobalCooldown`; these are not console cvars.