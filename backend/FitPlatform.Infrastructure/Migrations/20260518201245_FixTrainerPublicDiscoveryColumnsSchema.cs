using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixTrainerPublicDiscoveryColumnsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH('Trainers', 'AcceptingStudents') IS NULL
                    ALTER TABLE [Trainers] ADD [AcceptingStudents] bit NOT NULL CONSTRAINT [DF_Trainers_AcceptingStudents_Fix] DEFAULT(1);
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('Trainers', 'Latitude') IS NULL
                    ALTER TABLE [Trainers] ADD [Latitude] float NULL;
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('Trainers', 'Longitude') IS NULL
                    ALTER TABLE [Trainers] ADD [Longitude] float NULL;
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('Trainers', 'PublicSearchEnabled') IS NULL
                    ALTER TABLE [Trainers] ADD [PublicSearchEnabled] bit NOT NULL CONSTRAINT [DF_Trainers_PublicSearchEnabled_Fix] DEFAULT(0);
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('Trainers', 'ServiceMode') IS NULL
                    ALTER TABLE [Trainers] ADD [ServiceMode] nvarchar(max) NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH('Trainers', 'ServiceMode') IS NOT NULL
                    ALTER TABLE [Trainers] DROP COLUMN [ServiceMode];
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('Trainers', 'PublicSearchEnabled') IS NOT NULL
                BEGIN
                    IF OBJECT_ID('DF_Trainers_PublicSearchEnabled_Fix', 'D') IS NOT NULL
                        ALTER TABLE [Trainers] DROP CONSTRAINT [DF_Trainers_PublicSearchEnabled_Fix];
                    ALTER TABLE [Trainers] DROP COLUMN [PublicSearchEnabled];
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('Trainers', 'Longitude') IS NOT NULL
                    ALTER TABLE [Trainers] DROP COLUMN [Longitude];
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('Trainers', 'Latitude') IS NOT NULL
                    ALTER TABLE [Trainers] DROP COLUMN [Latitude];
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('Trainers', 'AcceptingStudents') IS NOT NULL
                BEGIN
                    IF OBJECT_ID('DF_Trainers_AcceptingStudents_Fix', 'D') IS NOT NULL
                        ALTER TABLE [Trainers] DROP CONSTRAINT [DF_Trainers_AcceptingStudents_Fix];
                    ALTER TABLE [Trainers] DROP COLUMN [AcceptingStudents];
                END
                """);
        }
    }
}
