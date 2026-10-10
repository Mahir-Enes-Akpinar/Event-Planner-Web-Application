using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using WebApplication1.Models;

#nullable disable

namespace WebApplication1.Migrations
{
    [Migration("20261001193000_AddEventDetailedProgram")]
    [DbContext(typeof(Context))]
    public partial class AddEventDetailedProgram : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DetailedProgram",
                table: "Etkinlikler",
                type: "nvarchar(3000)",
                maxLength: 3000,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DetailedProgram",
                table: "Etkinlikler");
        }
    }
}
