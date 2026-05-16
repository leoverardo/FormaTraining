using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class V12_WorkoutExecutionReviewHardening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PrescribedNotes",
                table: "WorkoutSessionExercises",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessions_StudentId_Status_CompletedAt",
                table: "WorkoutSessions",
                columns: new[] { "StudentId", "Status", "CompletedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkoutSessions_StudentId_Status_CompletedAt",
                table: "WorkoutSessions");

            migrationBuilder.DropColumn(
                name: "PrescribedNotes",
                table: "WorkoutSessionExercises");
        }
    }
}
