# GitHub’dan indirme ve derleme / Downloading from GitHub

## Türkçe

Sunucu paneline kurulum yapacaksanız GitHub’daki kaynak klasörlerini değil, derleme sonucundaki dağıtım paketini kullanın.

### Hazır paket

Repository sahibi bir release yayınladıysa **Releases** bölümünden indirin. Release yoksa:

1. **Actions** sekmesine girin.
2. `Build Wysteria CS2` workflow’unu açın.
3. Başarılı çalışmanın altındaki `wysteria-cs2-deploy` artifact’ini indirin.
4. Arşivi açın ve `plugins/Wysteria/` içindeki DLL’leri, `configs/` içindeki JSON’ları paneldeki ilgili CounterStrikeSharp klasörlerine yükleyin.

Panel için `src/` klasörü, `.cs` dosyaları veya repository’nin tamamı kullanılmaz.

### Kendi paketinizi oluşturma

Linux/macOS:

```bash
git clone https://github.com/<kullanici>/<repo>.git
cd <repo>
bash scripts/build.sh
```

Windows:

```powershell
./scripts/build.ps1
```

Hazır dosyalar `artifacts/deploy/` altında oluşur. Sunucuya yalnızca bu klasörün `plugins/` ve `configs/` içeriğini gönderin.

## English

For a panel installation, use the deployment artifact rather than the source folders shown on GitHub.

If a release exists, download it from **Releases**. Otherwise open **Actions**, run or open `Build Wysteria CS2`, and download the `wysteria-cs2-deploy` artifact from a successful run.

Upload only the DLL files under `plugins/Wysteria/` and the JSON files under `configs/`. Do not upload `src/`, `.cs` files, or the entire repository.

To build locally:

```bash
bash scripts/build.sh
```

PowerShell:

```powershell
./scripts/build.ps1
```

The panel-ready files are written to `artifacts/deploy/`.