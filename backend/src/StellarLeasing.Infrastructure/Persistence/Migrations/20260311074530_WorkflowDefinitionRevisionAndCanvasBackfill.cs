using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StellarLeasing.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WorkflowDefinitionRevisionAndCanvasBackfill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Revision",
                table: "WorkflowDefinitions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                """
                UPDATE "WorkflowDefinitions"
                SET "Revision" = 1
                WHERE "Revision" = 0;
                """);

            migrationBuilder.Sql(
                """
                UPDATE workflow_steps
                SET "PositionX" = 80 + ("SortOrder" * 280),
                    "PositionY" = 160
                WHERE "PositionX" = 0
                  AND "PositionY" = 0;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Revision",
                table: "WorkflowDefinitions");
        }
    }
}
