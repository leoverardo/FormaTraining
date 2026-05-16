using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class V14_HabitsTimezoneAndArchivalConsolidation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentHabitLogs_StudentHabits_HabitId",
                table: "StudentHabitLogs");

            migrationBuilder.DropIndex(
                name: "IX_StudentHabits_StudentId_IsActive",
                table: "StudentHabits");

            migrationBuilder.AddColumn<DateTime>(
                name: "InactivatedAt",
                table: "StudentHabits",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentHabits_StudentId_IsActive_InactivatedAt",
                table: "StudentHabits",
                columns: new[] { "StudentId", "IsActive", "InactivatedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_StudentHabitLogs_StudentHabits_HabitId",
                table: "StudentHabitLogs",
                column: "HabitId",
                principalTable: "StudentHabits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentHabitLogs_StudentHabits_HabitId",
                table: "StudentHabitLogs");

            migrationBuilder.DropIndex(
                name: "IX_StudentHabits_StudentId_IsActive_InactivatedAt",
                table: "StudentHabits");

            migrationBuilder.DropColumn(
                name: "InactivatedAt",
                table: "StudentHabits");

            migrationBuilder.CreateIndex(
                name: "IX_StudentHabits_StudentId_IsActive",
                table: "StudentHabits",
                columns: new[] { "StudentId", "IsActive" });

            migrationBuilder.AddForeignKey(
                name: "FK_StudentHabitLogs_StudentHabits_HabitId",
                table: "StudentHabitLogs",
                column: "HabitId",
                principalTable: "StudentHabits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
