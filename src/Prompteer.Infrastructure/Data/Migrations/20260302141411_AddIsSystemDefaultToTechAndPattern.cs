using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prompteer.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsSystemDefaultToTechAndPattern : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSystemDefault",
                table: "technologies",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSystemDefault",
                table: "architectural_patterns",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSystemDefault",
                table: "technologies");

            migrationBuilder.DropColumn(
                name: "IsSystemDefault",
                table: "architectural_patterns");
        }
    }
}
