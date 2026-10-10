using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WebApplication1.Models;

#nullable disable

namespace WebApplication1.Migrations;

[DbContext(typeof(Context))]
[Migration("20241129000000_InitialSchema")]
public sealed class InitialSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Idempotent by design: legacy installations already contain these tables but do not
        // have a baseline migration record. Fresh installations need the original schema so
        // the historical AddRole migration and the hardening migration can run in order.
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'[Kullanicilar]', N'U') IS NULL
            BEGIN
                CREATE TABLE [Kullanicilar] (
                    [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Kullanicilar] PRIMARY KEY,
                    [UserName] nvarchar(50) NOT NULL,
                    [Password] nvarchar(256) NOT NULL,
                    [Email] nvarchar(100) NOT NULL,
                    [Location] nvarchar(max) NOT NULL,
                    [FirstName] nvarchar(50) NOT NULL,
                    [LastName] nvarchar(50) NOT NULL,
                    [BirthDate] datetime2 NOT NULL,
                    [Gender] nvarchar(10) NOT NULL,
                    [PhoneNumber] nvarchar(15) NOT NULL,
                    [ProfilePicture] nvarchar(max) NOT NULL
                );
            END;

            IF OBJECT_ID(N'[Kategoriler]', N'U') IS NULL
            BEGIN
                CREATE TABLE [Kategoriler] (
                    [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Kategoriler] PRIMARY KEY,
                    [Name] nvarchar(50) NOT NULL
                );
            END;

            IF OBJECT_ID(N'[Etkinlikler]', N'U') IS NULL
            BEGIN
                CREATE TABLE [Etkinlikler] (
                    [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Etkinlikler] PRIMARY KEY,
                    [Name] nvarchar(100) NOT NULL,
                    [Description] nvarchar(500) NOT NULL,
                    [Date] datetime2 NOT NULL,
                    [Duration] time NOT NULL,
                    [Location] nvarchar(max) NOT NULL,
                    [ImageUrl] nvarchar(max) NOT NULL,
                    [Status] int NOT NULL,
                    [CreatedByUserId] int NOT NULL,
                    CONSTRAINT [FK_Etkinlikler_Kullanicilar_CreatedByUserId]
                        FOREIGN KEY ([CreatedByUserId]) REFERENCES [Kullanicilar] ([Id]) ON DELETE NO ACTION
                );
                CREATE INDEX [IX_Etkinlikler_CreatedByUserId] ON [Etkinlikler] ([CreatedByUserId]);
            END;

            IF OBJECT_ID(N'[KullaniciKategorileri]', N'U') IS NULL
            BEGIN
                CREATE TABLE [KullaniciKategorileri] (
                    [KullaniciId] int NOT NULL,
                    [KategoriId] int NOT NULL,
                    [Id] int NOT NULL CONSTRAINT [DF_KullaniciKategorileri_Id] DEFAULT 0,
                    CONSTRAINT [PK_KullaniciKategorileri] PRIMARY KEY ([KullaniciId], [KategoriId]),
                    CONSTRAINT [FK_KullaniciKategorileri_Kullanicilar_KullaniciId] FOREIGN KEY ([KullaniciId]) REFERENCES [Kullanicilar] ([Id]) ON DELETE CASCADE,
                    CONSTRAINT [FK_KullaniciKategorileri_Kategoriler_KategoriId] FOREIGN KEY ([KategoriId]) REFERENCES [Kategoriler] ([Id]) ON DELETE CASCADE
                );
                CREATE INDEX [IX_KullaniciKategorileri_KategoriId] ON [KullaniciKategorileri] ([KategoriId]);
            END;

            IF OBJECT_ID(N'[EtkinlikKategorileri]', N'U') IS NULL
            BEGIN
                CREATE TABLE [EtkinlikKategorileri] (
                    [EtkinlikId] int NOT NULL,
                    [KategoriId] int NOT NULL,
                    [Id] int NOT NULL CONSTRAINT [DF_EtkinlikKategorileri_Id] DEFAULT 0,
                    CONSTRAINT [PK_EtkinlikKategorileri] PRIMARY KEY ([EtkinlikId], [KategoriId]),
                    CONSTRAINT [FK_EtkinlikKategorileri_Etkinlikler_EtkinlikId] FOREIGN KEY ([EtkinlikId]) REFERENCES [Etkinlikler] ([Id]) ON DELETE CASCADE,
                    CONSTRAINT [FK_EtkinlikKategorileri_Kategoriler_KategoriId] FOREIGN KEY ([KategoriId]) REFERENCES [Kategoriler] ([Id]) ON DELETE CASCADE
                );
                CREATE INDEX [IX_EtkinlikKategorileri_KategoriId] ON [EtkinlikKategorileri] ([KategoriId]);
            END;

            IF OBJECT_ID(N'[Katilimlar]', N'U') IS NULL
            BEGIN
                CREATE TABLE [Katilimlar] (
                    [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Katilimlar] PRIMARY KEY,
                    [KullaniciId] int NOT NULL,
                    [EtkinlikId] int NOT NULL,
                    [ParticipationDate] datetime2 NOT NULL,
                    CONSTRAINT [FK_Katilimlar_Kullanicilar_KullaniciId] FOREIGN KEY ([KullaniciId]) REFERENCES [Kullanicilar] ([Id]) ON DELETE CASCADE,
                    CONSTRAINT [FK_Katilimlar_Etkinlikler_EtkinlikId] FOREIGN KEY ([EtkinlikId]) REFERENCES [Etkinlikler] ([Id]) ON DELETE CASCADE
                );
                CREATE INDEX [IX_Katilimlar_KullaniciId] ON [Katilimlar] ([KullaniciId]);
                CREATE INDEX [IX_Katilimlar_EtkinlikId] ON [Katilimlar] ([EtkinlikId]);
            END;

            IF OBJECT_ID(N'[Mesajlar]', N'U') IS NULL
            BEGIN
                CREATE TABLE [Mesajlar] (
                    [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Mesajlar] PRIMARY KEY,
                    [SenderId] int NOT NULL,
                    [EtkinlikId] int NOT NULL,
                    [Content] nvarchar(1000) NOT NULL,
                    [SentDate] datetime2 NOT NULL,
                    CONSTRAINT [FK_Mesajlar_Kullanicilar_SenderId] FOREIGN KEY ([SenderId]) REFERENCES [Kullanicilar] ([Id]) ON DELETE CASCADE,
                    CONSTRAINT [FK_Mesajlar_Etkinlikler_EtkinlikId] FOREIGN KEY ([EtkinlikId]) REFERENCES [Etkinlikler] ([Id]) ON DELETE CASCADE
                );
                CREATE INDEX [IX_Mesajlar_SenderId] ON [Mesajlar] ([SenderId]);
                CREATE INDEX [IX_Mesajlar_EtkinlikId] ON [Mesajlar] ([EtkinlikId]);
            END;

            IF OBJECT_ID(N'[Puanlar]', N'U') IS NULL
            BEGIN
                CREATE TABLE [Puanlar] (
                    [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Puanlar] PRIMARY KEY,
                    [KullaniciId] int NOT NULL,
                    [Value] int NOT NULL,
                    [EarnedDate] datetime2 NOT NULL,
                    CONSTRAINT [FK_Puanlar_Kullanicilar_KullaniciId] FOREIGN KEY ([KullaniciId]) REFERENCES [Kullanicilar] ([Id]) ON DELETE CASCADE
                );
                CREATE INDEX [IX_Puanlar_KullaniciId] ON [Puanlar] ([KullaniciId]);
            END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Deliberately non-destructive. This migration may only be a baseline marker on an
        // upgraded installation, so rolling it back must never drop pre-existing user data.
    }
}
