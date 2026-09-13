# Wysteria.TeamBalance

## Plugin Ne İşe Yarar?
Bu eklenti, Wysteria CS2 Jailbreak ekosisteminin bir parçası olarak çalışır. Modülün temel amacı, oyun içi etkileşimleri ve admin denetimlerini sorunsuz bir şekilde CS2/Source 2 altyapısına entegre etmektir. 

## Özellikler
- **Wysteria Core Entegrasyonu:** Modül bağımsız çalışmaz, Wysteria.Core tarafından sağlanan altyapıyı (WysteriaMessages, ConfigStore, WardenService) kullanır.
- **Güvenli State Yönetimi:** Round ve Map restartlarında gerekli timer ve stateler otomatik temizlenir.
- **Yüksek Performans:** Her kare (frame) başına gereksiz döngü (polling) barındırmaz, asenkron eventleri baz alır.

## Kurulum
Bu modül otomatik olarak `Wysteria.Core`'a bağımlı şekilde derlenir. 
Sunucunuza yüklemek için `.dll` dosyasını `csgo/addons/counterstrikesharp/plugins/Wysteria.TeamBalance/` dizinine atın.

## Komutlar
Modül içerisinde standart CSS komutlarına ek olarak `wysteria_` önekiyle güncellenmiş komutlar mevcuttur.

## Admin / Z Yetkisi
Komutlar varsayılan yetki flaglerine (`@css/generic`, `@css/ban` vs.) tabidir. Ancak **Root ('Z' Yetkisi)** bulunan yöneticiler sistemdeki her tür kontrolü bypass edebilir.

## Configuration
Eklentinin ayarları `Wysteria.TeamBalance.json` dosyasına kaydedilir. Modül yüklendiğinde mevcut değilse otomatik olarak default değerlerle oluşturulur. Sunucu çökmelerine karşı hata toleransı mevcuttur.

## SQL / Veritabanı
Bu modül ekstradan bir SQL bağlantısı gerektiriyorsa, açılışta `CREATE TABLE IF NOT EXISTS` ile kendi tablosunu oluşturur. SQL kullanmayan modüllerde gereksiz bağlantı sağlanmaz.

## Görsel Efektler ve Sesler
Ekstra dışarıdan dosya indirilmesi (FastDL spam'i) gerekmez. CS2/Source2'nin native sesleri ve entityleri kullanılmıştır.
