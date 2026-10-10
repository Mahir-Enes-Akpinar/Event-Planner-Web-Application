IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241129000000_InitialSchema'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241129000000_InitialSchema'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20241129000000_InitialSchema', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241130094315_AddRoleToUser'
)
BEGIN
    ALTER TABLE [Kullanicilar] ADD [Role] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241130094315_AddRoleToUser'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20241130094315_AddRoleToUser', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Katilimlar] DROP CONSTRAINT [FK_Katilimlar_Kullanicilar_KullaniciId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Mesajlar] DROP CONSTRAINT [FK_Mesajlar_Kullanicilar_SenderId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Puanlar] DROP CONSTRAINT [FK_Puanlar_Kullanicilar_KullaniciId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    DROP INDEX [IX_Puanlar_KullaniciId] ON [Puanlar];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    DROP INDEX [IX_Katilimlar_KullaniciId] ON [Katilimlar];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    EXEC sp_rename N'[Kullanicilar].[Password]', N'PasswordHash', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[KullaniciKategorileri]') AND [c].[name] = N'Id');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [KullaniciKategorileri] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [KullaniciKategorileri] DROP COLUMN [Id];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    DECLARE @var1 nvarchar(max);
    SELECT @var1 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[EtkinlikKategorileri]') AND [c].[name] = N'Id');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [EtkinlikKategorileri] DROP CONSTRAINT ' + @var1 + ';');
    ALTER TABLE [EtkinlikKategorileri] DROP COLUMN [Id];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Puanlar] ADD [Reason] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Puanlar] ADD [RevokedAtUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Puanlar] ADD [SourceId] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    DECLARE @var2 nvarchar(max);
    SELECT @var2 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Kullanicilar]') AND [c].[name] = N'UserName');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Kullanicilar] DROP CONSTRAINT ' + @var2 + ';');
    ALTER TABLE [Kullanicilar] ALTER COLUMN [UserName] nvarchar(256) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    DECLARE @var3 nvarchar(max);
    SELECT @var3 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Kullanicilar]') AND [c].[name] = N'ProfilePicture');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Kullanicilar] DROP CONSTRAINT ' + @var3 + ';');
    ALTER TABLE [Kullanicilar] ALTER COLUMN [ProfilePicture] nvarchar(300) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    DECLARE @var4 nvarchar(max);
    SELECT @var4 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Kullanicilar]') AND [c].[name] = N'PhoneNumber');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Kullanicilar] DROP CONSTRAINT ' + @var4 + ';');
    ALTER TABLE [Kullanicilar] ALTER COLUMN [PhoneNumber] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    DECLARE @var5 nvarchar(max);
    SELECT @var5 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Kullanicilar]') AND [c].[name] = N'Location');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Kullanicilar] DROP CONSTRAINT ' + @var5 + ';');
    ALTER TABLE [Kullanicilar] ALTER COLUMN [Location] nvarchar(250) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    DECLARE @var6 nvarchar(max);
    SELECT @var6 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Kullanicilar]') AND [c].[name] = N'Email');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Kullanicilar] DROP CONSTRAINT ' + @var6 + ';');
    ALTER TABLE [Kullanicilar] ALTER COLUMN [Email] nvarchar(256) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Kullanicilar] ADD [AccessFailedCount] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Kullanicilar] ADD [AnonymizeAtUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Kullanicilar] ADD [ConcurrencyStamp] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Kullanicilar] ADD [CreatedAtUtc] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Kullanicilar] ADD [DeletedAtUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Kullanicilar] ADD [EmailConfirmed] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Kullanicilar] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Kullanicilar] ADD [Latitude] float NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Kullanicilar] ADD [LockoutEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Kullanicilar] ADD [LockoutEnd] datetimeoffset NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Kullanicilar] ADD [Longitude] float NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Kullanicilar] ADD [NormalizedEmail] nvarchar(256) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Kullanicilar] ADD [NormalizedUserName] nvarchar(256) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    DECLARE @var7 nvarchar(max);
    SELECT @var7 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Kullanicilar]') AND [c].[name] = N'PasswordHash');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Kullanicilar] DROP CONSTRAINT ' + @var7 + ';');
    ALTER TABLE [Kullanicilar] ALTER COLUMN [PasswordHash] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Kullanicilar] ADD [PhoneNumberConfirmed] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Kullanicilar] ADD [RowVersion] rowversion NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Kullanicilar] ADD [SecurityStamp] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Kullanicilar] ADD [TwoFactorEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Kullanicilar] ADD [UpdatedAtUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Katilimlar] ADD [AttendanceConfirmedAtUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Katilimlar] ADD [AttendanceConfirmedByUserId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Katilimlar] ADD [Status] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    DECLARE @var8 nvarchar(max);
    SELECT @var8 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Etkinlikler]') AND [c].[name] = N'Location');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Etkinlikler] DROP CONSTRAINT ' + @var8 + ';');
    ALTER TABLE [Etkinlikler] ALTER COLUMN [Location] nvarchar(250) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    DECLARE @var9 nvarchar(max);
    SELECT @var9 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Etkinlikler]') AND [c].[name] = N'ImageUrl');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [Etkinlikler] DROP CONSTRAINT ' + @var9 + ';');
    ALTER TABLE [Etkinlikler] ALTER COLUMN [ImageUrl] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    DECLARE @var10 nvarchar(max);
    SELECT @var10 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Etkinlikler]') AND [c].[name] = N'Description');
    IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [Etkinlikler] DROP CONSTRAINT ' + @var10 + ';');
    ALTER TABLE [Etkinlikler] ALTER COLUMN [Description] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Etkinlikler] ADD [CreatedAtUtc] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Etkinlikler] ADD [DeletedAtUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Etkinlikler] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Etkinlikler] ADD [Latitude] float NOT NULL DEFAULT 0.0E0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Etkinlikler] ADD [Longitude] float NOT NULL DEFAULT 0.0E0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Etkinlikler] ADD [RejectionReason] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Etkinlikler] ADD [RowVersion] rowversion NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Etkinlikler] ADD [UpdatedAtUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_Kullanicilar_UserId] FOREIGN KEY ([UserId]) REFERENCES [Kullanicilar] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] int NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_Kullanicilar_UserId] FOREIGN KEY ([UserId]) REFERENCES [Kullanicilar] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] int NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_Kullanicilar_UserId] FOREIGN KEY ([UserId]) REFERENCES [Kullanicilar] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    CREATE TABLE [AuditLogs] (
        [Id] bigint NOT NULL IDENTITY,
        [ActorUserId] int NULL,
        [Action] nvarchar(80) NOT NULL,
        [EntityType] nvarchar(80) NOT NULL,
        [EntityId] nvarchar(80) NOT NULL,
        [OccurredAtUtc] datetime2 NOT NULL,
        [CorrelationId] nvarchar(100) NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] int NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] int NOT NULL,
        [RoleId] int NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_Kullanicilar_UserId] FOREIGN KEY ([UserId]) REFERENCES [Kullanicilar] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    UPDATE Kullanicilar
    SET EmailConfirmed = 1,
        NormalizedEmail = UPPER(Email),
        NormalizedUserName = UPPER(UserName),
        SecurityStamp = CONVERT(nvarchar(36), NEWID()),
        ConcurrencyStamp = CONVERT(nvarchar(36), NEWID()),
        CreatedAtUtc = SYSUTCDATETIME(),
        LockoutEnabled = 1;

    UPDATE Etkinlikler SET CreatedAtUtc = SYSUTCDATETIME();
    UPDATE Puanlar SET SourceId = Id;

    IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE NormalizedName = 'ADMIN')
        INSERT INTO AspNetRoles (Name, NormalizedName, ConcurrencyStamp)
        VALUES ('Admin', 'ADMIN', CONVERT(nvarchar(36), NEWID()));
    IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE NormalizedName = 'USER')
        INSERT INTO AspNetRoles (Name, NormalizedName, ConcurrencyStamp)
        VALUES ('User', 'USER', CONVERT(nvarchar(36), NEWID()));

    INSERT INTO AspNetUserRoles (UserId, RoleId)
    SELECT u.Id, r.Id
    FROM Kullanicilar u
    JOIN AspNetRoles r ON r.NormalizedName = CASE WHEN u.Role = 1 THEN 'ADMIN' ELSE 'USER' END
    WHERE NOT EXISTS (
        SELECT 1 FROM AspNetUserRoles ur WHERE ur.UserId = u.Id AND ur.RoleId = r.Id
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Puanlar_KullaniciId_Reason_SourceId] ON [Puanlar] ([KullaniciId], [Reason], [SourceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [Kullanicilar] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [Kullanicilar] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Katilimlar_KullaniciId_EtkinlikId] ON [Katilimlar] ([KullaniciId], [EtkinlikId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Kategoriler_Name] ON [Kategoriler] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Katilimlar] ADD CONSTRAINT [FK_Katilimlar_Kullanicilar_KullaniciId] FOREIGN KEY ([KullaniciId]) REFERENCES [Kullanicilar] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Mesajlar] ADD CONSTRAINT [FK_Mesajlar_Kullanicilar_SenderId] FOREIGN KEY ([SenderId]) REFERENCES [Kullanicilar] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    ALTER TABLE [Puanlar] ADD CONSTRAINT [FK_Puanlar_Kullanicilar_KullaniciId] FOREIGN KEY ([KullaniciId]) REFERENCES [Kullanicilar] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924110639_ProductionHardening'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260924110639_ProductionHardening', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924112311_CategoryArchiving'
)
BEGIN
    ALTER TABLE [Kategoriler] ADD [IsArchived] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924112311_CategoryArchiving'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260924112311_CategoryArchiving', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924114210_OperationalHardening'
)
BEGIN
    CREATE TABLE [EmailOutbox] (
        [Id] bigint NOT NULL IDENTITY,
        [Recipient] nvarchar(256) NOT NULL,
        [Subject] nvarchar(200) NOT NULL,
        [Body] nvarchar(4000) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [NextAttemptAtUtc] datetime2 NOT NULL,
        [SentAtUtc] datetime2 NULL,
        [AttemptCount] int NOT NULL,
        [LastError] nvarchar(1000) NULL,
        CONSTRAINT [PK_EmailOutbox] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924114210_OperationalHardening'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Puanlar_KullaniciId_Reason] ON [Puanlar] ([KullaniciId], [Reason]) WHERE [Reason] = 2 AND [RevokedAtUtc] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924114210_OperationalHardening'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260924114210_OperationalHardening', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924115421_QueryFilterConsistency'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260924115421_QueryFilterConsistency', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924124500_ExtendGenderLength'
)
BEGIN
    ALTER TABLE [Kullanicilar] ALTER COLUMN [Gender] nvarchar(20) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924124500_ExtendGenderLength'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260924124500_ExtendGenderLength', N'10.0.12');
END;

COMMIT;
GO

