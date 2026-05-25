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
                IF COL_LENGTH('PlatformPlans', 'Description') IS NULL
                BEGIN
                    ALTER TABLE PlatformPlans ADD Description nvarchar(max) NULL;
                END;

                IF COL_LENGTH('PlatformPlans', 'MonthlyPrice') IS NULL
                BEGIN
                    ALTER TABLE PlatformPlans ADD MonthlyPrice decimal(18,2) NOT NULL CONSTRAINT DF_PlatformPlans_MonthlyPrice_V13 DEFAULT(0) WITH VALUES;
                END;

                IF COL_LENGTH('PlatformPlans', 'MaxActiveStudents') IS NULL
                BEGIN
                    ALTER TABLE PlatformPlans ADD MaxActiveStudents int NOT NULL CONSTRAINT DF_PlatformPlans_MaxActiveStudents_V13 DEFAULT(0) WITH VALUES;
                END;

                IF COL_LENGTH('PlatformPlans', 'HasUnlimitedStudents') IS NULL
                BEGIN
                    ALTER TABLE PlatformPlans ADD HasUnlimitedStudents bit NOT NULL CONSTRAINT DF_PlatformPlans_HasUnlimitedStudents_V13 DEFAULT(0) WITH VALUES;
                END;

                IF COL_LENGTH('PlatformPlans', 'Active') IS NULL
                BEGIN
                    ALTER TABLE PlatformPlans ADD Active bit NOT NULL CONSTRAINT DF_PlatformPlans_Active_V13 DEFAULT(1) WITH VALUES;
                END;

                IF COL_LENGTH('PlatformPlans', 'IsPublic') IS NULL
                BEGIN
                    ALTER TABLE PlatformPlans ADD IsPublic bit NOT NULL CONSTRAINT DF_PlatformPlans_IsPublic_V13 DEFAULT(1) WITH VALUES;
                END;

                IF COL_LENGTH('PlatformPlans', 'IsComingSoon') IS NULL
                BEGIN
                    ALTER TABLE PlatformPlans ADD IsComingSoon bit NOT NULL CONSTRAINT DF_PlatformPlans_IsComingSoon_V13 DEFAULT(0) WITH VALUES;
                END;

                IF COL_LENGTH('PlatformPlans', 'IsAvailableForPurchase') IS NULL
                BEGIN
                    ALTER TABLE PlatformPlans ADD IsAvailableForPurchase bit NOT NULL CONSTRAINT DF_PlatformPlans_IsAvailableForPurchase_V13 DEFAULT(1) WITH VALUES;
                END;

                IF COL_LENGTH('PlatformPlans', 'UpdatedAt') IS NULL
                BEGIN
                    ALTER TABLE PlatformPlans ADD UpdatedAt datetime2 NOT NULL CONSTRAINT DF_PlatformPlans_UpdatedAt_V13 DEFAULT(SYSUTCDATETIME()) WITH VALUES;
                END;

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
                IF OBJECT_ID('PlatformPlanPrices', 'U') IS NULL
                BEGIN
                    CREATE TABLE PlatformPlanPrices (
                        Id uniqueidentifier NOT NULL CONSTRAINT PK_PlatformPlanPrices_V13 PRIMARY KEY DEFAULT NEWID(),
                        PlatformPlanId uniqueidentifier NOT NULL,
                        BillingCycle int NOT NULL,
                        Price decimal(18,2) NOT NULL CONSTRAINT DF_PlatformPlanPrices_Price_V13 DEFAULT(0),
                        Active bit NOT NULL CONSTRAINT DF_PlatformPlanPrices_Active_V13 DEFAULT(1),
                        CreatedAt datetime2 NOT NULL CONSTRAINT DF_PlatformPlanPrices_CreatedAt_V13 DEFAULT(SYSUTCDATETIME()),
                        UpdatedAt datetime2 NOT NULL CONSTRAINT DF_PlatformPlanPrices_UpdatedAt_V13 DEFAULT(SYSUTCDATETIME())
                    );
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('PlatformPlanPrices', 'PlatformPlanId') IS NULL
                    ALTER TABLE PlatformPlanPrices ADD PlatformPlanId uniqueidentifier NULL;
                IF COL_LENGTH('PlatformPlanPrices', 'BillingCycle') IS NULL
                    ALTER TABLE PlatformPlanPrices ADD BillingCycle int NULL;
                IF COL_LENGTH('PlatformPlanPrices', 'Price') IS NULL
                    ALTER TABLE PlatformPlanPrices ADD Price decimal(18,2) NOT NULL CONSTRAINT DF_PlatformPlanPrices_PriceMissing_V13 DEFAULT(0) WITH VALUES;
                IF COL_LENGTH('PlatformPlanPrices', 'Active') IS NULL
                    ALTER TABLE PlatformPlanPrices ADD Active bit NOT NULL CONSTRAINT DF_PlatformPlanPrices_ActiveMissing_V13 DEFAULT(1) WITH VALUES;
                IF COL_LENGTH('PlatformPlanPrices', 'CreatedAt') IS NULL
                    ALTER TABLE PlatformPlanPrices ADD CreatedAt datetime2 NOT NULL CONSTRAINT DF_PlatformPlanPrices_CreatedAtMissing_V13 DEFAULT(SYSUTCDATETIME()) WITH VALUES;
                IF COL_LENGTH('PlatformPlanPrices', 'UpdatedAt') IS NULL
                    ALTER TABLE PlatformPlanPrices ADD UpdatedAt datetime2 NOT NULL CONSTRAINT DF_PlatformPlanPrices_UpdatedAtMissing_V13 DEFAULT(SYSUTCDATETIME()) WITH VALUES;
                IF COL_LENGTH('PlatformPlanPrices', 'Id') IS NULL
                    ALTER TABLE PlatformPlanPrices ADD Id uniqueidentifier NOT NULL CONSTRAINT DF_PlatformPlanPrices_IdMissing_V13 DEFAULT(NEWID()) WITH VALUES;
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID('PlanBillingOptions', 'U') IS NULL
                BEGIN
                    CREATE TABLE PlanBillingOptions (
                        Id uniqueidentifier NOT NULL CONSTRAINT PK_PlanBillingOptions_V13 PRIMARY KEY DEFAULT NEWID(),
                        PlatformPlanId uniqueidentifier NOT NULL,
                        BillingCycle int NOT NULL,
                        MonthsCount int NOT NULL CONSTRAINT DF_PlanBillingOptions_MonthsCount_V13 DEFAULT(1),
                        MonthlyPrice decimal(18,2) NOT NULL CONSTRAINT DF_PlanBillingOptions_MonthlyPrice_V13 DEFAULT(0),
                        BasePriceInCents int NOT NULL CONSTRAINT DF_PlanBillingOptions_BasePriceInCents_V13 DEFAULT(0),
                        CycleDiscountPercent decimal(5,2) NOT NULL CONSTRAINT DF_PlanBillingOptions_CycleDiscountPercent_V13 DEFAULT(0),
                        CycleDiscountAmountInCents int NOT NULL CONSTRAINT DF_PlanBillingOptions_CycleDiscountAmountInCents_V13 DEFAULT(0),
                        FinalPriceInCents int NOT NULL CONSTRAINT DF_PlanBillingOptions_FinalPriceInCents_V13 DEFAULT(0),
                        IsActive bit NOT NULL CONSTRAINT DF_PlanBillingOptions_IsActive_V13 DEFAULT(1),
                        CreatedAt datetime2 NOT NULL CONSTRAINT DF_PlanBillingOptions_CreatedAt_V13 DEFAULT(SYSUTCDATETIME()),
                        UpdatedAt datetime2 NOT NULL CONSTRAINT DF_PlanBillingOptions_UpdatedAt_V13 DEFAULT(SYSUTCDATETIME())
                    );
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('PlanBillingOptions', 'PlatformPlanId') IS NULL
                    ALTER TABLE PlanBillingOptions ADD PlatformPlanId uniqueidentifier NULL;
                IF COL_LENGTH('PlanBillingOptions', 'BillingCycle') IS NULL
                    ALTER TABLE PlanBillingOptions ADD BillingCycle int NULL;
                IF COL_LENGTH('PlanBillingOptions', 'MonthsCount') IS NULL
                    ALTER TABLE PlanBillingOptions ADD MonthsCount int NOT NULL CONSTRAINT DF_PlanBillingOptions_MonthsCountMissing_V13 DEFAULT(1) WITH VALUES;
                IF COL_LENGTH('PlanBillingOptions', 'MonthlyPrice') IS NULL
                    ALTER TABLE PlanBillingOptions ADD MonthlyPrice decimal(18,2) NOT NULL CONSTRAINT DF_PlanBillingOptions_MonthlyPriceMissing_V13 DEFAULT(0) WITH VALUES;
                IF COL_LENGTH('PlanBillingOptions', 'BasePriceInCents') IS NULL
                    ALTER TABLE PlanBillingOptions ADD BasePriceInCents int NOT NULL CONSTRAINT DF_PlanBillingOptions_BasePriceInCentsMissing_V13 DEFAULT(0) WITH VALUES;
                IF COL_LENGTH('PlanBillingOptions', 'CycleDiscountPercent') IS NULL
                    ALTER TABLE PlanBillingOptions ADD CycleDiscountPercent decimal(5,2) NOT NULL CONSTRAINT DF_PlanBillingOptions_CycleDiscountPercentMissing_V13 DEFAULT(0) WITH VALUES;
                IF COL_LENGTH('PlanBillingOptions', 'CycleDiscountAmountInCents') IS NULL
                    ALTER TABLE PlanBillingOptions ADD CycleDiscountAmountInCents int NOT NULL CONSTRAINT DF_PlanBillingOptions_CycleDiscountAmountInCentsMissing_V13 DEFAULT(0) WITH VALUES;
                IF COL_LENGTH('PlanBillingOptions', 'FinalPriceInCents') IS NULL
                    ALTER TABLE PlanBillingOptions ADD FinalPriceInCents int NOT NULL CONSTRAINT DF_PlanBillingOptions_FinalPriceInCentsMissing_V13 DEFAULT(0) WITH VALUES;
                IF COL_LENGTH('PlanBillingOptions', 'IsActive') IS NULL
                    ALTER TABLE PlanBillingOptions ADD IsActive bit NOT NULL CONSTRAINT DF_PlanBillingOptions_IsActiveMissing_V13 DEFAULT(1) WITH VALUES;
                IF COL_LENGTH('PlanBillingOptions', 'CreatedAt') IS NULL
                    ALTER TABLE PlanBillingOptions ADD CreatedAt datetime2 NOT NULL CONSTRAINT DF_PlanBillingOptions_CreatedAtMissing_V13 DEFAULT(SYSUTCDATETIME()) WITH VALUES;
                IF COL_LENGTH('PlanBillingOptions', 'UpdatedAt') IS NULL
                    ALTER TABLE PlanBillingOptions ADD UpdatedAt datetime2 NOT NULL CONSTRAINT DF_PlanBillingOptions_UpdatedAtMissing_V13 DEFAULT(SYSUTCDATETIME()) WITH VALUES;
                IF COL_LENGTH('PlanBillingOptions', 'Id') IS NULL
                    ALTER TABLE PlanBillingOptions ADD Id uniqueidentifier NOT NULL CONSTRAINT DF_PlanBillingOptions_IdMissing_V13 DEFAULT(NEWID()) WITH VALUES;
                """);

            migrationBuilder.Sql(
                """
                UPDATE PlanBillingOptions
                SET MonthsCount = CASE
                    WHEN BillingCycle = 1 THEN 1
                    WHEN BillingCycle = 2 THEN 3
                    WHEN BillingCycle = 3 THEN 6
                    WHEN BillingCycle = 4 THEN 12
                    ELSE 1
                END
                WHERE MonthsCount IS NULL OR MonthsCount <= 0;
                """);

            migrationBuilder.Sql(
                """
                ;WITH DedupPlanPrices AS (
                    SELECT
                        Id,
                        ROW_NUMBER() OVER (
                            PARTITION BY PlatformPlanId, BillingCycle
                            ORDER BY ISNULL(UpdatedAt, CreatedAt) DESC, CreatedAt DESC, Id DESC
                        ) AS rn
                    FROM PlatformPlanPrices
                    WHERE PlatformPlanId IS NOT NULL AND BillingCycle IS NOT NULL
                )
                DELETE FROM PlatformPlanPrices
                WHERE Id IN (SELECT Id FROM DedupPlanPrices WHERE rn > 1);

                ;WITH DedupBillingOptions AS (
                    SELECT
                        Id,
                        ROW_NUMBER() OVER (
                            PARTITION BY PlatformPlanId, BillingCycle
                            ORDER BY ISNULL(UpdatedAt, CreatedAt) DESC, CreatedAt DESC, Id DESC
                        ) AS rn
                    FROM PlanBillingOptions
                    WHERE PlatformPlanId IS NOT NULL AND BillingCycle IS NOT NULL
                )
                DELETE FROM PlanBillingOptions
                WHERE Id IN (SELECT Id FROM DedupBillingOptions WHERE rn > 1);
                """);

            migrationBuilder.Sql(
                """
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PlatformPlanPrices_PlatformPlanId_BillingCycle' AND object_id = OBJECT_ID('PlatformPlanPrices'))
                    AND NOT EXISTS (
                        SELECT PlatformPlanId, BillingCycle
                        FROM PlatformPlanPrices
                        GROUP BY PlatformPlanId, BillingCycle
                        HAVING COUNT(*) > 1
                    )
                BEGIN
                    CREATE UNIQUE INDEX IX_PlatformPlanPrices_PlatformPlanId_BillingCycle ON PlatformPlanPrices(PlatformPlanId, BillingCycle);
                END;

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PlanBillingOptions_PlatformPlanId_BillingCycle' AND object_id = OBJECT_ID('PlanBillingOptions'))
                    AND NOT EXISTS (
                        SELECT PlatformPlanId, BillingCycle
                        FROM PlanBillingOptions
                        GROUP BY PlatformPlanId, BillingCycle
                        HAVING COUNT(*) > 1
                    )
                BEGIN
                    CREATE UNIQUE INDEX IX_PlanBillingOptions_PlatformPlanId_BillingCycle ON PlanBillingOptions(PlatformPlanId, BillingCycle);
                END;
                """);

            migrationBuilder.Sql(
                """
                EXEC sp_executesql N'
                DECLARE @basicId uniqueidentifier = (
                    SELECT TOP 1 Id FROM PlatformPlans
                    WHERE UPPER(Code) = ''BASIC'' OR UPPER(Name) IN (''STARTER'',''BASIC'')
                    ORDER BY CreatedAt
                );

                IF @basicId IS NOT NULL
                BEGIN
                    DECLARE @cycles TABLE (
                        BillingCycle int NOT NULL,
                        MonthsCount int NOT NULL,
                        Price decimal(18,2) NOT NULL,
                        MonthlyPrice decimal(18,2) NOT NULL,
                        BasePriceInCents int NOT NULL,
                        CycleDiscountPercent decimal(5,2) NOT NULL,
                        CycleDiscountAmountInCents int NOT NULL,
                        FinalPriceInCents int NOT NULL
                    );

                    INSERT INTO @cycles (BillingCycle, MonthsCount, Price, MonthlyPrice, BasePriceInCents, CycleDiscountPercent, CycleDiscountAmountInCents, FinalPriceInCents)
                    VALUES
                        (1, 1,  59.90, 59.90,  5990,  0.00,     0,  5990),
                        (2, 3, 161.73, 59.90, 17970, 10.00,  1797, 16173),
                        (3, 6, 305.49, 59.90, 35940, 15.00,  5391, 30549),
                        (4, 12, 575.04, 59.90, 71880, 20.00, 14376, 57504);

                    MERGE PlatformPlanPrices AS target
                    USING (
                        SELECT @basicId AS PlatformPlanId, c.BillingCycle, c.Price
                        FROM @cycles c
                    ) AS source
                    ON target.PlatformPlanId = source.PlatformPlanId AND target.BillingCycle = source.BillingCycle
                    WHEN MATCHED THEN
                        UPDATE SET
                            target.Price = source.Price,
                            target.Active = 1,
                            target.UpdatedAt = SYSUTCDATETIME()
                    WHEN NOT MATCHED BY TARGET THEN
                        INSERT (Id, PlatformPlanId, BillingCycle, Price, Active, CreatedAt, UpdatedAt)
                        VALUES (NEWID(), source.PlatformPlanId, source.BillingCycle, source.Price, 1, SYSUTCDATETIME(), SYSUTCDATETIME());

                    MERGE PlanBillingOptions AS target
                    USING (
                        SELECT
                            @basicId AS PlatformPlanId,
                            c.BillingCycle,
                            c.MonthsCount,
                            c.MonthlyPrice,
                            c.BasePriceInCents,
                            c.CycleDiscountPercent,
                            c.CycleDiscountAmountInCents,
                            c.FinalPriceInCents
                        FROM @cycles c
                    ) AS source
                    ON target.PlatformPlanId = source.PlatformPlanId AND target.BillingCycle = source.BillingCycle
                    WHEN MATCHED THEN
                        UPDATE SET
                            target.MonthsCount = source.MonthsCount,
                            target.MonthlyPrice = source.MonthlyPrice,
                            target.BasePriceInCents = source.BasePriceInCents,
                            target.CycleDiscountPercent = source.CycleDiscountPercent,
                            target.CycleDiscountAmountInCents = source.CycleDiscountAmountInCents,
                            target.FinalPriceInCents = source.FinalPriceInCents,
                            target.IsActive = 1,
                            target.UpdatedAt = SYSUTCDATETIME()
                    WHEN NOT MATCHED BY TARGET THEN
                        INSERT (
                            Id, PlatformPlanId, BillingCycle, MonthsCount, MonthlyPrice, BasePriceInCents,
                            CycleDiscountPercent, CycleDiscountAmountInCents, FinalPriceInCents,
                            IsActive, CreatedAt, UpdatedAt
                        )
                        VALUES (
                            NEWID(), source.PlatformPlanId, source.BillingCycle, source.MonthsCount, source.MonthlyPrice, source.BasePriceInCents,
                            source.CycleDiscountPercent, source.CycleDiscountAmountInCents, source.FinalPriceInCents,
                            1, SYSUTCDATETIME(), SYSUTCDATETIME()
                        );
                END';
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
