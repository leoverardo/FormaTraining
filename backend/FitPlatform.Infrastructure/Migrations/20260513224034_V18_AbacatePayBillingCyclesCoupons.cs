using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class V18_AbacatePayBillingCyclesCoupons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_TrainerServiceOrders_TrainerLeads_LeadId'
)
BEGIN
    ALTER TABLE [TrainerServiceOrders] DROP CONSTRAINT [FK_TrainerServiceOrders_TrainerLeads_LeadId];
END
");

            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_TrainerServiceOrders_LeadId'
      AND object_id = OBJECT_ID(N'[TrainerServiceOrders]')
)
BEGIN
    DROP INDEX [IX_TrainerServiceOrders_LeadId] ON [TrainerServiceOrders];
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_TrainerServiceOrders_LeadId'
      AND object_id = OBJECT_ID(N'[TrainerServiceOrders]')
)
BEGIN
    CREATE INDEX [IX_TrainerServiceOrders_LeadId] ON [TrainerServiceOrders]([LeadId]);
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_TrainerServiceOrders_TrainerLeads_LeadId'
)
BEGIN
    ALTER TABLE [TrainerServiceOrders]
    ADD CONSTRAINT [FK_TrainerServiceOrders_TrainerLeads_LeadId]
    FOREIGN KEY ([LeadId]) REFERENCES [TrainerLeads]([Id]) ON DELETE SET NULL;
END
");
        }
    }
}
