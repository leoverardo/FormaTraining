using FitPlatform.Application.DTOs.Subscription;
using FitPlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Tests.Payments;

/// <summary>
/// Testes de cupons, cancelamento e troca de plano no <c>PaymentService</c>.
/// Verificam também que o backend é a fonte da verdade de preço e que nenhuma
/// operação alcança a assinatura de outro trainer.
/// </summary>
public class SubscriptionManagementTests : PaymentsTestHarness
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Cupons
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Cupom_valido_reduz_o_valor_e_e_enviado_ao_provedor()
    {
        var provider = new FakePaymentProvider();
        var service = BuildPaymentService(provider);

        var trainer = await CreateTrainerAsync();
        var plan = await CreatePlanAsync("Basic", 59.90m, "prod_basic_monthly");
        var coupon = await CreateCouponAsync("PROMO20", DiscountType.Percentage, 20m);

        var result = await service.CreateSubscriptionAsync(trainer.Id, new CreateSubscriptionRequest
        {
            PlatformPlanId = plan.Id,
            BillingCycle = BillingFrequency.Monthly,
            CouponCode = "PROMO20"
        });

        Assert.True(result.Success, result.Message);

        // R$ 59,90 com 20% de desconto = R$ 47,92.
        var subscription = await Db.TrainerSubscriptions.FirstAsync(x => x.TrainerId == trainer.Id);
        Assert.Equal(5990, subscription.BaseAmountInCents);
        Assert.Equal(1198, subscription.CouponDiscountAmountInCents);
        Assert.Equal(4792, subscription.FinalAmountInCents);
        Assert.Equal("PROMO20", subscription.CouponCodeApplied);

        // O cupom foi espelhado no provedor e enviado no checkout.
        Assert.Single(provider.CouponCalls);
        Assert.Equal("PROMO20", provider.CouponCalls[0].Code);
        Assert.Equal("PERCENTAGE", provider.CouponCalls[0].DiscountKind);
        Assert.Equal(20, provider.CouponCalls[0].Discount);

        Assert.Single(provider.CreateCalls);
        Assert.Equal(["PROMO20"], provider.CreateCalls[0].CouponCodes);

        await Db.Entry(coupon).ReloadAsync();
        Assert.Equal("PROMO20", coupon.ProviderCouponCode);
        Assert.NotNull(coupon.ProviderSyncedAt);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Redirects do checkout
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Checkout_pelo_painel_redireciona_para_a_tela_de_assinatura()
    {
        var provider = new FakePaymentProvider();
        var service = BuildPaymentService(provider);

        var trainer = await CreateTrainerAsync();
        var plan = await CreatePlanAsync("Basic", 59.90m, "prod_basic_monthly");

        var result = await service.CreateSubscriptionAsync(trainer.Id, new CreateSubscriptionRequest
        {
            PlatformPlanId = plan.Id,
            BillingCycle = BillingFrequency.Monthly
        });

        Assert.True(result.Success, result.Message);
        Assert.Equal("https://app.example.test/trainer/subscription?checkout=success", provider.CreateCalls[0].CompletionUrl);
        Assert.Equal("https://app.example.test/trainer/subscription?checkout=canceled", provider.CreateCalls[0].ReturnUrl);
    }

    [Fact]
    public async Task Checkout_de_onboarding_redireciona_com_o_id_do_onboarding()
    {
        var provider = new FakePaymentProvider();
        var service = BuildPaymentService(provider);

        var trainer = await CreateTrainerAsync();
        var email = await Db.Users.Where(u => u.Id == trainer.UserId).Select(u => u.Email).FirstAsync();
        var plan = await CreatePlanAsync("Basic", 59.90m, "prod_basic_monthly");

        var onboarding = new FitPlatform.Domain.Entities.TrainerOnboarding
        {
            FullName = "Trainer Test",
            Email = email,
            Status = OnboardingStatus.WaitingPayment
        };
        Db.TrainerOnboardings.Add(onboarding);
        await Db.SaveChangesAsync();

        var result = await service.CreateSubscriptionAsync(trainer.Id, new CreateSubscriptionRequest
        {
            PlatformPlanId = plan.Id,
            BillingCycle = BillingFrequency.Monthly
        });

        Assert.True(result.Success, result.Message);
        Assert.Equal($"https://app.example.test/onboarding/success?onboardingId={onboarding.Id}", provider.CreateCalls[0].CompletionUrl);
        Assert.Equal($"https://app.example.test/onboarding/return?onboardingId={onboarding.Id}", provider.CreateCalls[0].ReturnUrl);
    }

    [Fact]
    public async Task Cupom_ja_sincronizado_nao_chama_o_provedor_novamente()
    {
        var provider = new FakePaymentProvider();
        var service = BuildPaymentService(provider);

        var plan = await CreatePlanAsync("Basic", 59.90m, "prod_basic_monthly");
        var coupon = await CreateCouponAsync("CACHED10", DiscountType.Percentage, 10m);
        coupon.ProviderCouponCode = "CACHED10";
        coupon.ProviderSyncedAt = DateTime.UtcNow.AddDays(-1);
        await Db.SaveChangesAsync();

        var trainer = await CreateTrainerAsync();
        var result = await service.CreateSubscriptionAsync(trainer.Id, new CreateSubscriptionRequest
        {
            PlatformPlanId = plan.Id,
            BillingCycle = BillingFrequency.Monthly,
            CouponCode = "CACHED10"
        });

        Assert.True(result.Success, result.Message);
        Assert.Empty(provider.CouponCalls);
        Assert.Equal(["CACHED10"], provider.CreateCalls[0].CouponCodes);
    }

    [Theory]
    [InlineData("INEXISTENTE", "Cupom inválido.")]
    public async Task Cupom_inexistente_e_rejeitado(string code, string expectedMessage)
    {
        var service = BuildPaymentService(new FakePaymentProvider());
        var trainer = await CreateTrainerAsync();
        var plan = await CreatePlanAsync();

        var result = await service.CreateSubscriptionAsync(trainer.Id, new CreateSubscriptionRequest
        {
            PlatformPlanId = plan.Id,
            BillingCycle = BillingFrequency.Monthly,
            CouponCode = code
        });

        Assert.False(result.Success);
        Assert.Equal(expectedMessage, result.Message);
    }

    [Fact]
    public async Task Cupom_expirado_e_rejeitado()
    {
        var service = BuildPaymentService(new FakePaymentProvider());
        var trainer = await CreateTrainerAsync();
        var plan = await CreatePlanAsync();
        await CreateCouponAsync("EXPIRADO", expiresAt: DateTime.UtcNow.AddDays(-1));

        var result = await service.CreateSubscriptionAsync(trainer.Id, new CreateSubscriptionRequest
        {
            PlatformPlanId = plan.Id,
            BillingCycle = BillingFrequency.Monthly,
            CouponCode = "EXPIRADO"
        });

        Assert.False(result.Success);
        Assert.Equal("Cupom expirado.", result.Message);
    }

    [Fact]
    public async Task Cupom_esgotado_e_rejeitado()
    {
        var service = BuildPaymentService(new FakePaymentProvider());
        var trainer = await CreateTrainerAsync();
        var plan = await CreatePlanAsync();
        await CreateCouponAsync("ESGOTADO", maxUsesTotal: 5, currentUses: 5);

        var result = await service.CreateSubscriptionAsync(trainer.Id, new CreateSubscriptionRequest
        {
            PlatformPlanId = plan.Id,
            BillingCycle = BillingFrequency.Monthly,
            CouponCode = "ESGOTADO"
        });

        Assert.False(result.Success);
        Assert.Equal("Cupom esgotado.", result.Message);
    }

    [Fact]
    public async Task Cupom_de_outro_plano_e_rejeitado()
    {
        var service = BuildPaymentService(new FakePaymentProvider());
        var trainer = await CreateTrainerAsync();
        var plan = await CreatePlanAsync();
        var outroPlano = await CreatePlanAsync("Pro", 99.90m, "prod_pro");
        await CreateCouponAsync("SOPRO", appliesToPlanId: outroPlano.Id);

        var result = await service.CreateSubscriptionAsync(trainer.Id, new CreateSubscriptionRequest
        {
            PlatformPlanId = plan.Id,
            BillingCycle = BillingFrequency.Monthly,
            CouponCode = "SOPRO"
        });

        Assert.False(result.Success);
        Assert.Equal("Cupom não aplicável a este plano.", result.Message);
    }

    [Fact]
    public async Task Cupom_com_percentual_acima_de_100_e_rejeitado()
    {
        var service = BuildPaymentService(new FakePaymentProvider());
        var trainer = await CreateTrainerAsync();
        var plan = await CreatePlanAsync();
        await CreateCouponAsync("ABSURDO", DiscountType.Percentage, 150m);

        var result = await service.CreateSubscriptionAsync(trainer.Id, new CreateSubscriptionRequest
        {
            PlatformPlanId = plan.Id,
            BillingCycle = BillingFrequency.Monthly,
            CouponCode = "ABSURDO"
        });

        Assert.False(result.Success);
        Assert.Equal("Cupom com desconto inválido.", result.Message);
    }

    [Fact]
    public async Task Cupom_fixo_maior_que_o_valor_nao_gera_preco_negativo()
    {
        var provider = new FakePaymentProvider();
        var service = BuildPaymentService(provider);
        var trainer = await CreateTrainerAsync();
        var plan = await CreatePlanAsync("Basic", 59.90m, "prod_basic_monthly");
        await CreateCouponAsync("MEGA", DiscountType.FixedAmount, 100_000m);

        var result = await service.CreateSubscriptionAsync(trainer.Id, new CreateSubscriptionRequest
        {
            PlatformPlanId = plan.Id,
            BillingCycle = BillingFrequency.Monthly,
            CouponCode = "MEGA"
        });

        Assert.True(result.Success, result.Message);

        var subscription = await Db.TrainerSubscriptions.FirstAsync(x => x.TrainerId == trainer.Id);
        Assert.Equal(0, subscription.FinalAmountInCents);
        Assert.Equal(5990, subscription.CouponDiscountAmountInCents);
    }

    [Fact]
    public async Task Falha_ao_sincronizar_cupom_bloqueia_o_checkout_em_vez_de_cobrar_valor_cheio()
    {
        var provider = new FakePaymentProvider { CouponException = ProviderFailure("coupons/create indisponível") };
        var service = BuildPaymentService(provider);

        var trainer = await CreateTrainerAsync();
        var plan = await CreatePlanAsync("Basic", 59.90m, "prod_basic_monthly");
        await CreateCouponAsync("FALHA20");

        var result = await service.CreateSubscriptionAsync(trainer.Id, new CreateSubscriptionRequest
        {
            PlatformPlanId = plan.Id,
            BillingCycle = BillingFrequency.Monthly,
            CouponCode = "FALHA20"
        });

        Assert.False(result.Success);
        Assert.Empty(provider.CreateCalls);
        Assert.False(await Db.TrainerSubscriptions.AnyAsync(x => x.TrainerId == trainer.Id));
    }

    [Fact]
    public async Task Preco_divergente_do_produto_configurado_bloqueia_o_checkout()
    {
        var provider = new FakePaymentProvider();
        var service = BuildPaymentService(provider);

        var trainer = await CreateTrainerAsync();
        var plan = await CreatePlanAsync("Basic", 59.90m, "prod_basic_monthly");

        // Produto da AbacatePay cadastrado com preço diferente do calculado pelo backend.
        var option = await Db.PlanBillingOptions.FirstAsync(x => x.PlatformPlanId == plan.Id);
        option.FinalPriceInCents = 7990;
        await Db.SaveChangesAsync();

        var result = await service.CreateSubscriptionAsync(trainer.Id, new CreateSubscriptionRequest
        {
            PlatformPlanId = plan.Id,
            BillingCycle = BillingFrequency.Monthly
        });

        Assert.False(result.Success);
        Assert.Empty(provider.CreateCalls);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Falha do provedor no checkout
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Falha_do_provedor_no_checkout_nao_deixa_assinatura_orfa()
    {
        var provider = new FakePaymentProvider { CreateException = ProviderFailure("HTTP 503") };
        var service = BuildPaymentService(provider);

        var trainer = await CreateTrainerAsync();
        var plan = await CreatePlanAsync("Basic", 59.90m, "prod_basic_monthly");

        var result = await service.CreateSubscriptionAsync(trainer.Id, new CreateSubscriptionRequest
        {
            PlatformPlanId = plan.Id,
            BillingCycle = BillingFrequency.Monthly
        });

        Assert.False(result.Success);
        Assert.Contains("provedor de pagamento", result.Message, StringComparison.OrdinalIgnoreCase);

        // Rollback: nada persistido.
        Assert.False(await Db.TrainerSubscriptions.AnyAsync(x => x.TrainerId == trainer.Id));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Cancelamento
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Cancelamento_cancela_no_provedor_e_preserva_acesso_ate_fim_do_periodo()
    {
        var provider = new FakePaymentProvider();
        var service = BuildPaymentService(provider);

        var sub = await CreateActiveSubscriptionAsync("subs_cancel_ok");
        var originalEndDate = sub.EndDate;

        var result = await service.CancelSubscriptionAsync(sub.TrainerId);

        Assert.True(result.Success, result.Message);
        Assert.False(result.Data!.AlreadyCanceled);
        Assert.Equal(originalEndDate, result.Data.AccessValidUntil);
        Assert.Equal(["subs_cancel_ok"], provider.CancelCalls);

        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Canceled, sub.Status);
        Assert.Equal(originalEndDate, sub.EndDate);
    }

    [Fact]
    public async Task Cancelamento_e_idempotente()
    {
        var provider = new FakePaymentProvider();
        var service = BuildPaymentService(provider);
        var sub = await CreateActiveSubscriptionAsync("subs_cancel_idem");

        var first = await service.CancelSubscriptionAsync(sub.TrainerId);
        var second = await service.CancelSubscriptionAsync(sub.TrainerId);

        Assert.True(first.Success);
        Assert.True(second.Success);
        Assert.True(second.Data!.AlreadyCanceled);

        // O provedor não é chamado duas vezes.
        Assert.Single(provider.CancelCalls);
    }

    [Fact]
    public async Task Trainer_nao_consegue_cancelar_assinatura_de_outro_trainer()
    {
        var provider = new FakePaymentProvider();
        var service = BuildPaymentService(provider);

        var dono = await CreateTrainerAsync();
        var intruso = await CreateTrainerAsync();
        var sub = await CreateActiveSubscriptionAsync("subs_do_dono", dono);

        // O serviço só enxerga assinaturas do trainer autenticado.
        var result = await service.CancelSubscriptionAsync(intruso.Id);

        Assert.False(result.Success);
        Assert.Empty(provider.CancelCalls);

        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Active, sub.Status);
    }

    [Fact]
    public async Task Cancelamento_com_falha_do_provedor_nao_altera_estado_local()
    {
        var provider = new FakePaymentProvider { CancelException = ProviderFailure("HTTP 500") };
        var service = BuildPaymentService(provider);
        var sub = await CreateActiveSubscriptionAsync("subs_cancel_fail");

        var result = await service.CancelSubscriptionAsync(sub.TrainerId);

        Assert.False(result.Success);

        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Active, sub.Status);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Troca de plano
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Troca_de_plano_agenda_alteracao_para_o_proximo_ciclo()
    {
        var provider = new FakePaymentProvider();
        var service = BuildPaymentService(provider);

        var trainer = await CreateTrainerAsync();
        var planoAtual = await CreatePlanAsync("Basic", 59.90m, "prod_basic_monthly");
        var novoPlano = await CreatePlanAsync("Pro", 99.90m, "prod_pro_monthly");
        var sub = await CreateActiveSubscriptionAsync("subs_change", trainer, planoAtual);

        var result = await service.ChangePlanAsync(trainer.Id, new ChangeSubscriptionPlanRequest
        {
            PlatformPlanId = novoPlano.Id
        });

        Assert.True(result.Success, result.Message);
        Assert.Equal("Upgrade", result.Data!.ChangeKind);
        Assert.Equal(9990, result.Data.NewAmountInCents);

        // O provedor recebeu o produto resolvido pelo backend.
        Assert.Single(provider.ChangePlanCalls);
        Assert.Equal("subs_change", provider.ChangePlanCalls[0].ProviderSubscriptionId);
        Assert.Equal("prod_pro_monthly", provider.ChangePlanCalls[0].ProductId);

        // O plano vigente ainda é o antigo: a troca só vale no próximo ciclo.
        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(planoAtual.Id, sub.PlatformPlanId);
        Assert.Equal(novoPlano.Id, sub.PendingPlatformPlanId);
        Assert.Equal(9990, sub.PendingFinalAmountInCents);
        Assert.Equal("subu_fake", sub.PendingPlanChangeProviderId);
    }

    [Fact]
    public async Task Troca_de_plano_identifica_downgrade()
    {
        var provider = new FakePaymentProvider();
        var service = BuildPaymentService(provider);

        var trainer = await CreateTrainerAsync();
        var planoAtual = await CreatePlanAsync("Pro", 99.90m, "prod_pro_monthly");
        var novoPlano = await CreatePlanAsync("Basic", 59.90m, "prod_basic_monthly");

        var sub = await CreateActiveSubscriptionAsync("subs_down", trainer, planoAtual);
        sub.FinalAmountInCents = 9990;
        await Db.SaveChangesAsync();

        var result = await service.ChangePlanAsync(trainer.Id, new ChangeSubscriptionPlanRequest
        {
            PlatformPlanId = novoPlano.Id
        });

        Assert.True(result.Success, result.Message);
        Assert.Equal("Downgrade", result.Data!.ChangeKind);
    }

    [Fact]
    public async Task Trainer_nao_consegue_trocar_plano_de_outro_trainer()
    {
        var provider = new FakePaymentProvider();
        var service = BuildPaymentService(provider);

        var dono = await CreateTrainerAsync();
        var intruso = await CreateTrainerAsync();
        var planoAtual = await CreatePlanAsync("Basic", 59.90m, "prod_basic_monthly");
        var novoPlano = await CreatePlanAsync("Pro", 99.90m, "prod_pro_monthly");
        var sub = await CreateActiveSubscriptionAsync("subs_alheia", dono, planoAtual);

        var result = await service.ChangePlanAsync(intruso.Id, new ChangeSubscriptionPlanRequest
        {
            PlatformPlanId = novoPlano.Id
        });

        Assert.False(result.Success);
        Assert.Empty(provider.ChangePlanCalls);

        await Db.Entry(sub).ReloadAsync();
        Assert.Null(sub.PendingPlatformPlanId);
    }

    [Fact]
    public async Task Troca_para_o_mesmo_plano_e_ciclo_e_rejeitada()
    {
        var provider = new FakePaymentProvider();
        var service = BuildPaymentService(provider);

        var trainer = await CreateTrainerAsync();
        var plano = await CreatePlanAsync("Basic", 59.90m, "prod_basic_monthly");
        await CreateActiveSubscriptionAsync("subs_same", trainer, plano);

        var result = await service.ChangePlanAsync(trainer.Id, new ChangeSubscriptionPlanRequest
        {
            PlatformPlanId = plano.Id,
            BillingCycle = BillingFrequency.Monthly
        });

        Assert.False(result.Success);
        Assert.Empty(provider.ChangePlanCalls);
    }

    [Fact]
    public async Task Troca_de_plano_exige_assinatura_confirmada_pelo_provedor()
    {
        var provider = new FakePaymentProvider();
        var service = BuildPaymentService(provider);

        var trainer = await CreateTrainerAsync();
        var planoAtual = await CreatePlanAsync("Basic", 59.90m, "prod_basic_monthly");
        var novoPlano = await CreatePlanAsync("Pro", 99.90m, "prod_pro_monthly");

        // Assinatura ativa localmente mas ainda sem subs_... do provedor.
        var sub = await CreateActiveSubscriptionAsync("subs_temp", trainer, planoAtual);
        sub.AbacatePaySubscriptionId = null;
        await Db.SaveChangesAsync();

        var result = await service.ChangePlanAsync(trainer.Id, new ChangeSubscriptionPlanRequest
        {
            PlatformPlanId = novoPlano.Id
        });

        Assert.False(result.Success);
        Assert.Empty(provider.ChangePlanCalls);
    }

    [Fact]
    public async Task Troca_de_plano_com_falha_do_provedor_nao_agenda_nada()
    {
        var provider = new FakePaymentProvider { ChangePlanException = ProviderFailure("HTTP 502") };
        var service = BuildPaymentService(provider);

        var trainer = await CreateTrainerAsync();
        var planoAtual = await CreatePlanAsync("Basic", 59.90m, "prod_basic_monthly");
        var novoPlano = await CreatePlanAsync("Pro", 99.90m, "prod_pro_monthly");
        var sub = await CreateActiveSubscriptionAsync("subs_change_fail", trainer, planoAtual);

        var result = await service.ChangePlanAsync(trainer.Id, new ChangeSubscriptionPlanRequest
        {
            PlatformPlanId = novoPlano.Id
        });

        Assert.False(result.Success);

        await Db.Entry(sub).ReloadAsync();
        Assert.Null(sub.PendingPlatformPlanId);
    }

    [Fact]
    public async Task Troca_para_plano_sem_produto_configurado_e_rejeitada()
    {
        var provider = new FakePaymentProvider();
        var service = BuildPaymentService(provider);

        var trainer = await CreateTrainerAsync();
        var planoAtual = await CreatePlanAsync("Basic", 59.90m, "prod_basic_monthly");
        var semProduto = await CreatePlanAsync("Enterprise", 199.90m, productId: null);
        await CreateActiveSubscriptionAsync("subs_noproduct", trainer, planoAtual);

        var result = await service.ChangePlanAsync(trainer.Id, new ChangeSubscriptionPlanRequest
        {
            PlatformPlanId = semProduto.Id
        });

        Assert.False(result.Success);
        Assert.Empty(provider.ChangePlanCalls);
    }
}
