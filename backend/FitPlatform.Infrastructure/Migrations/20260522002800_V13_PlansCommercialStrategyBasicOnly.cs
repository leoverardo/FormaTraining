using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class V13_PlansCommercialStrategyBasicOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH('PlatformPlans', 'Code') IS NULL
                BEGIN
                    ALTER TABLE PlatformPlans ADD Code nvarchar(450) NULL;
                END;
                """);

            migrationBuilder.Sql(
                """
                UPDATE PlatformPlans
                SET Code = CASE
                    WHEN UPPER(ISNULL(Code, '')) = '' AND UPPER(Name) IN ('STARTER','BASIC') THEN 'BASIC'
                    WHEN UPPER(ISNULL(Code, '')) = '' AND UPPER(Name) = 'PRO' THEN 'PRO'
                    WHEN UPPER(ISNULL(Code, '')) = '' AND UPPER(Name) = 'GROWTH' THEN 'GROWTH'
                    WHEN UPPER(ISNULL(Code, '')) = '' AND UPPER(ISNULL(Name, '')) <> '' THEN UPPER(REPLACE(REPLACE(Name, ' ', '_'), '-', '_'))
                    WHEN UPPER(ISNULL(Code, '')) = '' THEN CONCAT('PLAN_', REPLACE(CONVERT(varchar(36), Id), '-', ''))
                    ELSE UPPER(Code)
                END;
                """);

            migrationBuilder.Sql(
                """
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PlatformPlans_Code' AND object_id = OBJECT_ID('PlatformPlans'))
                BEGIN
                    CREATE UNIQUE INDEX IX_PlatformPlans_Code ON PlatformPlans(Code);
                END;
                """);

            migrationBuilder.Sql(
                """
                UPDATE p
                SET
                    p.Name = 'Basic',
                    p.Description = 'Tudo que o personal precisa para gerenciar alunos, montar treinos e acompanhar a evolução em uma plataforma profissional.',
                    p.MonthlyPrice = 59.90,
                    p.MaxActiveStudents = 0,
                    p.HasUnlimitedStudents = 1,
                    p.Active = 1,
                    p.IsPublic = 1,
                    p.IsComingSoon = 0,
                    p.IsAvailableForPurchase = 1,
                    p.UpdatedAt = SYSUTCDATETIME()
                FROM PlatformPlans p
                WHERE UPPER(p.Code) = 'BASIC' OR UPPER(p.Name) IN ('STARTER','BASIC');
                """);

            migrationBuilder.Sql(
                """
                UPDATE p
                SET
                    p.Description = 'Para automatizar atendimento e retenção',
                    p.IsComingSoon = 1,
                    p.IsAvailableForPurchase = 0,
                    p.IsPublic = 1,
                    p.Active = 1,
                    p.UpdatedAt = SYSUTCDATETIME()
                FROM PlatformPlans p
                WHERE UPPER(p.Code) = 'PRO' OR UPPER(p.Name) = 'PRO';
                """);

            migrationBuilder.Sql(
                """
                UPDATE p
                SET
                    p.Description = 'Para captar alunos e crescer o negócio',
                    p.IsComingSoon = 1,
                    p.IsAvailableForPurchase = 0,
                    p.IsPublic = 1,
                    p.Active = 1,
                    p.UpdatedAt = SYSUTCDATETIME()
                FROM PlatformPlans p
                WHERE UPPER(p.Code) = 'GROWTH' OR UPPER(p.Name) = 'GROWTH';
                """);

            migrationBuilder.Sql(
                """
                DECLARE @basicId uniqueidentifier = (
                    SELECT TOP 1 Id FROM PlatformPlans
                    WHERE UPPER(Code) = 'BASIC' OR UPPER(Name) IN ('STARTER','BASIC')
                    ORDER BY CreatedAt
                );

                IF @basicId IS NOT NULL
                BEGIN
                    UPDATE PlatformPlanPrices SET Price = 59.90, Active = 1 WHERE PlatformPlanId = @basicId AND BillingCycle = 1;
                    UPDATE PlatformPlanPrices SET Price = 161.73, Active = 1 WHERE PlatformPlanId = @basicId AND BillingCycle = 2;
                    UPDATE PlatformPlanPrices SET Price = 305.49, Active = 1 WHERE PlatformPlanId = @basicId AND BillingCycle = 3;
                    UPDATE PlatformPlanPrices SET Price = 575.04, Active = 1 WHERE PlatformPlanId = @basicId AND BillingCycle = 4;

                    UPDATE PlanBillingOptions SET
                        MonthlyPrice = 59.90,
                        BasePriceInCents = 5990,
                        CycleDiscountPercent = 0,
                        CycleDiscountAmountInCents = 0,
                        FinalPriceInCents = 5990,
                        IsActive = 1,
                        UpdatedAt = SYSUTCDATETIME()
                    WHERE PlatformPlanId = @basicId AND BillingCycle = 1;

                    UPDATE PlanBillingOptions SET
                        MonthlyPrice = 59.90,
                        BasePriceInCents = 17970,
                        CycleDiscountPercent = 10,
                        CycleDiscountAmountInCents = 1797,
                        FinalPriceInCents = 16173,
                        IsActive = 1,
                        UpdatedAt = SYSUTCDATETIME()
                    WHERE PlatformPlanId = @basicId AND BillingCycle = 2;

                    UPDATE PlanBillingOptions SET
                        MonthlyPrice = 59.90,
                        BasePriceInCents = 35940,
                        CycleDiscountPercent = 15,
                        CycleDiscountAmountInCents = 5391,
                        FinalPriceInCents = 30549,
                        IsActive = 1,
                        UpdatedAt = SYSUTCDATETIME()
                    WHERE PlatformPlanId = @basicId AND BillingCycle = 3;

                    UPDATE PlanBillingOptions SET
                        MonthlyPrice = 59.90,
                        BasePriceInCents = 71880,
                        CycleDiscountPercent = 20,
                        CycleDiscountAmountInCents = 14376,
                        FinalPriceInCents = 57504,
                        IsActive = 1,
                        UpdatedAt = SYSUTCDATETIME()
                    WHERE PlatformPlanId = @basicId AND BillingCycle = 4;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE p
                SET
                    p.Name = 'Starter',
                    p.Description = 'Ideal para personal trainers iniciantes',
                    p.MonthlyPrice = 97.00,
                    p.MaxActiveStudents = 20,
                    p.HasUnlimitedStudents = 0,
                    p.IsComingSoon = 0,
                    p.IsAvailableForPurchase = 1,
                    p.UpdatedAt = SYSUTCDATETIME()
                FROM PlatformPlans p
                WHERE UPPER(p.Code) = 'BASIC' OR UPPER(p.Name) = 'BASIC';
                """);
        }
    }
}
