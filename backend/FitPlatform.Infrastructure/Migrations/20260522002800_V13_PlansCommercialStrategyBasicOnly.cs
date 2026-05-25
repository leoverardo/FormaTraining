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
                IF COL_LENGTH('dbo.PlatformPlans', 'Code') IS NULL
                BEGIN
                    ALTER TABLE dbo.PlatformPlans
                    ADD Code NVARCHAR(50) NULL;
                END;

                IF COL_LENGTH('dbo.PlatformPlans', 'HasUnlimitedStudents') IS NULL
                BEGIN
                    ALTER TABLE dbo.PlatformPlans
                    ADD HasUnlimitedStudents bit NOT NULL CONSTRAINT DF_PlatformPlans_HasUnlimitedStudents DEFAULT(0);
                END;

                IF COL_LENGTH('dbo.PlatformPlans', 'IsPublic') IS NULL
                BEGIN
                    ALTER TABLE dbo.PlatformPlans
                    ADD IsPublic bit NOT NULL CONSTRAINT DF_PlatformPlans_IsPublic DEFAULT(1);
                END;

                IF COL_LENGTH('dbo.PlatformPlans', 'IsComingSoon') IS NULL
                BEGIN
                    ALTER TABLE dbo.PlatformPlans
                    ADD IsComingSoon bit NOT NULL CONSTRAINT DF_PlatformPlans_IsComingSoon DEFAULT(0);
                END;

                IF COL_LENGTH('dbo.PlatformPlans', 'IsAvailableForPurchase') IS NULL
                BEGIN
                    ALTER TABLE dbo.PlatformPlans
                    ADD IsAvailableForPurchase bit NOT NULL CONSTRAINT DF_PlatformPlans_IsAvailableForPurchase DEFAULT(1);
                END;
                """);

            migrationBuilder.Sql(
                """
                UPDATE PlatformPlans
                SET Code = CASE
                    WHEN UPPER(ISNULL(Code, '')) = '' AND UPPER(Name) IN ('STARTER','BASIC') THEN 'BASIC'
                    WHEN UPPER(ISNULL(Code, '')) = '' AND UPPER(Name) = 'PRO' THEN 'PRO'
                    WHEN UPPER(ISNULL(Code, '')) = '' AND UPPER(Name) = 'GROWTH' THEN 'GROWTH'
                    ELSE UPPER(Code)
                END;
                UPDATE dbo.PlatformPlans
                SET Code = UPPER(Name)
                WHERE Code IS NULL OR LTRIM(RTRIM(Code)) = '';
                """);

            migrationBuilder.Sql(
                """
                IF EXISTS (SELECT 1 FROM dbo.PlatformPlans WHERE Code IS NULL OR LTRIM(RTRIM(Code)) = '')
                BEGIN
                    THROW 50001, 'Não foi possível normalizar PlatformPlans.Code para todos os registros.', 1;
                END;

                IF EXISTS (
                    SELECT Code
                    FROM dbo.PlatformPlans
                    GROUP BY Code
                    HAVING COUNT(*) > 1
                )
                BEGIN
                    ;WITH dupes AS (
                        SELECT
                            Id,
                            Code,
                            ROW_NUMBER() OVER (PARTITION BY Code ORDER BY CreatedAt, Id) AS rn
                        FROM dbo.PlatformPlans
                    )
                    UPDATE p
                    SET p.Code = CONCAT(p.Code, '_', RIGHT(CONVERT(varchar(36), p.Id), 8))
                    FROM dbo.PlatformPlans p
                    INNER JOIN dupes d ON d.Id = p.Id
                    WHERE d.rn > 1;
                END;
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE dbo.PlatformPlans
                ALTER COLUMN Code NVARCHAR(50) NOT NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PlatformPlans_Code' AND object_id = OBJECT_ID('dbo.PlatformPlans'))
                BEGIN
                    CREATE UNIQUE INDEX IX_PlatformPlans_Code ON dbo.PlatformPlans(Code);
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
                        MonthsCount = 1,
                        BasePriceInCents = 5990,
                        CycleDiscountPercent = 0,
                        FinalPriceInCents = 5990,
                        IsActive = 1,
                        UpdatedAt = SYSUTCDATETIME()
                    WHERE PlatformPlanId = @basicId AND BillingCycle = 1;

                    UPDATE PlanBillingOptions SET
                        MonthsCount = 3,
                        BasePriceInCents = 17970,
                        CycleDiscountPercent = 10,
                        FinalPriceInCents = 16173,
                        IsActive = 1,
                        UpdatedAt = SYSUTCDATETIME()
                    WHERE PlatformPlanId = @basicId AND BillingCycle = 2;

                    UPDATE PlanBillingOptions SET
                        MonthsCount = 6,
                        BasePriceInCents = 35940,
                        CycleDiscountPercent = 15,
                        FinalPriceInCents = 30549,
                        IsActive = 1,
                        UpdatedAt = SYSUTCDATETIME()
                    WHERE PlatformPlanId = @basicId AND BillingCycle = 3;

                    UPDATE PlanBillingOptions SET
                        MonthsCount = 12,
                        BasePriceInCents = 71880,
                        CycleDiscountPercent = 20,
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
