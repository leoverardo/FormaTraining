using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitPlatform.Infrastructure.Migrations
{
    /// <summary>
    /// V23 — Adiciona campos de rastreamento de status ao PaymentWebhookLog:
    ///   - ProcessingStatus (int): ciclo de vida do processamento (Pending→Processing→Processed|Failed).
    ///   - ErrorMessage (nvarchar(max)): detalhe do erro quando Status = Failed.
    ///   - RetryCount (int): número de tentativas de reprocessamento.
    ///
    /// NOTA: O arquivo .Designer.cs desta migration não é gerado automaticamente.
    /// Para regenerar o snapshot do modelo (necessário para adicionar futuras migrations
    /// via EF tooling), execute após aplicar esta migration:
    ///   dotnet ef migrations add V23_WebhookEventTracking_Snapshot --output-dir Migrations
    /// e descarte a migration gerada, mantendo apenas o snapshot atualizado.
    /// Para deploys via db.Database.Migrate() (padrão do projeto), o Designer não é necessário.
    /// </summary>
    public partial class V23_WebhookEventTracking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProcessingStatus",
                table: "PaymentWebhookLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "PaymentWebhookLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RetryCount",
                table: "PaymentWebhookLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Marcar eventos previamente processados como Processed (2).
            migrationBuilder.Sql(
                "UPDATE [PaymentWebhookLogs] SET [ProcessingStatus] = 2 WHERE [ProcessedAt] IS NOT NULL;");

            // Índice para queries de monitoramento/retry por status.
            migrationBuilder.CreateIndex(
                name: "IX_PaymentWebhookLogs_ProcessingStatus",
                table: "PaymentWebhookLogs",
                column: "ProcessingStatus");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentWebhookLogs_ProcessingStatus",
                table: "PaymentWebhookLogs");

            migrationBuilder.DropColumn(
                name: "ProcessingStatus",
                table: "PaymentWebhookLogs");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "PaymentWebhookLogs");

            migrationBuilder.DropColumn(
                name: "RetryCount",
                table: "PaymentWebhookLogs");
        }
    }
}
