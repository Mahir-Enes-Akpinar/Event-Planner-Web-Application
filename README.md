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
server anahtarı yalnızca kullanılan API'lere kısıtlanmalıdır. SMTP/Resend ve bootstrap admin
parolaları deployment secret store'dan verilmelidir.

Mail gönderimi için iki yol desteklenir: `EmailProvider=Resend` ve `Resend__ApiKey` verildiğinde
Resend HTTP API kullanılır; aksi halde eski SMTP ayarları ile devam edilir. Render free tier'da
SMTP portları zaman aşımına düşebildiği için production ortamında Resend tercih edilir.

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
EmailSettings__FromName=Event Planner
EmailProvider=Resend
Resend__ApiKey=<resend api key>
Resend__BaseUrl=https://api.resend.com/
Geoapify__ServerApiKey=<optional server key>
Storage__ImagePath=/app/data/uploads
Storage__DataProtectionPath=/app/data/keys
```

Resend kullanırken `EmailSettings__FromEmail` doğrulanmış bir Resend domain adresi olmalıdır.
Domain henüz doğrulanmadıysa test için Resend'in izin verdiği test sender adresini kullanın;
aksi halde outbox kayıtları `LastError` alanında domain/sender hatası ile kalır.

Render `PORT` variable'ını otomatik verir; uygulama container içinde `0.0.0.0:$PORT`
üzerinden dinler.

### Migration

Render'da `Database__MigrateOnStartup=true` ile migration uygulama açılışında uygulanır.
Bu işlem database'i silmez; `EnsureDeleted` kullanılmaz.

Manuel migration için:

```powershell
dotnet ef database update --project WebApplication1/WebApplication1.csproj
```

### Upload storage

Profil ve etkinlik görselleri `/media/{fileName}` URL'i ile gösterilir. Dosya geçici olarak
`Storage__ImagePath` altına yazılır, ayrıca PostgreSQL'deki `StoredImages` tablosuna `bytea`
olarak kaydedilir. Render filesystem'i kalıcı olmasa bile redeploy/restart sonrasında `/media`
isteği geldiğinde dosya veritabanından geri oluşturulur.

Büyük trafik veya yüksek görsel hacmi için ileride Cloudinary, S3 veya benzeri object storage'a
geçmek daha doğru olur; mevcut yapı küçük/orta ölçekli deployment için Supabase PostgreSQL'i
kalıcı storage olarak kullanır.

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
