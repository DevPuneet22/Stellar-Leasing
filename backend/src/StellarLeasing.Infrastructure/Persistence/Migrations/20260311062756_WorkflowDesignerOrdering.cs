using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StellarLeasing.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WorkflowDesignerOrdering : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_workflow_steps_WorkflowVersionId",
                table: "workflow_steps");

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "workflow_transitions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "workflow_steps",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_workflow_steps_WorkflowVersionId_Key",
                table: "workflow_steps",
                columns: new[] { "WorkflowVersionId", "Key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_workflow_steps_WorkflowVersionId_Key",
                table: "workflow_steps");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "workflow_transitions");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "workflow_steps");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_steps_WorkflowVersionId",
                table: "workflow_steps",
                column: "WorkflowVersionId");
        }
    }
}
