using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class V19_SaasOnboardingCouponFinalization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AbacatePayCheckoutId",
                table: "TrainerSubscriptions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AbacatePayCustomerId",
                table: "TrainerSubscriptions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AbacatePaySubscriptionId",
                table: "TrainerSubscriptions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BaseAmountInCents",
                table: "TrainerSubscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CouponCodeApplied",
                table: "TrainerSubscriptions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CouponDiscountAmountInCents",
                table: "TrainerSubscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CycleDiscountAmountInCents",
                table: "TrainerSubscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FinalAmountInCents",
                table: "TrainerSubscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "TrainerSubscriptions",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AbacatePayCheckoutId",
                table: "TrainerPayments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DiscountCoupons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiscountType = table.Column<int>(type: "int", nullable: false),
                    DiscountValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MaxUsesTotal = table.Column<int>(type: "int", nullable: true),
                    MaxUsesPerCustomer = table.Column<int>(type: "int", nullable: true),
                    CurrentUses = table.Column<int>(type: "int", nullable: false),
                    StartsAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AppliesToPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AppliesToBillingCycle = table.Column<int>(type: "int", nullable: true),
                    MinimumPurchaseAmountInCents = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountCoupons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlanBillingOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlatformPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BillingCycle = table.Column<int>(type: "int", nullable: false),
                    MonthsCount = table.Column<int>(type: "int", nullable: false),
                    CycleDiscountPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    BasePriceInCents = table.Column<int>(type: "int", nullable: false),
                    FinalPriceInCents = table.Column<int>(type: "int", nullable: false),
                    AbacatePayProductId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanBillingOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanBillingOptions_PlatformPlans_PlatformPlanId",
                        column: x => x.PlatformPlanId,
                        principalTable: "PlatformPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DiscountCouponRedemptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CouponId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RedeemedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DiscountAmountInCents = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountCouponRedemptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscountCouponRedemptions_DiscountCoupons_CouponId",
                        column: x => x.CouponId,
                        principalTable: "DiscountCoupons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrainerSubscriptions_AbacatePaySubscriptionId",
                table: "TrainerSubscriptions",
                column: "AbacatePaySubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainerSubscriptions_Provider",
                table: "TrainerSubscriptions",
                column: "Provider");

            migrationBuilder.CreateIndex(
                name: "IX_TrainerPayments_AbacatePayCheckoutId",
                table: "TrainerPayments",
                column: "AbacatePayCheckoutId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountCouponRedemptions_CouponId_TrainerId_SubscriptionId",
                table: "DiscountCouponRedemptions",
                columns: new[] { "CouponId", "TrainerId", "SubscriptionId" },
                unique: true,
                filter: "[TrainerId] IS NOT NULL AND [SubscriptionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountCoupons_Code",
                table: "DiscountCoupons",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanBillingOptions_PlatformPlanId_BillingCycle",
                table: "PlanBillingOptions",
                columns: new[] { "PlatformPlanId", "BillingCycle" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiscountCouponRedemptions");

            migrationBuilder.DropTable(
                name: "PlanBillingOptions");

            migrationBuilder.DropTable(
                name: "DiscountCoupons");

            migrationBuilder.DropIndex(
                name: "IX_TrainerSubscriptions_AbacatePaySubscriptionId",
                table: "TrainerSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_TrainerSubscriptions_Provider",
                table: "TrainerSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_TrainerPayments_AbacatePayCheckoutId",
                table: "TrainerPayments");

            migrationBuilder.DropColumn(
                name: "AbacatePayCheckoutId",
                table: "TrainerSubscriptions");

            migrationBuilder.DropColumn(
                name: "AbacatePayCustomerId",
                table: "TrainerSubscriptions");

            migrationBuilder.DropColumn(
                name: "AbacatePaySubscriptionId",
                table: "TrainerSubscriptions");

            migrationBuilder.DropColumn(
                name: "BaseAmountInCents",
                table: "TrainerSubscriptions");

            migrationBuilder.DropColumn(
                name: "CouponCodeApplied",
                table: "TrainerSubscriptions");

            migrationBuilder.DropColumn(
                name: "CouponDiscountAmountInCents",
                table: "TrainerSubscriptions");

            migrationBuilder.DropColumn(
                name: "CycleDiscountAmountInCents",
                table: "TrainerSubscriptions");

            migrationBuilder.DropColumn(
                name: "FinalAmountInCents",
                table: "TrainerSubscriptions");

            migrationBuilder.DropColumn(
                name: "Provider",
                table: "TrainerSubscriptions");

            migrationBuilder.DropColumn(
                name: "AbacatePayCheckoutId",
                table: "TrainerPayments");
        }
    }
}
