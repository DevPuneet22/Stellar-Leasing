using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StellarLeasing.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WorkflowCanvasCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "PositionX",
                table: "workflow_steps",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PositionY",
                table: "workflow_steps",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PositionX",
                table: "workflow_steps");

            migrationBuilder.DropColumn(
                name: "PositionY",
                table: "workflow_steps");
        }
    }
}
