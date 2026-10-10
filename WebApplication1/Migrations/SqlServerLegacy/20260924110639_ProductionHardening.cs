using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class ProductionHardening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Katilimlar_Kullanicilar_KullaniciId",
                table: "Katilimlar");

            migrationBuilder.DropForeignKey(
                name: "FK_Mesajlar_Kullanicilar_SenderId",
                table: "Mesajlar");

            migrationBuilder.DropForeignKey(
                name: "FK_Puanlar_Kullanicilar_KullaniciId",
                table: "Puanlar");

            migrationBuilder.DropIndex(
                name: "IX_Puanlar_KullaniciId",
                table: "Puanlar");

            migrationBuilder.DropIndex(
                name: "IX_Katilimlar_KullaniciId",
                table: "Katilimlar");

            migrationBuilder.RenameColumn(
                name: "Password",
                table: "Kullanicilar",
                newName: "PasswordHash");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "KullaniciKategorileri");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "EtkinlikKategorileri");

            migrationBuilder.AddColumn<int>(
                name: "Reason",
                table: "Puanlar",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "RevokedAtUtc",
                table: "Puanlar",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceId",
                table: "Puanlar",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "Kullanicilar",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "ProfilePicture",
                table: "Kullanicilar",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Kullanicilar",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15);

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "Kullanicilar",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Kullanicilar",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<int>(
                name: "AccessFailedCount",
                table: "Kullanicilar",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "AnonymizeAtUtc",
                table: "Kullanicilar",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "Kullanicilar",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Kullanicilar",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "Kullanicilar",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailConfirmed",
                table: "Kullanicilar",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Kullanicilar",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Kullanicilar",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LockoutEnabled",
                table: "Kullanicilar",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LockoutEnd",
                table: "Kullanicilar",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Kullanicilar",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedEmail",
                table: "Kullanicilar",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedUserName",
                table: "Kullanicilar",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Kullanicilar",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AddColumn<bool>(
                name: "PhoneNumberConfirmed",
                table: "Kullanicilar",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Kullanicilar",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecurityStamp",
                table: "Kullanicilar",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TwoFactorEnabled",
                table: "Kullanicilar",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Kullanicilar",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AttendanceConfirmedAtUtc",
                table: "Katilimlar",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AttendanceConfirmedByUserId",
                table: "Katilimlar",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Katilimlar",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "Etkinlikler",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Etkinlikler",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Etkinlikler",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Etkinlikler",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "Etkinlikler",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Etkinlikler",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Etkinlikler",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Etkinlikler",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Etkinlikler",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Etkinlikler",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Etkinlikler",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_Kullanicilar_UserId",
                        column: x => x.UserId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_Kullanicilar_UserId",
                        column: x => x.UserId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_Kullanicilar_UserId",
                        column: x => x.UserId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActorUserId = table.Column<int>(type: "int", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_Kullanicilar_UserId",
                        column: x => x.UserId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
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
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Puanlar_KullaniciId_Reason_SourceId",
                table: "Puanlar",
                columns: new[] { "KullaniciId", "Reason", "SourceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Kullanicilar",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Kullanicilar",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Katilimlar_KullaniciId_EtkinlikId",
                table: "Katilimlar",
                columns: new[] { "KullaniciId", "EtkinlikId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kategoriler_Name",
                table: "Kategoriler",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Katilimlar_Kullanicilar_KullaniciId",
                table: "Katilimlar",
                column: "KullaniciId",
                principalTable: "Kullanicilar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Mesajlar_Kullanicilar_SenderId",
                table: "Mesajlar",
                column: "SenderId",
                principalTable: "Kullanicilar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Puanlar_Kullanicilar_KullaniciId",
                table: "Puanlar",
                column: "KullaniciId",
                principalTable: "Kullanicilar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Katilimlar_Kullanicilar_KullaniciId",
                table: "Katilimlar");

            migrationBuilder.DropForeignKey(
                name: "FK_Mesajlar_Kullanicilar_SenderId",
                table: "Mesajlar");

            migrationBuilder.DropForeignKey(
                name: "FK_Puanlar_Kullanicilar_KullaniciId",
                table: "Puanlar");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropIndex(
                name: "IX_Puanlar_KullaniciId_Reason_SourceId",
                table: "Puanlar");

            migrationBuilder.DropIndex(
                name: "EmailIndex",
                table: "Kullanicilar");

            migrationBuilder.DropIndex(
                name: "UserNameIndex",
                table: "Kullanicilar");

            migrationBuilder.DropIndex(
                name: "IX_Katilimlar_KullaniciId_EtkinlikId",
                table: "Katilimlar");

            migrationBuilder.DropIndex(
                name: "IX_Kategoriler_Name",
                table: "Kategoriler");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "Puanlar");

            migrationBuilder.DropColumn(
                name: "RevokedAtUtc",
                table: "Puanlar");

            migrationBuilder.DropColumn(
                name: "SourceId",
                table: "Puanlar");

            migrationBuilder.DropColumn(
                name: "AccessFailedCount",
                table: "Kullanicilar");

            migrationBuilder.DropColumn(
                name: "AnonymizeAtUtc",
                table: "Kullanicilar");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "Kullanicilar");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Kullanicilar");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "Kullanicilar");

            migrationBuilder.DropColumn(
                name: "EmailConfirmed",
                table: "Kullanicilar");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Kullanicilar");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Kullanicilar");

            migrationBuilder.DropColumn(
                name: "LockoutEnabled",
                table: "Kullanicilar");

            migrationBuilder.DropColumn(
                name: "LockoutEnd",
                table: "Kullanicilar");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Kullanicilar");

            migrationBuilder.DropColumn(
                name: "NormalizedEmail",
                table: "Kullanicilar");

            migrationBuilder.DropColumn(
                name: "NormalizedUserName",
                table: "Kullanicilar");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Kullanicilar",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "PhoneNumberConfirmed",
                table: "Kullanicilar");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Kullanicilar");

            migrationBuilder.DropColumn(
                name: "SecurityStamp",
                table: "Kullanicilar");

            migrationBuilder.DropColumn(
                name: "TwoFactorEnabled",
                table: "Kullanicilar");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Kullanicilar");

            migrationBuilder.DropColumn(
                name: "AttendanceConfirmedAtUtc",
                table: "Katilimlar");

            migrationBuilder.DropColumn(
                name: "AttendanceConfirmedByUserId",
                table: "Katilimlar");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Katilimlar");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Etkinlikler");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "Etkinlikler");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Etkinlikler");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Etkinlikler");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Etkinlikler");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Etkinlikler");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Etkinlikler");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Etkinlikler");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "Kullanicilar",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProfilePicture",
                table: "Kullanicilar",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Kullanicilar",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "Kullanicilar",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Kullanicilar",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "Kullanicilar",
                newName: "Password");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "KullaniciKategorileri",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "Etkinlikler",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Etkinlikler",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Etkinlikler",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "EtkinlikKategorileri",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Puanlar_KullaniciId",
                table: "Puanlar",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_Katilimlar_KullaniciId",
                table: "Katilimlar",
                column: "KullaniciId");

            migrationBuilder.AddForeignKey(
                name: "FK_Katilimlar_Kullanicilar_KullaniciId",
                table: "Katilimlar",
                column: "KullaniciId",
                principalTable: "Kullanicilar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Mesajlar_Kullanicilar_SenderId",
                table: "Mesajlar",
                column: "SenderId",
                principalTable: "Kullanicilar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Puanlar_Kullanicilar_KullaniciId",
                table: "Puanlar",
                column: "KullaniciId",
                principalTable: "Kullanicilar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
