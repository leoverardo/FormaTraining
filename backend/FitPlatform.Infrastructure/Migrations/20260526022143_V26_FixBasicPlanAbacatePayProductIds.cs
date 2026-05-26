using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitPlatform.Infrastructure.Migrations
{
    /// <summary>
    /// Corrige AbacatePayProductId nulo para o plano Basic (antigo Starter) em todos os ciclos.
    ///
    /// Causa raiz: V20_SetAbacatePayProductIds (14/05) rodou quando PlanBillingOptions
    /// ainda estava vazia — a tabela só foi populada por V13_PlansCommercialStrategyBasicOnly
    /// (22/05). Por isso o UPDATE de V20 não afetou nenhuma linha, deixando
    /// AbacatePayProductId = NULL para o plano Basic.
    ///
    /// Produto IDs (AbacatePay):
    ///   Monthly    (1) — prod_QTnE1M6UHFwqhxpTJhHahrxB  (R$ 59,90 / mês)
    ///   Semiannual (3) — prod_26kYBznyFUgmSYUNZ25dY6kj  (R$ 305,49 / semestre)
    ///   Yearly     (4) — prod_bn3nSLyu2mfwePQj4sAu3kxu  (R$ 575,04 / ano)
    ///
    /// ⚠️  Verifique no painel AbacatePay se esses produto IDs correspondem
    ///     ao preço correto antes de aplicar em produção.
    /// </summary>
    public partial class V26_FixBasicPlanAbacatePayProductIds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE pbo
SET pbo.AbacatePayProductId = 'prod_QTnE1M6UHFwqhxpTJhHahrxB',
    pbo.UpdatedAt = SYSUTCDATETIME()
FROM PlanBillingOptions pbo
INNER JOIN PlatformPlans pp ON pp.Id = pbo.PlatformPlanId
WHERE (UPPER(pp.Code) = 'BASIC' OR UPPER(pp.Name) IN ('BASIC','STARTER'))
  AND pbo.BillingCycle = 1
  AND (pbo.AbacatePayProductId IS NULL OR pbo.AbacatePayProductId = '');

UPDATE pbo
SET pbo.AbacatePayProductId = 'prod_26kYBznyFUgmSYUNZ25dY6kj',
    pbo.UpdatedAt = SYSUTCDATETIME()
FROM PlanBillingOptions pbo
INNER JOIN PlatformPlans pp ON pp.Id = pbo.PlatformPlanId
WHERE (UPPER(pp.Code) = 'BASIC' OR UPPER(pp.Name) IN ('BASIC','STARTER'))
  AND pbo.BillingCycle = 3
  AND (pbo.AbacatePayProductId IS NULL OR pbo.AbacatePayProductId = '');

UPDATE pbo
SET pbo.AbacatePayProductId = 'prod_bn3nSLyu2mfwePQj4sAu3kxu',
    pbo.UpdatedAt = SYSUTCDATETIME()
FROM PlanBillingOptions pbo
INNER JOIN PlatformPlans pp ON pp.Id = pbo.PlatformPlanId
WHERE (UPPER(pp.Code) = 'BASIC' OR UPPER(pp.Name) IN ('BASIC','STARTER'))
  AND pbo.BillingCycle = 4
  AND (pbo.AbacatePayProductId IS NULL OR pbo.AbacatePayProductId = '');
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE pbo
SET pbo.AbacatePayProductId = NULL,
    pbo.UpdatedAt = SYSUTCDATETIME()
FROM PlanBillingOptions pbo
INNER JOIN PlatformPlans pp ON pp.Id = pbo.PlatformPlanId
WHERE (UPPER(pp.Code) = 'BASIC' OR UPPER(pp.Name) IN ('BASIC','STARTER'))
  AND pbo.BillingCycle IN (1, 3, 4)
  AND pbo.AbacatePayProductId IN (
    'prod_QTnE1M6UHFwqhxpTJhHahrxB',
    'prod_26kYBznyFUgmSYUNZ25dY6kj',
    'prod_bn3nSLyu2mfwePQj4sAu3kxu'
  );
");
        }
    }
}
