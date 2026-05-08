using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitPlatform.Infrastructure.Migrations
{
    public partial class V9_MercadoPagoSubscriptionsWebhook : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProviderSubscriptionId",
                table: "TrainerPayments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RawPayload",
                table: "TrainerPayments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InitPoint",
                table: "TrainerSubscriptions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MercadoPagoPayerId",
                table: "TrainerSubscriptions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PaymentWebhookLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EventId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResourceId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RawPayload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentWebhookLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentWebhookLogs_Provider_EventId",
                table: "PaymentWebhookLogs",
                columns: new[] { "Provider", "EventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentWebhookLogs_ResourceId",
                table: "PaymentWebhookLogs",
                column: "ResourceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentWebhookLogs");

            migrationBuilder.DropColumn(
                name: "ProviderSubscriptionId",
                table: "TrainerPayments");

            migrationBuilder.DropColumn(
                name: "RawPayload",
                table: "TrainerPayments");

            migrationBuilder.DropColumn(
                name: "InitPoint",
                table: "TrainerSubscriptions");

            migrationBuilder.DropColumn(
                name: "MercadoPagoPayerId",
                table: "TrainerSubscriptions");
        }
    }
}
