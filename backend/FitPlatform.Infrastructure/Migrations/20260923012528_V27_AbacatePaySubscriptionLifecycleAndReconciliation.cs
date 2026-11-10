using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitPlatform.Infrastructure.Migrations
{
    /// <summary>
    /// Suporte a ciclo de vida completo da assinatura AbacatePay.
    ///
    /// TrainerSubscriptions
    ///   • PaymentFailureCount / LastPaymentFailureAt — registro de falhas de cobrança
    ///     recorrente (subscription.payment_failed) sem cancelar a assinatura.
    ///   • Pending{PlatformPlanId, BillingCycle, PlanChangeProviderId, PlanChangeRequestedAt,
    ///     FinalAmountInCents} — troca de plano agendada; a AbacatePay aplica a troca
    ///     no início do próximo ciclo, então o plano vigente só muda na renovação.
    ///   • LastReconciledAt / ReconciliationAttempts / ProviderStatus — controle da rotina
    ///     de reconciliação, incluindo o limite de tentativas por assinatura.
    ///   • Índices (AbacatePayCheckoutId) e (Status, CreatedAt) para a varredura do lote.
    ///
    /// PaymentWebhookLogs
    ///   • LastAttemptAt / NextRetryAt — estratégia de retry com backoff exponencial,
    ///     complementando RetryCount e ErrorMessage que já existiam.
    ///   • Índice (ProcessingStatus, NextRetryAt) para selecionar o lote de retry.
    ///
    /// DiscountCoupons
    ///   • ProviderCouponCode / ProviderSyncedAt — espelhamento idempotente do cupom na
    ///     AbacatePay, para que o desconto calculado pelo backend seja de fato cobrado.
    ///
    /// Observação sobre o alerta de perda de dados do scaffolding:
    /// TrainerSubscriptions.AbacatePayCheckoutId passa de nvarchar(max) para nvarchar(450)
    /// porque colunas indexadas no SQL Server não podem ser nvarchar(max). Os identificadores
    /// da AbacatePay têm poucas dezenas de caracteres (ex.: "bill_abc123xyz"), portanto não
    /// há truncamento real. Verifique com a query abaixo antes de aplicar em produção:
    ///   SELECT MAX(LEN(AbacatePayCheckoutId)) FROM TrainerSubscriptions;
    /// </summary>
    public partial class V27_AbacatePaySubscriptionLifecycleAndReconciliation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AbacatePayCheckoutId",
                table: "TrainerSubscriptions",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPaymentFailureAt",
                table: "TrainerSubscriptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastReconciledAt",
                table: "TrainerSubscriptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentFailureCount",
                table: "TrainerSubscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PendingBillingCycle",
                table: "TrainerSubscriptions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PendingFinalAmountInCents",
                table: "TrainerSubscriptions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PendingPlanChangeProviderId",
                table: "TrainerSubscriptions",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PendingPlanChangeRequestedAt",
                table: "TrainerSubscriptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PendingPlatformPlanId",
                table: "TrainerSubscriptions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderStatus",
                table: "TrainerSubscriptions",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReconciliationAttempts",
                table: "TrainerSubscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastAttemptAt",
                table: "PaymentWebhookLogs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextRetryAt",
                table: "PaymentWebhookLogs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderCouponCode",
                table: "DiscountCoupons",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProviderSyncedAt",
                table: "DiscountCoupons",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainerSubscriptions_AbacatePayCheckoutId",
                table: "TrainerSubscriptions",
                column: "AbacatePayCheckoutId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainerSubscriptions_Status_CreatedAt",
                table: "TrainerSubscriptions",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentWebhookLogs_ProcessingStatus_NextRetryAt",
                table: "PaymentWebhookLogs",
                columns: new[] { "ProcessingStatus", "NextRetryAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrainerSubscriptions_AbacatePayCheckoutId",
                table: "TrainerSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_TrainerSubscriptions_Status_CreatedAt",
                table: "TrainerSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_PaymentWebhookLogs_ProcessingStatus_NextRetryAt",
                table: "PaymentWebhookLogs");

            migrationBuilder.DropColumn(
                name: "LastPaymentFailureAt",
                table: "TrainerSubscriptions");

            migrationBuilder.DropColumn(
                name: "LastReconciledAt",
                table: "TrainerSubscriptions");

            migrationBuilder.DropColumn(
                name: "PaymentFailureCount",
                table: "TrainerSubscriptions");

            migrationBuilder.DropColumn(
                name: "PendingBillingCycle",
                table: "TrainerSubscriptions");

            migrationBuilder.DropColumn(
                name: "PendingFinalAmountInCents",
                table: "TrainerSubscriptions");

            migrationBuilder.DropColumn(
                name: "PendingPlanChangeProviderId",
                table: "TrainerSubscriptions");

            migrationBuilder.DropColumn(
                name: "PendingPlanChangeRequestedAt",
                table: "TrainerSubscriptions");

            migrationBuilder.DropColumn(
                name: "PendingPlatformPlanId",
                table: "TrainerSubscriptions");

            migrationBuilder.DropColumn(
                name: "ProviderStatus",
                table: "TrainerSubscriptions");

            migrationBuilder.DropColumn(
                name: "ReconciliationAttempts",
                table: "TrainerSubscriptions");

            migrationBuilder.DropColumn(
                name: "LastAttemptAt",
                table: "PaymentWebhookLogs");

            migrationBuilder.DropColumn(
                name: "NextRetryAt",
                table: "PaymentWebhookLogs");

            migrationBuilder.DropColumn(
                name: "ProviderCouponCode",
                table: "DiscountCoupons");

            migrationBuilder.DropColumn(
                name: "ProviderSyncedAt",
                table: "DiscountCoupons");

            migrationBuilder.AlterColumn<string>(
                name: "AbacatePayCheckoutId",
                table: "TrainerSubscriptions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
