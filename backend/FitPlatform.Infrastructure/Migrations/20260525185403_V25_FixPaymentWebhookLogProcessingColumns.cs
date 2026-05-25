using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class V25_FixPaymentWebhookLogProcessingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF COL_LENGTH('PaymentWebhookLogs', 'ProcessingStatus') IS NULL
                BEGIN
                    ALTER TABLE [PaymentWebhookLogs]
                    ADD [ProcessingStatus] INT NOT NULL
                    CONSTRAINT [DF_PaymentWebhookLogs_ProcessingStatus] DEFAULT 0;
                END
            ");

            migrationBuilder.Sql(@"
                IF COL_LENGTH('PaymentWebhookLogs', 'ErrorMessage') IS NULL
                BEGIN
                    ALTER TABLE [PaymentWebhookLogs]
                    ADD [ErrorMessage] NVARCHAR(MAX) NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF COL_LENGTH('PaymentWebhookLogs', 'RetryCount') IS NULL
                BEGIN
                    ALTER TABLE [PaymentWebhookLogs]
                    ADD [RetryCount] INT NOT NULL
                    CONSTRAINT [DF_PaymentWebhookLogs_RetryCount] DEFAULT 0;
                END
            ");

            migrationBuilder.Sql(@"
                UPDATE [PaymentWebhookLogs]
                SET [ProcessingStatus] = 2
                WHERE [ProcessedAt] IS NOT NULL
                  AND [ProcessingStatus] <> 2;
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_PaymentWebhookLogs_ProcessingStatus'
                      AND object_id = OBJECT_ID('PaymentWebhookLogs')
                )
                BEGIN
                    CREATE INDEX [IX_PaymentWebhookLogs_ProcessingStatus]
                    ON [PaymentWebhookLogs] ([ProcessingStatus]);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_PaymentWebhookLogs_ProcessingStatus'
                      AND object_id = OBJECT_ID('PaymentWebhookLogs')
                )
                BEGIN
                    DROP INDEX [IX_PaymentWebhookLogs_ProcessingStatus]
                    ON [PaymentWebhookLogs];
                END
            ");

            migrationBuilder.Sql(@"
                DECLARE @constraintName NVARCHAR(200);

                IF COL_LENGTH('PaymentWebhookLogs', 'RetryCount') IS NOT NULL
                BEGIN
                    SELECT @constraintName = dc.name
                    FROM sys.default_constraints dc
                    INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
                    INNER JOIN sys.tables t ON t.object_id = c.object_id
                    WHERE t.name = 'PaymentWebhookLogs'
                      AND c.name = 'RetryCount';

                    IF @constraintName IS NOT NULL
                        EXEC('ALTER TABLE [PaymentWebhookLogs] DROP CONSTRAINT [' + @constraintName + ']');

                    ALTER TABLE [PaymentWebhookLogs] DROP COLUMN [RetryCount];
                END
            ");

            migrationBuilder.Sql(@"
                IF COL_LENGTH('PaymentWebhookLogs', 'ErrorMessage') IS NOT NULL
                BEGIN
                    ALTER TABLE [PaymentWebhookLogs] DROP COLUMN [ErrorMessage];
                END
            ");

            migrationBuilder.Sql(@"
                DECLARE @constraintName NVARCHAR(200);

                IF COL_LENGTH('PaymentWebhookLogs', 'ProcessingStatus') IS NOT NULL
                BEGIN
                    SELECT @constraintName = dc.name
                    FROM sys.default_constraints dc
                    INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
                    INNER JOIN sys.tables t ON t.object_id = c.object_id
                    WHERE t.name = 'PaymentWebhookLogs'
                      AND c.name = 'ProcessingStatus';

                    IF @constraintName IS NOT NULL
                        EXEC('ALTER TABLE [PaymentWebhookLogs] DROP CONSTRAINT [' + @constraintName + ']');

                    ALTER TABLE [PaymentWebhookLogs] DROP COLUMN [ProcessingStatus];
                END
            ");
        }
    }
}