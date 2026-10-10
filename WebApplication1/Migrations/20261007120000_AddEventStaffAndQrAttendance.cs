using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using WebApplication1.Models;

#nullable disable

namespace WebApplication1.Migrations
{
    [Migration("20261007120000_AddEventStaffAndQrAttendance")]
    [DbContext(typeof(Context))]
    public partial class AddEventStaffAndQrAttendance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventStaff",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EtkinlikId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventStaff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventStaff_Etkinlikler_EtkinlikId",
                        column: x => x.EtkinlikId,
                        principalTable: "Etkinlikler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventStaff_Kullanicilar_UserId",
                        column: x => x.UserId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AttendanceQrTokens",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EtkinlikId = table.Column<int>(type: "int", nullable: false),
                    ParticipantId = table.Column<int>(type: "int", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceQrTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceQrTokens_Etkinlikler_EtkinlikId",
                        column: x => x.EtkinlikId,
                        principalTable: "Etkinlikler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AttendanceQrTokens_Katilimlar_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "Katilimlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventStaff_EtkinlikId_UserId_Role",
                table: "EventStaff",
                columns: new[] { "EtkinlikId", "UserId", "Role" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventStaff_UserId",
                table: "EventStaff",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceQrTokens_EtkinlikId",
                table: "AttendanceQrTokens",
                column: "EtkinlikId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceQrTokens_ParticipantId",
                table: "AttendanceQrTokens",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceQrTokens_TokenHash",
                table: "AttendanceQrTokens",
                column: "TokenHash",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AttendanceQrTokens");
            migrationBuilder.DropTable(name: "EventStaff");
        }
    }
}
