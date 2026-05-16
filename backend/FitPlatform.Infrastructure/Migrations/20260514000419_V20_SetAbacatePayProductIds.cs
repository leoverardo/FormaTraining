using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class V20_SetAbacatePayProductIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TrainerOnboardingId",
                table: "TrainerSubscriptions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainerSubscriptions_TrainerOnboardingId",
                table: "TrainerSubscriptions",
                column: "TrainerOnboardingId");

            migrationBuilder.Sql(@"
UPDATE pbo
SET pbo.AbacatePayProductId = 'prod_QTnE1M6UHFwqhxpTJhHahrxB'
FROM PlanBillingOptions pbo
INNER JOIN PlatformPlans pp ON pp.Id = pbo.PlatformPlanId
WHERE pp.Name = 'Starter' AND pbo.BillingCycle = 1;

UPDATE pbo
SET pbo.AbacatePayProductId = 'prod_26kYBznyFUgmSYUNZ25dY6kj'
FROM PlanBillingOptions pbo
INNER JOIN PlatformPlans pp ON pp.Id = pbo.PlatformPlanId
WHERE pp.Name = 'Starter' AND pbo.BillingCycle = 3;

UPDATE pbo
SET pbo.AbacatePayProductId = 'prod_bn3nSLyu2mfwePQj4sAu3kxu'
FROM PlanBillingOptions pbo
INNER JOIN PlatformPlans pp ON pp.Id = pbo.PlatformPlanId
WHERE pp.Name = 'Starter' AND pbo.BillingCycle = 4;

UPDATE pbo
SET pbo.AbacatePayProductId = 'prod_UwDswqSBuexnuubjkEjf5tmK'
FROM PlanBillingOptions pbo
INNER JOIN PlatformPlans pp ON pp.Id = pbo.PlatformPlanId
WHERE pp.Name = 'Pro' AND pbo.BillingCycle = 1;

UPDATE pbo
SET pbo.AbacatePayProductId = 'prod_3BcYD4jbJjMpYwrTkGHEZCzL'
FROM PlanBillingOptions pbo
INNER JOIN PlatformPlans pp ON pp.Id = pbo.PlatformPlanId
WHERE pp.Name = 'Pro' AND pbo.BillingCycle = 3;

UPDATE pbo
SET pbo.AbacatePayProductId = 'prod_JYXpgmD1Ch1wcNT3YZKLNq41'
FROM PlanBillingOptions pbo
INNER JOIN PlatformPlans pp ON pp.Id = pbo.PlatformPlanId
WHERE pp.Name = 'Pro' AND pbo.BillingCycle = 4;

UPDATE pbo
SET pbo.AbacatePayProductId = 'prod_hFfqP2jgtwnmyccdShUZfmLM'
FROM PlanBillingOptions pbo
INNER JOIN PlatformPlans pp ON pp.Id = pbo.PlatformPlanId
WHERE pp.Name = 'Growth' AND pbo.BillingCycle = 1;

UPDATE pbo
SET pbo.AbacatePayProductId = 'prod_3pcSXZYRSQSTbkMnraNp4Sdm'
FROM PlanBillingOptions pbo
INNER JOIN PlatformPlans pp ON pp.Id = pbo.PlatformPlanId
WHERE pp.Name = 'Growth' AND pbo.BillingCycle = 3;

UPDATE pbo
SET pbo.AbacatePayProductId = 'prod_mKU52WXcjTJpDRw6xpEJmmDp'
FROM PlanBillingOptions pbo
INNER JOIN PlatformPlans pp ON pp.Id = pbo.PlatformPlanId
WHERE pp.Name = 'Growth' AND pbo.BillingCycle = 4;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE pbo
SET pbo.AbacatePayProductId = NULL
FROM PlanBillingOptions pbo
INNER JOIN PlatformPlans pp ON pp.Id = pbo.PlatformPlanId
WHERE pp.Name IN ('Starter', 'Pro', 'Growth')
  AND pbo.BillingCycle IN (1, 3, 4)
  AND pbo.AbacatePayProductId IN (
    'prod_QTnE1M6UHFwqhxpTJhHahrxB',
    'prod_26kYBznyFUgmSYUNZ25dY6kj',
    'prod_bn3nSLyu2mfwePQj4sAu3kxu',
    'prod_UwDswqSBuexnuubjkEjf5tmK',
    'prod_3BcYD4jbJjMpYwrTkGHEZCzL',
    'prod_JYXpgmD1Ch1wcNT3YZKLNq41',
    'prod_hFfqP2jgtwnmyccdShUZfmLM',
    'prod_3pcSXZYRSQSTbkMnraNp4Sdm',
    'prod_mKU52WXcjTJpDRw6xpEJmmDp'
  );
");

            migrationBuilder.DropIndex(
                name: "IX_TrainerSubscriptions_TrainerOnboardingId",
                table: "TrainerSubscriptions");

            migrationBuilder.DropColumn(
                name: "TrainerOnboardingId",
                table: "TrainerSubscriptions");
        }
    }
}
