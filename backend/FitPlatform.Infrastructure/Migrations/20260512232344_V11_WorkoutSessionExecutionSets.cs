using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class V11_WorkoutSessionExecutionSets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkoutSessionExercises_WorkoutSessionId",
                table: "WorkoutSessionExercises");

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "WorkoutSessionExercises",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "WorkoutSessionExercises",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OrderIndex",
                table: "WorkoutSessionExercises",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PrescribedLoad",
                table: "WorkoutSessionExercises",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrescribedReps",
                table: "WorkoutSessionExercises",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PrescribedRestSeconds",
                table: "WorkoutSessionExercises",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PrescribedSets",
                table: "WorkoutSessionExercises",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkoutExerciseId",
                table: "WorkoutSessionExercises",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkoutSessionSets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkoutSessionExerciseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SetNumber = table.Column<int>(type: "int", nullable: false),
                    PrescribedReps = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrescribedLoad = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrescribedRestSeconds = table.Column<int>(type: "int", nullable: true),
                    ActualReps = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActualLoad = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutSessionSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutSessionSets_WorkoutSessionExercises_WorkoutSessionExerciseId",
                        column: x => x.WorkoutSessionExerciseId,
                        principalTable: "WorkoutSessionExercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessionExercises_WorkoutExerciseId",
                table: "WorkoutSessionExercises",
                column: "WorkoutExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessionExercises_WorkoutSessionId_ExerciseId",
                table: "WorkoutSessionExercises",
                columns: new[] { "WorkoutSessionId", "ExerciseId" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessionExercises_WorkoutSessionId_OrderIndex",
                table: "WorkoutSessionExercises",
                columns: new[] { "WorkoutSessionId", "OrderIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessionSets_WorkoutSessionExerciseId_IsCompleted",
                table: "WorkoutSessionSets",
                columns: new[] { "WorkoutSessionExerciseId", "IsCompleted" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessionSets_WorkoutSessionExerciseId_SetNumber",
                table: "WorkoutSessionSets",
                columns: new[] { "WorkoutSessionExerciseId", "SetNumber" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutSessionExercises_WorkoutExercises_WorkoutExerciseId",
                table: "WorkoutSessionExercises",
                column: "WorkoutExerciseId",
                principalTable: "WorkoutExercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutSessionExercises_WorkoutExercises_WorkoutExerciseId",
                table: "WorkoutSessionExercises");

            migrationBuilder.DropTable(
                name: "WorkoutSessionSets");

            migrationBuilder.DropIndex(
                name: "IX_WorkoutSessionExercises_WorkoutExerciseId",
                table: "WorkoutSessionExercises");

            migrationBuilder.DropIndex(
                name: "IX_WorkoutSessionExercises_WorkoutSessionId_ExerciseId",
                table: "WorkoutSessionExercises");

            migrationBuilder.DropIndex(
                name: "IX_WorkoutSessionExercises_WorkoutSessionId_OrderIndex",
                table: "WorkoutSessionExercises");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "WorkoutSessionExercises");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "WorkoutSessionExercises");

            migrationBuilder.DropColumn(
                name: "OrderIndex",
                table: "WorkoutSessionExercises");

            migrationBuilder.DropColumn(
                name: "PrescribedLoad",
                table: "WorkoutSessionExercises");

            migrationBuilder.DropColumn(
                name: "PrescribedReps",
                table: "WorkoutSessionExercises");

            migrationBuilder.DropColumn(
                name: "PrescribedRestSeconds",
                table: "WorkoutSessionExercises");

            migrationBuilder.DropColumn(
                name: "PrescribedSets",
                table: "WorkoutSessionExercises");

            migrationBuilder.DropColumn(
                name: "WorkoutExerciseId",
                table: "WorkoutSessionExercises");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessionExercises_WorkoutSessionId",
                table: "WorkoutSessionExercises",
                column: "WorkoutSessionId");
        }
    }
}
