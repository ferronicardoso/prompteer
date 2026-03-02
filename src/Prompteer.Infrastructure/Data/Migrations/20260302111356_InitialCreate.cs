using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prompteer.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "agent_profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    KnowledgeDomain = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Tone = table.Column<int>(type: "integer", nullable: false),
                    DefaultConstraints = table.Column<string>(type: "text", nullable: false),
                    IsSystemDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agent_profiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "architectural_patterns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Ecosystem = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_architectural_patterns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "backlog_tools",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DefaultInstructions = table.Column<string>(type: "text", nullable: false),
                    IsSystemDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_backlog_tools", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "prompt_drafts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    WizardDataJson = table.Column<string>(type: "text", nullable: false),
                    CurrentStep = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prompt_drafts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "prompt_templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CurrentVersionNumber = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prompt_templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "technologies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Ecosystem = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ShortDescription = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_technologies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "prompt_template_versions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PromptTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    GeneratedPrompt = table.Column<string>(type: "text", nullable: false),
                    WizardDataJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prompt_template_versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_prompt_template_versions_prompt_templates_PromptTemplateId",
                        column: x => x.PromptTemplateId,
                        principalTable: "prompt_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prompt_modules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PromptTemplateVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prompt_modules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_prompt_modules_prompt_template_versions_PromptTemplateVersi~",
                        column: x => x.PromptTemplateVersionId,
                        principalTable: "prompt_template_versions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prompt_version_patterns",
                columns: table => new
                {
                    PromptTemplateVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ArchitecturalPatternId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prompt_version_patterns", x => new { x.PromptTemplateVersionId, x.ArchitecturalPatternId });
                    table.ForeignKey(
                        name: "FK_prompt_version_patterns_architectural_patterns_Architectura~",
                        column: x => x.ArchitecturalPatternId,
                        principalTable: "architectural_patterns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_prompt_version_patterns_prompt_template_versions_PromptTemp~",
                        column: x => x.PromptTemplateVersionId,
                        principalTable: "prompt_template_versions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prompt_version_technologies",
                columns: table => new
                {
                    PromptTemplateVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TechnologyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prompt_version_technologies", x => new { x.PromptTemplateVersionId, x.TechnologyId });
                    table.ForeignKey(
                        name: "FK_prompt_version_technologies_prompt_template_versions_Prompt~",
                        column: x => x.PromptTemplateVersionId,
                        principalTable: "prompt_template_versions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_prompt_version_technologies_technologies_TechnologyId",
                        column: x => x.TechnologyId,
                        principalTable: "technologies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "prompt_module_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PromptModuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prompt_module_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_prompt_module_items_prompt_modules_PromptModuleId",
                        column: x => x.PromptModuleId,
                        principalTable: "prompt_modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_agent_profiles_IsDeleted",
                table: "agent_profiles",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_agent_profiles_Name",
                table: "agent_profiles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_architectural_patterns_IsDeleted",
                table: "architectural_patterns",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_backlog_tools_IsDeleted",
                table: "backlog_tools",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_backlog_tools_Name",
                table: "backlog_tools",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_prompt_module_items_PromptModuleId",
                table: "prompt_module_items",
                column: "PromptModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_prompt_modules_PromptTemplateVersionId_DisplayOrder",
                table: "prompt_modules",
                columns: new[] { "PromptTemplateVersionId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_prompt_template_versions_PromptTemplateId_VersionNumber",
                table: "prompt_template_versions",
                columns: new[] { "PromptTemplateId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_prompt_templates_IsDeleted_UpdatedAt",
                table: "prompt_templates",
                columns: new[] { "IsDeleted", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_prompt_version_patterns_ArchitecturalPatternId",
                table: "prompt_version_patterns",
                column: "ArchitecturalPatternId");

            migrationBuilder.CreateIndex(
                name: "IX_prompt_version_technologies_TechnologyId",
                table: "prompt_version_technologies",
                column: "TechnologyId");

            migrationBuilder.CreateIndex(
                name: "IX_technologies_Category_Ecosystem",
                table: "technologies",
                columns: new[] { "Category", "Ecosystem" });

            migrationBuilder.CreateIndex(
                name: "IX_technologies_IsDeleted",
                table: "technologies",
                column: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agent_profiles");

            migrationBuilder.DropTable(
                name: "backlog_tools");

            migrationBuilder.DropTable(
                name: "prompt_drafts");

            migrationBuilder.DropTable(
                name: "prompt_module_items");

            migrationBuilder.DropTable(
                name: "prompt_version_patterns");

            migrationBuilder.DropTable(
                name: "prompt_version_technologies");

            migrationBuilder.DropTable(
                name: "prompt_modules");

            migrationBuilder.DropTable(
                name: "architectural_patterns");

            migrationBuilder.DropTable(
                name: "technologies");

            migrationBuilder.DropTable(
                name: "prompt_template_versions");

            migrationBuilder.DropTable(
                name: "prompt_templates");
        }
    }
}
