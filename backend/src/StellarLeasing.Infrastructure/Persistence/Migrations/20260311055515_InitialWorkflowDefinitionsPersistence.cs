using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StellarLeasing.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialWorkflowDefinitionsPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkflowDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "workflow_versions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    WorkflowDefinitionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workflow_versions_WorkflowDefinitions_WorkflowDefinitionId",
                        column: x => x.WorkflowDefinitionId,
                        principalTable: "WorkflowDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workflow_steps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StepType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AssigneeRule = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    WorkflowVersionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workflow_steps_workflow_versions_WorkflowVersionId",
                        column: x => x.WorkflowVersionId,
                        principalTable: "workflow_versions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workflow_transitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStepKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ToStepKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ConditionName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    WorkflowVersionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_transitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workflow_transitions_workflow_versions_WorkflowVersionId",
                        column: x => x.WorkflowVersionId,
                        principalTable: "workflow_versions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_workflow_steps_WorkflowVersionId",
                table: "workflow_steps",
                column: "WorkflowVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_transitions_WorkflowVersionId",
                table: "workflow_transitions",
                column: "WorkflowVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_versions_WorkflowDefinitionId_VersionNumber",
                table: "workflow_versions",
                columns: new[] { "WorkflowDefinitionId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowDefinitions_TenantId_Code",
                table: "WorkflowDefinitions",
                columns: new[] { "TenantId", "Code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "workflow_steps");

            migrationBuilder.DropTable(
                name: "workflow_transitions");

            migrationBuilder.DropTable(
                name: "workflow_versions");

            migrationBuilder.DropTable(
                name: "WorkflowDefinitions");
        }
    }
}
