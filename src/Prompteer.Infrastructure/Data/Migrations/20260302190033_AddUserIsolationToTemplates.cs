using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prompteer.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIsolationToTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "prompt_templates",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "prompt_drafts",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "prompt_templates");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "prompt_drafts");
        }
    }
}
