using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitPlatform.Infrastructure.Migrations
{
    public partial class V9_ExploreTrainerVisibilityGeoColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('Trainers', 'Latitude') IS NULL
    ALTER TABLE [Trainers] ADD [Latitude] float NULL;
IF COL_LENGTH('Trainers', 'Longitude') IS NULL
    ALTER TABLE [Trainers] ADD [Longitude] float NULL;
IF COL_LENGTH('Trainers', 'ServiceMode') IS NULL
    ALTER TABLE [Trainers] ADD [ServiceMode] nvarchar(50) NULL;
IF COL_LENGTH('Trainers', 'PublicSearchEnabled') IS NULL
    ALTER TABLE [Trainers] ADD [PublicSearchEnabled] bit NOT NULL CONSTRAINT [DF_Trainers_PublicSearchEnabled] DEFAULT(0);
IF COL_LENGTH('Trainers', 'AcceptingStudents') IS NULL
    ALTER TABLE [Trainers] ADD [AcceptingStudents] bit NOT NULL CONSTRAINT [DF_Trainers_AcceptingStudents] DEFAULT(1);
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_Trainers_PublicPageEnabled_PublicSearchEnabled_AcceptingStudents'
      AND object_id = OBJECT_ID('Trainers')
)
    CREATE INDEX [IX_Trainers_PublicPageEnabled_PublicSearchEnabled_AcceptingStudents]
    ON [Trainers] ([PublicPageEnabled], [PublicSearchEnabled], [AcceptingStudents]);
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_Trainers_PublicPageEnabled_PublicSearchEnabled_AcceptingStudents'
      AND object_id = OBJECT_ID('Trainers')
)
    DROP INDEX [IX_Trainers_PublicPageEnabled_PublicSearchEnabled_AcceptingStudents] ON [Trainers];
");
        }
    }
}
