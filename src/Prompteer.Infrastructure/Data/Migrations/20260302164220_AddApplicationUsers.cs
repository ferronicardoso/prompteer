using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prompteer.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "technologies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "technologies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "prompt_templates",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "prompt_templates",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "prompt_modules",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "prompt_modules",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "prompt_module_items",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "prompt_module_items",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "backlog_tools",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "backlog_tools",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "architectural_patterns",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "architectural_patterns",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "agent_profiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "agent_profiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ApplicationUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntraObjectId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUsers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_EntraObjectId",
                table: "ApplicationUsers",
                column: "EntraObjectId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationUsers");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "technologies");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "technologies");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "prompt_templates");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "prompt_templates");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "prompt_modules");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "prompt_modules");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "prompt_module_items");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "prompt_module_items");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "backlog_tools");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "backlog_tools");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "architectural_patterns");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "architectural_patterns");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "agent_profiles");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "agent_profiles");
        }
    }
}
