# Panel Kurulum Rehberi / Game Panel Installation

## Türkçe

Bu rehber, ProGame, CS2Center ve benzeri hazır oyun panelleri içindir. Panel kullanıyorsanız source repository’sini yüklemezsiniz; hazır DLL ve JSON dosyalarını ilgili klasörlere koyarsınız.

### 1. Gereken dosyayı seçin

- GitHub **Releases** bölümündeki hazır deployment paketini indirin.
- Release yoksa **Actions → Build Wysteria CS2 → Artifacts** bölümünden `wysteria-cs2-deploy` paketini indirin.
- `src/` klasörünü veya `.cs` dosyalarını yüklemeyin.

### 2. DLL dosyalarını yükleyin

Arşivdeki `plugins/Wysteria/` klasörünün içeriğini panelde şu klasöre yükleyin:

```text
<server_root>/game/csgo/addons/counterstrikesharp/plugins/Wysteria/
```

`Wysteria.Core.dll` zorunludur ve bütün Wysteria modülleriyle aynı klasörde durmalıdır.

### 3. Config dosyalarını yükleyin

Arşivdeki `configs/` klasörünün içindeki JSON dosyalarını şuraya yükleyin:

```text
<server_root>/game/csgo/addons/counterstrikesharp/configs/
```

Bir modülü kullanmayacaksanız DLL’sini ve config’ini yüklemeyebilirsiniz. Core’u ise modül kullanacaksanız mutlaka yükleyin.

### 4. İlk ayarlar

Her modülün JSON dosyasında:

- `Enabled`: `true` ise modül açık, `false` ise kapalıdır.
- `AllowedMaps`: `jb_,jail_` gibi harita adlarını veya prefix’lerini virgülle kabul eder.
- `GlobalCooldown`: genel spam korumasıdır.
- Diğer alanlar, modülün kendi README’sinde açıklanır.

### 5. Yeniden başlatma

Dosyaları yükledikten sonra panelden sunucuyu tamamen restart edin. Ardından konsolda:

```text
meta list
css_plugins list
```

Core veya modül listede yoksa dosya yolunu, DLL adını ve CounterStrikeSharp logunu kontrol edin.

### Sık yapılan hata

GitHub’da görülen `src/Wysteria.PrisonJob/PrisonJobPlugin.cs` dosyası kaynak koddur. Panele atılacak dosya `Wysteria.PrisonJob.dll` dosyasıdır. Aynı şekilde config olarak da repository içindeki `Configs/Wysteria.PrisonJob.json` dosyası kullanılır.

## English

This guide is for ProGame, CS2Center, and similar game panels. A panel installation uses the ready DLL and JSON files, not the source repository.

### 1. Download the deployment files

Download a package from GitHub **Releases**, or use `wysteria-cs2-deploy` under **Actions → Build Wysteria CS2 → Artifacts**. Do not upload `src/` or `.cs` files.

### 2. Upload DLLs

Upload the contents of `plugins/Wysteria/` to:

```text
<server_root>/game/csgo/addons/counterstrikesharp/plugins/Wysteria/
```

`Wysteria.Core.dll` is required beside every Wysteria module DLL.

### 3. Upload configs

Upload the JSON files from `configs/` to:

```text
<server_root>/game/csgo/addons/counterstrikesharp/configs/
```

### 4. Configure and restart

Set `Enabled`, `AllowedMaps`, and the module-specific values in each JSON file. Fully restart the server, then verify with:

```text
meta list
css_plugins list
```

If a module is missing, check the exact folder, DLL name, and CounterStrikeSharp log.

The `.cs` file shown in a GitHub source folder is not the file you upload to a panel. Upload the matching `.dll`, plus its JSON config.