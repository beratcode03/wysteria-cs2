# Wysteria CS2 Jailbreak

Wysteria CS2 Jailbreak, CounterStrikeSharp (CS2) altyapısı üzerine inşa edilmiş tam kapsamlı, modüler ve production-ready (üretim aşamasına hazır) bir plugin ekosistemidir.

## 🇹🇷 Türkçe (Turkish)

### 📦 Mimari (Architecture)
Sistem **1 Ana Modül (Core)** ve **50 bağımsız modül** (plugin) üzerinden çalışır. Core modülü; ortak yetki (permission), loglama (logging), oyun evresi (round state) ve komutçu (Warden) atamaları gibi işlemleri yönetirken, diğer pluginler bağımsız olarak çalışır ve yalnızca Core'a bağımlıdır.

### 🧩 Plugin Listesi ve İşlevleri (Plugin List and Functions)
Aşağıda Wysteria CS2 ekosistemindeki tüm modüllerin ne işe yaradığı listelenmiştir:

- **Wysteria.Core**: Tüm pluginlerin merkezidir. API, ortak değişkenler ve yetki kontrollerini sağlar.
- **Wysteria.AdminTools**: Yöneticilerin oyuncuları yönetmesi için gerekli temel araçları barındırır.
- **Wysteria.Advertiser**: Sunucu içi reklam ve otomatik duyuru mesajlarını yönetir.
- **Wysteria.BountySystem**: İsyan eden mahkumlara (T) ödül koyma ve onları avlayan korumalara (CT) para verme sistemidir.
- **Wysteria.CellSearch**: Korumaların mahkum hücrelerini aramasını ve gizli eşyaları bulmasını sağlar.
- **Wysteria.CoinFlip**: Oyuncuların kendi aralarında para (kredi) karşılığında yazı tura atıp bahis oynamasına izin verir.
- **Wysteria.Contraband**: Mahkumların gizlice taşıyabildiği yasaklı eşya/silah sistemini yönetir.
- **Wysteria.ContrabandScanner**: Korumaların, mahkumların üzerinde yasaklı eşya olup olmadığını taramasına yarar.
- **Wysteria.Disguise**: Mahkumların koruma kıyafeti giyerek gizlenmesini sağlar.
- **Wysteria.DoorControl**: Sunucudaki hücre ve harita kapılarının Warden veya adminler tarafından açılıp kapanmasını sağlar.
- **Wysteria.FakeRebel**: Mahkumların isyan etmemiş gibi sahte bir masumiyet durumu oluşturmasını sağlar.
- **Wysteria.FreeDay**: Serbest gün (Freeday) elini başlatır ve o el boyunca kuralları esnetir.
- **Wysteria.GangSystem**: Mahkumların kendi aralarında çete kurmasını, çete seviyelerini ve özel çete yeteneklerini yönetir.
- **Wysteria.GuardDuty**: Gardiyanların aktif görev sürelerini kaydeder ve round bazlı istatistik sunar.
- **Wysteria.GuardJail**: Kural ihlali yapan korumaların hapse atılmasını (CT Ban/Jail) sağlar.
- **Wysteria.GuardOrders**: Korumalara hızlı komutlar ve hedefler verilmesini sağlar.
- **Wysteria.GuardQueue**: CT takımına (gardiyan) geçmek isteyen mahkumlar için kuyruk (sıra) sistemi sağlar.
- **Wysteria.GuardRadio**: Korumaların kendi aralarında özel telsiz (radio) mesajlaşması yapmasını sağlar.
- **Wysteria.GuardStamina**: Korumalar için koşma/zıplama eforunu kısıtlayan stamina sistemini ayarlar.
- **Wysteria.GuardTraining**: Yeni CT oyuncuları için temel koruma eğitimi ve test sürecini yönetir.
- **Wysteria.HostageGuard**: Haritadaki rehinelerin (varsa) korumalar tarafından korunması görevlerini ayarlar.
- **Wysteria.InmateRanks**: Mahkumların oyunda geçirdiği süreye ve isyanlarına göre rütbe atlamasını sağlar.
- **Wysteria.JailBreakEvents**: Çeşitli Jailbreak özel etkinliklerini (Saklambaç, Zombi vb.) yönetir.
- **Wysteria.LastRequest**: Sonda kalan son mahkumun (LR) özel düello ve mini oyun menüsünü açar.
- **Wysteria.MetalDetector**: Haritadaki belirli bölgelere silah tarayıcı (metal detektörü) yerleştirir.
- **Wysteria.PrisonBetrayal**: Mahkumların birbirlerine ihanet edip ödül kazanabileceği sistemi yönetir.
- **Wysteria.PrisonCCTV**: Haritadaki güvenlik kameralarının yöneticiler ve gardiyanlar tarafından izlenmesini sağlar.
- **Wysteria.PrisonEvidence**: Round içerisindeki önemli cezalandırma ve öldürme kayıtlarını RAM üzerinde loglayıp adminlere sunar.
- **Wysteria.PrisonGym**: Hapishane spor salonunda mahkumların çalışarak geçici güç/hız kazanmasını sağlar.
- **Wysteria.PrisonJob**: Mahkumların oyun içinde çeşitli işlerde (örneğin temizlik) çalışarak para kazanmasını sağlar.
- **Wysteria.PrisonLockdown**: Hapishanede genel tecrit (Lockdown) ilan edilmesini sağlar.
- **Wysteria.PrisonMail**: Oyuncuların birbirine özel ve kısıtlı hücre postası (mail) göndermesine imkan tanır.
- **Wysteria.PrisonSchedule**: Hapishane içindeki aktivitelerin (yemek, avlu vb.) programlanmasını ve duyurulmasını yönetir.
- **Wysteria.PrisonerShop**: Mahkumların kazandıkları paralarla yetenek, silah ve can alabildikleri market sistemidir.
- **Wysteria.RebelAdrenaline**: İsyan eden mahkumların kısa süreliğine adrenalin (hız ve ekstra hasar) kazanmasını sağlar.
- **Wysteria.RiotAlarm**: Hapishanede isyan çıktığında çalan uyarı alarmı sistemidir.
- **Wysteria.RoundStats**: El sonlarında kimin ne kadar hasar vurduğunu ve en iyileri (MVP) gösterir.
- **Wysteria.SecretPassage**: Haritadaki gizli isyan tünellerini/geçitlerini yönetir.
- **Wysteria.SimonSaysAI**: "Simon Diyor Ki" oyununu yapay zeka veya otomatik sistem desteğiyle yönetir.
- **Wysteria.TeamBalance**: CT ve T takımları arasındaki oyuncu sayısını (örneğin 1 CT'ye 3 T) otomatik dengeler.
- **Wysteria.VoiceControl**: Mikrofon basma haklarını (sadece Warden konuşabilir, ölüler konuşamaz vb.) düzenler.
- **Wysteria.WardenAnvil**: Warden'ın oyuncuların kafasına örs (Anvil) düşürerek onları cezalandırmasını sağlar.
- **Wysteria.WardenDraw**: Warden'ın yere çizim yaparak (Lazer boya) yer göstermesini sağlar.
- **Wysteria.WardenLazer**: Warden'ın baktığı yere lazer çizgisi çekmesini sağlar.
- **Wysteria.WardenMarker**: Warden'ın haritada gitmeleri gereken hedefi işaretlemesini (Marker) sağlar.
- **Wysteria.WardenMicCheck**: Warden olmak isteyenlerin mikrofon testi/kontrolü menüsünü yönetir.
- **Wysteria.WardenPanic**: Warden'ın acil durumlarda panik butonuna basıp tüm CT'lere uyarı göndermesini sağlar.
- **Wysteria.WardenRules**: Warden'ın koyduğu hapishane kurallarının ekranda listelenmesini sağlar.
- **Wysteria.WardenShield**: Warden'a ekstra zırh ve koruma kalkanı verir.
- **Wysteria.WardenTimer**: Warden'ın belirlediği aktiviteler için geri sayım sayacı oluşturmasını sağlar.
- **Wysteria.WardenVote**: El başında yeni Warden'ın oyuncuların oylamasıyla seçilmesini sağlar.

### 🚀 Kurulum (Installation)
1. Sunucunuzda **Metamod:Source** ve **CounterStrikeSharp** kurulu olmalıdır.
2. `Plugins/` klasöründeki dosyaları sunucunuzun `csgo/addons/counterstrikesharp/plugins/` klasörüne atın. `Wysteria.Core.dll` dosyasının diğer pluginlerle aynı klasörde olduğundan emin olun.
3. Konfigürasyon dosyalarını `Configs/` altından `addons/counterstrikesharp/configs/` içine yükleyin. Sunucuyu yeniden başlatın.

### 🔧 Geliştirme (Development)
 Kendi bilgisayarınızda derlemek isterseniz `dotnet build -c Release` kullanabilirsiniz.

---

## 🇬🇧 English

### 📦 Architecture
The system runs on **1 Core Module** and **50 independent modules** (plugins). The Core manages shared resources such as permissions, logging, round states, and Warden assignments, while the other plugins operate independently depending only on the Core.

### 🧩 Plugin List and Functions
Below is a list of all modules in the Wysteria CS2 ecosystem and what they do:

- **Wysteria.Core**: The center of all plugins. Provides the API, shared variables, and permission checks.
- **Wysteria.AdminTools**: Essential tools for admins to manage players.
- **Wysteria.Advertiser**: Manages in-server advertisements and automated broadcast messages.
- **Wysteria.BountySystem**: A system that places a bounty on rebelling prisoners (T) and rewards guards (CT) who hunt them down.
- **Wysteria.CellSearch**: Allows guards to search prisoner cells and find hidden contrabands.
- **Wysteria.CoinFlip**: Allows players to flip coins and bet their credits against each other.
- **Wysteria.Contraband**: Manages the hidden contraband/weapon system for prisoners.
- **Wysteria.ContrabandScanner**: Allows guards to scan prisoners for hidden contrabands.
- **Wysteria.Disguise**: Allows prisoners to disguise themselves by wearing guard uniforms.
- **Wysteria.DoorControl**: Allows the Warden or admins to open/close cell and map doors.
- **Wysteria.FakeRebel**: Allows a prisoner to fake innocence while actually rebelling.
- **Wysteria.FreeDay**: Initiates a Freeday round where standard prison rules are relaxed.
- **Wysteria.GangSystem**: Allows prisoners to create gangs, level them up, and unlock special gang abilities.
- **Wysteria.GuardDuty**: Records active guard duty times and provides round-based statistics.
- **Wysteria.GuardJail**: Jails or bans guards (CTs) who break the rules.
- **Wysteria.GuardOrders**: Allows quick issuing of orders and targets to guards.
- **Wysteria.GuardQueue**: Manages a priority queue for prisoners waiting to join the CT team.
- **Wysteria.GuardRadio**: Provides a private radio messaging system for guards.
- **Wysteria.GuardStamina**: Configures a stamina system that limits running/jumping effort for guards.
- **Wysteria.GuardTraining**: Manages the basic training and testing process for new CT players.
- **Wysteria.HostageGuard**: Sets up protection tasks for guards regarding hostages on the map (if any).
- **Wysteria.InmateRanks**: Allows prisoners to rank up based on their playtime and rebellion actions.
- **Wysteria.JailBreakEvents**: Manages various Jailbreak special events (Hide and Seek, Zombie, etc.).
- **Wysteria.LastRequest**: Opens a special duel and mini-game menu for the last surviving prisoner (LR).
- **Wysteria.MetalDetector**: Places weapon scanners (metal detectors) in specific areas of the map.
- **Wysteria.PrisonBetrayal**: A system where prisoners can betray each other for rewards.
- **Wysteria.PrisonCCTV**: Allows admins and guards to monitor the predefined CCTV camera points on the map.
- **Wysteria.PrisonEvidence**: Logs important punishment and kill events into a low-cost memory RAM log for admins.
- **Wysteria.PrisonGym**: Allows prisoners to work out in the gym to gain temporary strength/speed.
- **Wysteria.PrisonJob**: Allows prisoners to earn money by working on various jobs (e.g., cleaning) in the game.
- **Wysteria.PrisonLockdown**: Enforces a general lockdown in the prison.
- **Wysteria.PrisonMail**: Allows players to send private cell mails to each other with a constrained inbox limit.
- **Wysteria.PrisonSchedule**: Manages the jailbreak day schedule activities and broadcasts current events.
- **Wysteria.PrisonerShop**: A shop system where prisoners can buy abilities, weapons, and health using earned credits.
- **Wysteria.RebelAdrenaline**: Gives rebelling prisoners short-term adrenaline (speed and extra damage).
- **Wysteria.RiotAlarm**: The warning alarm system that goes off when a riot breaks out in the prison.
- **Wysteria.RoundStats**: Displays who dealt the most damage and the MVPs at the end of the round.
- **Wysteria.SecretPassage**: Manages secret rebellion tunnels/passages on the map.
- **Wysteria.SimonSaysAI**: Manages the "Simon Says" game with AI or automated system support.
- **Wysteria.TeamBalance**: Automatically balances the player ratio between CT and T teams (e.g., 1 CT to 3 Ts).
- **Wysteria.VoiceControl**: Regulates microphone permissions (e.g., only the Warden can speak, dead players are muted).
- **Wysteria.WardenAnvil**: Allows the Warden to drop an anvil on players' heads to punish them.
- **Wysteria.WardenDraw**: Allows the Warden to draw lines (laser paint) on the ground to show paths.
- **Wysteria.WardenLazer**: Allows the Warden to draw a straight laser line to where they are looking.
- **Wysteria.WardenMarker**: Allows the Warden to place a marker on the map as a destination target.
- **Wysteria.WardenMicCheck**: Manages the microphone test/check menu for players who want to become the Warden.
- **Wysteria.WardenPanic**: Allows the Warden to press a panic button in emergencies to alert all CTs.
- **Wysteria.WardenRules**: Displays the prison rules set by the Warden on the screen.
- **Wysteria.WardenShield**: Grants the Warden extra armor and a protection shield.
- **Wysteria.WardenTimer**: Allows the Warden to set up low-frequency event-driven timers for activities.
- **Wysteria.WardenVote**: Allows players to select the new Warden by voting at the start of the round.

### 🚀 Installation
1. Your server must have **Metamod:Source** and **CounterStrikeSharp** installed.
2. Upload the files inside the `Plugins/` folder to your server's `csgo/addons/counterstrikesharp/plugins/` directory. Ensure `Wysteria.Core.dll` is in the same directory as the other plugins.
3. Upload the configuration files from `Configs/` to `addons/counterstrikesharp/configs/`. Restart your server completely.

### 🔧 Development
build it on your PC, you can use `dotnet build -c Release`. 