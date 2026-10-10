# Event Planner

ASP.NET Core MVC tabanlı etkinlik planlama uygulaması. Uygulama .NET 10, EF Core 10,
ASP.NET Identity, PostgreSQL ve Geoapify/Leaflet kullanır.

## Yerel çalıştırma

1. `.env.example` dosyasını `.env` adıyla kopyalayın ve tüm secret değerlerini değiştirin.
2. `docker compose up --build` çalıştırın.
3. Uygulamayı `http://localhost:8080` adresinden açın.

Production ortamında `ASPNETCORE_ENVIRONMENT=Production` kullanılmalı ve uygulama TLS sonlandıran
güvenilir bir reverse proxy arkasında yayınlanmalıdır.

Migration işlemi yerelde `migrate` container'ında tamamlanır. Render ortamında ise
`Database__MigrateOnStartup=true` verildiğinde uygulama açılışta idempotent EF migration uygular.
`/health` ve `/health/live` process'i, `/health/ready` ise database erişimini denetler.

## PostgreSQL migration notu

Aktif migration seti PostgreSQL için `WebApplication1/Migrations/Postgres` altındadır.
Eski SQL Server migration geçmişi silinmemiştir; referans için
`WebApplication1/Migrations/SqlServerLegacy` altında tutulur ve derlemeye dahil edilmez.

Bu migration yapısı temiz PostgreSQL database içindir. Mevcut SQL Server verisini Supabase'e
taşımaz.

Migration SQL çıktısını incelemek için:

```powershell
dotnet ef migrations script --project WebApplication1/WebApplication1.csproj
```

## Yapılandırma

Secret değerler repoya yazılmaz. Yapılandırma anahtarları `.env.example` içindedir. Geoapify
server anahtarı yalnızca kullanılan API'lere kısıtlanmalıdır. SMTP ve bootstrap admin parolaları
deployment secret store'dan verilmelidir.

Yüklenen görseller `data/uploads`, Data Protection anahtarları `data/keys` altında tutulur.
Bu path'ler yerelde Docker volume ile saklanır.

## Render + Supabase PostgreSQL deployment

### Supabase database

1. Supabase'de yeni bir project oluşturun.
2. Project Settings > Database alanından PostgreSQL connection string'i alın.
3. Render için pooled veya direct connection string kullanabilirsiniz; connection string'i
   `ConnectionStrings__DefaultConnection` environment variable olarak verin.
4. Supabase SSL ister. Connection string'e gerekirse `SSL Mode=Require;Trust Server Certificate=true`
   ekleyin.

Production connection string kaynak koda yazılmaz. Örnek format:

```text
Host=<supabase-host>;Port=5432;Database=postgres;Username=postgres.<project-ref>;Password=<password>;SSL Mode=Require;Trust Server Certificate=true
```

### Render Web Service

1. Render'da New > Web Service seçin.
2. GitHub reposunu bağlayın.
3. Environment olarak Docker seçin. Dockerfile yolu: `WebApplication1/Dockerfile`.
4. Aşağıdaki environment variable'ları ekleyin:

```text
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=<Supabase PostgreSQL connection string>
Database__MigrateOnStartup=true
BootstrapAdmin__Email=<admin email>
BootstrapAdmin__Password=<strong admin password>
EmailSettings__SMTPHost=<smtp host>
EmailSettings__SMTPPort=587
EmailSettings__SMTPUser=<smtp user>
EmailSettings__SMTPPass=<smtp password>
EmailSettings__FromEmail=<from email>
Geoapify__ServerApiKey=<optional server key>
Storage__ImagePath=/app/data/uploads
Storage__DataProtectionPath=/app/data/keys
```

Render `PORT` variable'ını otomatik verir; uygulama container içinde `0.0.0.0:$PORT`
üzerinden dinler.

### Migration

Render'da `Database__MigrateOnStartup=true` ile migration uygulama açılışında uygulanır.
Bu işlem database'i silmez; `EnsureDeleted` kullanılmaz.

Manuel migration için:

```powershell
dotnet ef database update --project WebApplication1/WebApplication1.csproj
```

### Render filesystem uyarısı

Mevcut upload sistemi profil ve etkinlik görsellerini local diske yazar. Render filesystem'i
persistent disk bağlanmadıkça kalıcı kabul edilmemelidir. Kalıcı görsel saklama için ileride
Cloudinary, S3 veya benzeri bir storage servisine geçilmelidir.

### Deploy sonrası test

- `/` ana sayfa
- `/health`
- `/health/ready`
- Register
- Login
- Logout
- Event listeleme
- Event oluşturma
- Admin paneli
- Admin event onaylama
- Kullanıcının event'e katılması
- Redeploy sonrasında verilerin Supabase'de kalması
