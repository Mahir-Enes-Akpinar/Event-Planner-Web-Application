# Event Planner

ASP.NET Core MVC tabanlı etkinlik planlama uygulaması. Uygulama .NET 10, EF Core 10,
ASP.NET Identity, SQL Server ve Geoapify/Leaflet kullanır.

## Yerel çalıştırma

1. `.env.example` dosyasını `.env` adıyla kopyalayın ve tüm secret değerlerini değiştirin.
2. `docker compose up --build` çalıştırın.
3. Uygulamayı `http://localhost:8080` adresinden açın.

Production ortamında `APP_ENVIRONMENT=Production` kullanılmalı ve uygulama TLS sonlandıran güvenilir
bir reverse proxy arkasında yayınlanmalıdır.

Migration işlemi web uygulamasından ayrı `migrate` container'ında tamamlanır. Uygulama
yalnızca migration başarıyla bittikten sonra başlar. `/health/live` process, `/health/ready`
ise SQL Server erişimini denetler.

## Mevcut veritabanını yükseltme

Yükseltmeden önce doğrulanmış bir SQL Server yedeği alın. Migration; kullanıcı adı, e-posta,
kategori ve katılım benzersizliği için indeks oluşturur. Çakışan mevcut kayıtlar varsa migration
bilerek durur; kayıtlar kayıpsız biçimde operatör tarafından düzeltilmelidir. Eski SHA-256 parola
değerleri korunur ve kullanıcının ilk başarılı girişinde Identity formatına yükseltilir.

Önce migration SQL çıktısını inceleyin:

```powershell
dotnet ef migrations script --idempotent --project WebApplication1/WebApplication1.csproj
```

## Yapılandırma

Secret değerler repoya yazılmaz. Yapılandırma anahtarları `.env.example` içindedir. Geoapify
browser anahtarı yalnızca uygulama domain'ine, server anahtarı yalnızca kullanılan API'lere
kısıtlanmalıdır. SMTP ve bootstrap admin parolaları deployment secret store'dan verilmelidir.

Yüklenen görseller `data/uploads`, Data Protection anahtarları `data/keys` altında tutulur;
bu volume'lar ile SQL Server verisi düzenli yedeklenmelidir.
