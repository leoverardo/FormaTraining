using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitPlatform.Infrastructure.Migrations
{
    public partial class V22_RemoveQuarterlyBillingCycle : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                -- Remove ciclo trimestral (2) de tabelas de precificacao
                IF OBJECT_ID('dbo.PlatformPlanPrices', 'U') IS NOT NULL
                BEGIN
                    DELETE FROM dbo.PlatformPlanPrices WHERE BillingCycle = 2;
                END;

                IF OBJECT_ID('dbo.PlanBillingOptions', 'U') IS NOT NULL
                BEGIN
                    DELETE FROM dbo.PlanBillingOptions WHERE BillingCycle = 2;
                END;

                -- Assinaturas existentes em trimestral passam para mensal
                IF OBJECT_ID('dbo.TrainerSubscriptions', 'U') IS NOT NULL
                BEGIN
                    UPDATE ts
                    SET ts.BillingCycle = 1
                    FROM dbo.TrainerSubscriptions ts
                    WHERE ts.BillingCycle = 2;

                    IF COL_LENGTH('dbo.TrainerSubscriptions', 'PlatformPlanPriceId') IS NOT NULL
                    BEGIN
                        UPDATE ts
                        SET ts.PlatformPlanPriceId = ppp.Id
                        FROM dbo.TrainerSubscriptions ts
                        INNER JOIN dbo.PlatformPlanPrices ppp
                            ON ppp.PlatformPlanId = ts.PlatformPlanId
                           AND ppp.BillingCycle = 1
                           AND ppp.Active = 1
                        WHERE ts.PlatformPlanPriceId IS NULL OR ts.BillingCycle = 1;
                    END
                END;

                -- Onboarding pendente em trimestral passa para mensal
                IF OBJECT_ID('dbo.TrainerOnboardings', 'U') IS NOT NULL
                BEGIN
                    UPDATE o
                    SET o.BillingCycle = 1
                    FROM dbo.TrainerOnboardings o
                    WHERE o.BillingCycle = 2;
                END;

                -- Cupons restritos ao trimestral deixam de restringir por ciclo
                IF OBJECT_ID('dbo.DiscountCoupons', 'U') IS NOT NULL
                   AND COL_LENGTH('dbo.DiscountCoupons', 'AppliesToBillingCycle') IS NOT NULL
                BEGIN
                    UPDATE c
                    SET c.AppliesToBillingCycle = NULL
                    FROM dbo.DiscountCoupons c
                    WHERE c.AppliesToBillingCycle = 2;
                END;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
