using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WebApplication1.Models;

#nullable disable

namespace WebApplication1.Migrations
{
    [Migration("20261001165000_AddPinnedMessages")]
    [DbContext(typeof(Context))]
    public partial class AddPinnedMessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPinned",
                table: "Mesajlar",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PinnedAtUtc",
                table: "Mesajlar",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PinnedByUserId",
                table: "Mesajlar",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPinned",
                table: "Mesajlar");

            migrationBuilder.DropColumn(
                name: "PinnedAtUtc",
                table: "Mesajlar");

            migrationBuilder.DropColumn(
                name: "PinnedByUserId",
                table: "Mesajlar");
        }
    }
}
