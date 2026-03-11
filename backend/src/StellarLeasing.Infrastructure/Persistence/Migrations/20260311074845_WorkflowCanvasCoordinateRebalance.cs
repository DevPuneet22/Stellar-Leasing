using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StellarLeasing.Infrastructure.Persistence.Migrations;

[DbContext(typeof(StellarLeasingDbContext))]
[Migration("20260311074845_WorkflowCanvasCoordinateRebalance")]
public partial class WorkflowCanvasCoordinateRebalance : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            WITH versions_to_rebalance AS (
                SELECT "WorkflowVersionId"
                FROM workflow_steps
                GROUP BY "WorkflowVersionId"
                HAVING COUNT(*) > 1
                   AND COUNT(DISTINCT (CAST("PositionX" AS text) || ':' || CAST("PositionY" AS text))) = 1
            ),
            ranked_steps AS (
                SELECT ws."Id",
                       ROW_NUMBER() OVER (
                           PARTITION BY ws."WorkflowVersionId"
                           ORDER BY ws."SortOrder", ws."Key", ws."Id"
                       ) - 1 AS row_index
                FROM workflow_steps AS ws
                INNER JOIN versions_to_rebalance AS vr
                    ON vr."WorkflowVersionId" = ws."WorkflowVersionId"
            )
            UPDATE workflow_steps AS ws
            SET "PositionX" = 80 + (ranked_steps.row_index * 280),
                "PositionY" = CASE
                    WHEN MOD(ranked_steps.row_index, 2) = 0 THEN 160
                    ELSE 280
                END
            FROM ranked_steps
            WHERE ws."Id" = ranked_steps."Id";
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
