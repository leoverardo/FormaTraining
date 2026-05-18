using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixOrphanedV9MigrationsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH('TrainerSubscriptions', 'InitPoint') IS NULL
                    ALTER TABLE [TrainerSubscriptions] ADD [InitPoint] nvarchar(max) NULL;
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('TrainerSubscriptions', 'MercadoPagoPayerId') IS NULL
                    ALTER TABLE [TrainerSubscriptions] ADD [MercadoPagoPayerId] nvarchar(max) NULL;
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('TrainerPayments', 'ProviderSubscriptionId') IS NULL
                    ALTER TABLE [TrainerPayments] ADD [ProviderSubscriptionId] nvarchar(max) NULL;
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('TrainerPayments', 'RawPayload') IS NULL
                    ALTER TABLE [TrainerPayments] ADD [RawPayload] nvarchar(max) NULL;
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[PaymentWebhookLogs]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [PaymentWebhookLogs](
                        [Id] uniqueidentifier NOT NULL,
                        [Provider] nvarchar(450) NOT NULL,
                        [EventId] nvarchar(450) NOT NULL,
                        [Type] nvarchar(max) NOT NULL,
                        [ResourceId] nvarchar(450) NULL,
                        [RawPayload] nvarchar(max) NOT NULL,
                        [ProcessedAt] datetime2 NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        [UpdatedAt] datetime2 NOT NULL,
                        CONSTRAINT [PK_PaymentWebhookLogs] PRIMARY KEY ([Id])
                    );
                END
                """);

            migrationBuilder.Sql(
                """
                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_PaymentWebhookLogs_Provider_EventId'
                      AND object_id = OBJECT_ID('PaymentWebhookLogs')
                )
                    CREATE UNIQUE INDEX [IX_PaymentWebhookLogs_Provider_EventId]
                    ON [PaymentWebhookLogs] ([Provider], [EventId]);
                """);

            migrationBuilder.Sql(
                """
                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_PaymentWebhookLogs_ResourceId'
                      AND object_id = OBJECT_ID('PaymentWebhookLogs')
                )
                    CREATE INDEX [IX_PaymentWebhookLogs_ResourceId]
                    ON [PaymentWebhookLogs] ([ResourceId]);
                """);

            migrationBuilder.Sql(
                """
                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_Trainers_PublicPageEnabled_PublicSearchEnabled_AcceptingStudents'
                      AND object_id = OBJECT_ID('Trainers')
                )
                    CREATE INDEX [IX_Trainers_PublicPageEnabled_PublicSearchEnabled_AcceptingStudents]
                    ON [Trainers] ([PublicPageEnabled], [PublicSearchEnabled], [AcceptingStudents]);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_Trainers_PublicPageEnabled_PublicSearchEnabled_AcceptingStudents'
                      AND object_id = OBJECT_ID('Trainers')
                )
                    DROP INDEX [IX_Trainers_PublicPageEnabled_PublicSearchEnabled_AcceptingStudents] ON [Trainers];
                """);

            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_PaymentWebhookLogs_ResourceId'
                      AND object_id = OBJECT_ID('PaymentWebhookLogs')
                )
                    DROP INDEX [IX_PaymentWebhookLogs_ResourceId] ON [PaymentWebhookLogs];
                """);

            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_PaymentWebhookLogs_Provider_EventId'
                      AND object_id = OBJECT_ID('PaymentWebhookLogs')
                )
                    DROP INDEX [IX_PaymentWebhookLogs_Provider_EventId] ON [PaymentWebhookLogs];
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[PaymentWebhookLogs]', N'U') IS NOT NULL
                    DROP TABLE [PaymentWebhookLogs];
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('TrainerPayments', 'RawPayload') IS NOT NULL
                    ALTER TABLE [TrainerPayments] DROP COLUMN [RawPayload];
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('TrainerPayments', 'ProviderSubscriptionId') IS NOT NULL
                    ALTER TABLE [TrainerPayments] DROP COLUMN [ProviderSubscriptionId];
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('TrainerSubscriptions', 'MercadoPagoPayerId') IS NOT NULL
                    ALTER TABLE [TrainerSubscriptions] DROP COLUMN [MercadoPagoPayerId];
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('TrainerSubscriptions', 'InitPoint') IS NOT NULL
                    ALTER TABLE [TrainerSubscriptions] DROP COLUMN [InitPoint];
                """);
        }
    }
}
