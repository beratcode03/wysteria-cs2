# Wysteria.Core

## Türkçe

`Wysteria.Core`, Wysteria Jailbreak eklentilerinin ortak çalışma katmanıdır. Round durumunu, aktif Warden bilgisini, hücrelerin açılıp açılmadığını ve ortak mesaj/cooldown yardımcılarını yönetir.

Bu dosya tek başına oyuncuya özellik eklemez. Ancak diğer Wysteria modüllerinin tamamı buna bağlıdır. Bu yüzden **önce Core, sonra modüller** yüklenmelidir.

Yeni ekonomi ve Jailbreak oyun etkisi modülleri `IJailbreakAdapter` sözleşmesini
kullanır. Hedef sunucunun gerçek adapter’ı kaydedilene kadar varsayılan adapter
kredi düşürmez, kredi vermez, contraband uydurmaz ve kapı işlemi uygulamaz.

### Panel kurulumu

GitHub’dan hazır dağıtım dosyası indirdiyseniz `Wysteria.Core.dll` dosyasını şu klasöre yükleyin:

```text
<sunucu klasörü>/game/csgo/addons/counterstrikesharp/plugins/Wysteria/
```

Kaynak kodu indirdiyseniz doğrudan `.cs` dosyalarını yüklemeyin. Önce proje kökünde `bash scripts/build.sh` çalıştırın; sunucuya `artifacts/deploy/plugins/Wysteria/Wysteria.Core.dll` dosyasını gönderin.

### Sağladığı komutlar

- `!wysteria_w` / `css_wysteria_w`: Canlı round içinde ilk geçerli CT oyuncusunun Warden rolünü almasını sağlar.
- `!wysteria_opencells` / `css_wysteria_opencells`: Aktif Warden’ın hücre durumunu açık olarak işaretlemesini sağlar.

Komutların çalışması için round’un başlamış olması gerekir. Round bitince Core, Warden ve hücre durumunu sıfırlar.

### Yetki ve ayar özeti

- `!wysteria_w`: Canlı round’daki ilk geçerli CT oyuncusu kullanabilir.
- `!wysteria_opencells`: Yalnızca aktif Warden kullanabilir.
- Core için ayrı bir JSON config dosyası yoktur.
- Core Source 2 console cvar’ı kullanmaz; ortak durum runtime tarafından yönetilir.

### Sorun giderme

Bir modül `Wysteria.Core.dll` bulunamadı diyorsa Core dosyasının modüllerle **aynı `plugins/Wysteria/` klasöründe** olduğundan emin olun. Core dosyasını yeniden adlandırmayın ve yalnızca modülü reload etmek yerine sunucuyu tamamen yeniden başlatın.

## English

`Wysteria.Core` is the shared runtime used by every Wysteria Jailbreak module. It owns round state, the active Warden, cell state, and common message and cooldown helpers.

It does not add a player-facing feature by itself, but every other Wysteria module depends on it. Install **Core first, then the modules**.

New economy and Jailbreak gameplay modules use the `IJailbreakAdapter`
contract. Until a real server adapter is registered, the default adapter does
not deduct or award credits, invent contraband, or manipulate doors.

### Panel installation

For a ready-made release, upload `Wysteria.Core.dll` to:

```text
<server folder>/game/csgo/addons/counterstrikesharp/plugins/Wysteria/
```

If you downloaded the source repository, do not upload the `.cs` files. Build it with `bash scripts/build.sh`, then upload `artifacts/deploy/plugins/Wysteria/Wysteria.Core.dll`.

### Commands

- `!wysteria_w` / `css_wysteria_w`: lets the first valid CT player claim the Warden role during a live round.
- `!wysteria_opencells` / `css_wysteria_opencells`: lets the active Warden mark the cell state as open.

The round must be live. Core resets the Warden and cell state when the round ends.

### Permissions and configuration

- `!wysteria_w`: available to the first valid CT player during a live round.
- `!wysteria_opencells`: available only to the active Warden.
- Core has no separate JSON config file.
- Core does not use Source 2 console cvars; shared state is managed by the runtime.

### Troubleshooting

If a module reports that `Wysteria.Core.dll` is missing, make sure Core is in the **same `plugins/Wysteria/` directory** as the modules. Do not rename the DLL, and restart the server after correcting the files.