# Wysteria CS2 — Panel paketi / Panel package

## Türkçe

Bu paket derlenmiş CounterStrikeSharp plugin DLL’lerini ve JSON ayar dosyalarını içerir. Panel kurmak için source repository’sini veya `.cs` dosyalarını yüklemeniz gerekmez.

### Kurulum

1. CounterStrikeSharp With Runtime ve Metamod:Source 2.x’in sunucuda kurulu olduğundan emin olun.
2. `plugins/Wysteria/` klasöründeki bütün DLL dosyalarını şu sunucu klasörüne yükleyin:

   ```text
   <sunucu>/game/csgo/addons/counterstrikesharp/plugins/Wysteria/
   ```

3. `configs/` klasöründeki bütün JSON dosyalarını şuraya yükleyin:

   ```text
   <sunucu>/game/csgo/addons/counterstrikesharp/configs/
   ```

4. Sunucuyu tamamen yeniden başlatın.
5. Konsolda `meta list` ve `css_plugins list` ile `Wysteria.Core` ve modülleri kontrol edin.

`Wysteria.Core.dll` bütün Wysteria modülleriyle aynı plugin klasöründe bulunmalıdır. JSON dosyaları cvar değildir; değişiklikten sonra restart gerekir.

### Dosya yapısı

```text
plugins/Wysteria/
  Wysteria.Core.dll
  Wysteria.PrisonJob.dll
  ... diğer Wysteria DLL dosyaları
configs/
  Wysteria.PrisonJob.json
  ... diğer Wysteria JSON dosyaları
```

Kullanmak istemediğiniz bir modülün DLL ve JSON dosyasını yüklemeyebilirsiniz. Core’u ise kullanacağınız modüller için mutlaka yükleyin.

## English

This package contains compiled CounterStrikeSharp plugin DLLs and JSON configuration files. You do not need to upload the source repository or any `.cs` files to a game panel.

### Installation

1. Install CounterStrikeSharp With Runtime and Metamod:Source 2.x on the server.
2. Upload every DLL from `plugins/Wysteria/` to:

   ```text
   <server>/game/csgo/addons/counterstrikesharp/plugins/Wysteria/
   ```

3. Upload every JSON from `configs/` to:

   ```text
   <server>/game/csgo/addons/counterstrikesharp/configs/
   ```

4. Fully restart the server.
5. Verify `Wysteria.Core` and the modules with `meta list` and `css_plugins list`.

`Wysteria.Core.dll` must be beside all Wysteria module DLLs. The JSON files are configuration files, not console cvars; restart after editing them.

The package includes a bilingual module index and detailed README files for every plugin.