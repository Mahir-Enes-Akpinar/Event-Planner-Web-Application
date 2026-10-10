using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using WebApplication1.Models;

#nullable disable

namespace WebApplication1.Migrations
{
    [Migration("20261001150000_AddEventCapacity")]
    [DbContext(typeof(Context))]
    public partial class AddEventCapacity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxParticipants",
                table: "Etkinlikler",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxParticipants",
                table: "Etkinlikler");
        }
    }
}
