using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Onboarding;
using FitPlatform.Application.DTOs.Subscription;
using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Infrastructure.Services;

public class OnboardingService
{
    private readonly AppDbContext _db;
    private readonly PasswordSetupService _passwordSetup;
    private readonly PaymentService _paymentService;
    private readonly PrivacyLgpdService _privacyLgpdService;

    public OnboardingService(AppDbContext db, PasswordSetupService passwordSetup, PaymentService paymentService, PrivacyLgpdService privacyLgpdService)
    {
        _db = db;
        _passwordSetup = passwordSetup;
        _paymentService = paymentService;
        _privacyLgpdService = privacyLgpdService;
    }

    public async Task<ApiResponse<TrainerOnboardingResponse>> StartAsync(StartOnboardingRequest request)
    {
        if (!request.AcceptPrivacyPolicy || !request.AcceptTermsOfUse)
            return ApiResponse<TrainerOnboardingResponse>.Fail("Voce precisa aceitar os Termos de Uso e a Politica de Privacidade.");
        var email = request.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == email))
            return ApiResponse<TrainerOnboardingResponse>.Fail("Este e-mail já está cadastrado.");
        if (await _db.TrainerOnboardings.AnyAsync(o => o.Email == email && o.Status != OnboardingStatus.Canceled))
            return ApiResponse<TrainerOnboardingResponse>.Fail("Já existe um cadastro em andamento para este e-mail.");

        var onboarding = new TrainerOnboarding
        {
            FullName = request.FullName,
            Email = email,
            Phone = request.Phone,
            CPF = request.CPF,
            BirthDate = request.BirthDate,
            Status = OnboardingStatus.Draft
        };
        _db.TrainerOnboardings.Add(onboarding);
        await _db.SaveChangesAsync();
        await _privacyLgpdService.RegisterAcceptanceAsync(new()
        {
            AcceptPrivacyPolicy = true,
            AcceptTermsOfUse = true,
            Email = email,
            Source = "TrainerOnboarding"
        }, null, null, null);
        return ApiResponse<TrainerOnboardingResponse>.Ok(MapResponse(onboarding));
    }

    public async Task<ApiResponse<TrainerOnboardingResponse>> GetAsync(Guid id)
    {
        var onboarding = await _db.TrainerOnboardings
            .Include(x => x.SelectedPlan)
            .Include(x => x.SelectedPlanPrice)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (onboarding == null) return ApiResponse<TrainerOnboardingResponse>.Fail("Onboarding não encontrado.");
        return ApiResponse<TrainerOnboardingResponse>.Ok(MapResponse(onboarding));
    }

    public async Task<ApiResponse<TrainerOnboardingResponse>> UpdateProfessionalDataAsync(Guid id, UpdateProfessionalDataRequest request)
    {
        var onboarding = await _db.TrainerOnboardings.FindAsync(id);
        if (onboarding == null) return ApiResponse<TrainerOnboardingResponse>.Fail("Onboarding não encontrado.");

        if (!string.IsNullOrWhiteSpace(request.ProfilePhotoUrl) || !string.IsNullOrWhiteSpace(request.LogoUrl))
            return ApiResponse<TrainerOnboardingResponse>.Fail("Envio por URL não é permitido. Use upload de mídia e informe os MediaIds.");

        if (request.ProfilePhotoMediaId.HasValue)
        {
            var profileMedia = await _db.MediaFiles.FirstOrDefaultAsync(m => m.Id == request.ProfilePhotoMediaId.Value);
            if (profileMedia == null) return ApiResponse<TrainerOnboardingResponse>.Fail("Mídia de foto de perfil não encontrada.");
            onboarding.ProfilePhotoUrl = profileMedia.SecureUrl ?? profileMedia.Url;
        }
        if (request.LogoMediaId.HasValue)
        {
            var logoMedia = await _db.MediaFiles.FirstOrDefaultAsync(m => m.Id == request.LogoMediaId.Value);
            if (logoMedia == null) return ApiResponse<TrainerOnboardingResponse>.Fail("Mídia de logo não encontrada.");
            onboarding.LogoUrl = logoMedia.SecureUrl ?? logoMedia.Url;
        }

        onboarding.BrandName = request.BrandName;
        onboarding.CREF = request.CREF;
        onboarding.Bio = request.Bio;
        onboarding.Specialties = request.Specialties;
        onboarding.Instagram = request.Instagram;
        onboarding.PrimaryColor = request.PrimaryColor;
        onboarding.SecondaryColor = request.SecondaryColor;
        onboarding.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ApiResponse<TrainerOnboardingResponse>.Ok(MapResponse(onboarding));
    }

    public async Task<ApiResponse<TrainerOnboardingResponse>> UpdateAddressAsync(Guid id, UpdateAddressRequest request)
    {
        var onboarding = await _db.TrainerOnboardings.FindAsync(id);
        if (onboarding == null) return ApiResponse<TrainerOnboardingResponse>.Fail("Onboarding não encontrado.");

        onboarding.ZipCode = request.ZipCode;
        onboarding.Street = request.Street;
        onboarding.AddressNumber = request.AddressNumber;
        onboarding.Complement = request.Complement;
        onboarding.Neighborhood = request.Neighborhood;
        onboarding.City = request.City;
        onboarding.State = request.State;
        onboarding.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ApiResponse<TrainerOnboardingResponse>.Ok(MapResponse(onboarding));
    }

    public async Task<ApiResponse<TrainerOnboardingResponse>> SelectPlanAsync(Guid id, SelectPlanRequest request)
    {
        if (!IsSupportedBillingCycle(request.BillingCycle))
            return ApiResponse<TrainerOnboardingResponse>.Fail("Ciclo de cobrança inválido. Escolha mensal, semestral ou anual.");

        var onboarding = await _db.TrainerOnboardings.FindAsync(id);
        if (onboarding == null) return ApiResponse<TrainerOnboardingResponse>.Fail("Onboarding não encontrado.");

        var price = await _db.PlatformPlanPrices
            .Include(p => p.PlatformPlan)
            .FirstOrDefaultAsync(p => p.Id == request.PlatformPlanPriceId && p.PlatformPlanId == request.PlatformPlanId && p.Active);
        if (price == null) return ApiResponse<TrainerOnboardingResponse>.Fail("Plano ou preço não encontrado.");
        if (!price.PlatformPlan.IsAvailableForPurchase)
            return ApiResponse<TrainerOnboardingResponse>.Fail("Este plano ainda não está disponível para contratação.");

        onboarding.SelectedPlatformPlanId = request.PlatformPlanId;
        onboarding.SelectedPlatformPlanPriceId = request.PlatformPlanPriceId;
        onboarding.BillingCycle = request.BillingCycle;
        onboarding.Status = OnboardingStatus.WaitingPayment;
        onboarding.UpdatedAt = DateTime.UtcNow;
        onboarding.SelectedPlan = price.PlatformPlan;
        onboarding.SelectedPlanPrice = price;
        await _db.SaveChangesAsync();
        return ApiResponse<TrainerOnboardingResponse>.Ok(MapResponse(onboarding));
    }

    public async Task<ApiResponse<OnboardingPricingPreviewResponse>> ValidateCouponPreviewAsync(Guid onboardingId, string? couponCode, CancellationToken cancellationToken = default)
    {
        var onboarding = await _db.TrainerOnboardings.FirstOrDefaultAsync(x => x.Id == onboardingId, cancellationToken);
        if (onboarding == null) return ApiResponse<OnboardingPricingPreviewResponse>.Fail("Onboarding não encontrado.");
        if (!onboarding.SelectedPlatformPlanId.HasValue || !onboarding.BillingCycle.HasValue)
            return ApiResponse<OnboardingPricingPreviewResponse>.Fail("Selecione plano e ciclo antes de validar cupom.");

        var trainerId = onboarding.CreatedTrainerId ?? Guid.Empty;
        var result = await _paymentService.ValidateCouponAsync(trainerId, new ValidateCouponRequest
        {
            PlanId = onboarding.SelectedPlatformPlanId.Value,
            BillingCycle = onboarding.BillingCycle.Value,
            CouponCode = couponCode
        }, cancellationToken);
        if (!result.Success || result.Data == null) return ApiResponse<OnboardingPricingPreviewResponse>.Fail(result.Message ?? "Erro ao validar cupom.");

        var option = await _db.PlanBillingOptions.FirstOrDefaultAsync(
            x => x.PlatformPlanId == onboarding.SelectedPlatformPlanId.Value
                 && x.BillingCycle == onboarding.BillingCycle.Value
                 && x.IsActive,
            cancellationToken);

        var isValid = result.Data.IsValid;
        var message = result.Data.Message;
        if (isValid && option != null && result.Data.FinalAmountInCents != option.FinalPriceInCents)
        {
            isValid = false;
            message = "Este cupom não é suportado para cobrança recorrente neste plano/ciclo com a configuração atual da AbacatePay.";
        }

        return ApiResponse<OnboardingPricingPreviewResponse>.Ok(new OnboardingPricingPreviewResponse
        {
            IsValid = isValid,
            Message = message,
            CouponCode = result.Data.CouponCode,
            CycleBaseAmountInCents = result.Data.CycleBaseAmountInCents,
            CycleDiscountAmountInCents = result.Data.CycleDiscountAmountInCents,
            SubtotalAfterCycleDiscountInCents = result.Data.SubtotalAfterCycleDiscountInCents,
            CouponDiscountAmountInCents = result.Data.CouponDiscountAmountInCents,
            FinalAmountInCents = result.Data.FinalAmountInCents
        });
    }

    public async Task<ApiResponse<OnboardingPaymentStatusResponse>> GetPaymentStatusAsync(Guid onboardingId, CancellationToken cancellationToken = default)
    {
        var onboarding = await _db.TrainerOnboardings.FirstOrDefaultAsync(x => x.Id == onboardingId, cancellationToken);
        if (onboarding == null) return ApiResponse<OnboardingPaymentStatusResponse>.Fail("Onboarding não encontrado.");

        var subscription = await _db.TrainerSubscriptions
            .Where(x => x.TrainerOnboardingId == onboardingId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        var payment = subscription == null
            ? null
            : await _db.TrainerPayments
                .Where(x => x.TrainerSubscriptionId == subscription.Id)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

        return ApiResponse<OnboardingPaymentStatusResponse>.Ok(new OnboardingPaymentStatusResponse
        {
            OnboardingId = onboardingId,
            OnboardingStatus = onboarding.Status.ToString(),
            SubscriptionStatus = subscription?.Status.ToString(),
            PaymentStatus = payment?.Status.ToString(),
            CheckoutUrl = subscription?.InitPoint,
            IsPaymentConfirmed = subscription?.Status == TrainerSubscriptionStatus.Active
        });
    }

    public async Task<ApiResponse<SubscriptionResponse>> CreateCheckoutAsync(Guid onboardingId, CreateOnboardingCheckoutRequest request, CancellationToken cancellationToken = default)
    {
        var onboarding = await _db.TrainerOnboardings.FirstOrDefaultAsync(x => x.Id == onboardingId, cancellationToken);
        if (onboarding == null) return ApiResponse<SubscriptionResponse>.Fail("Onboarding não encontrado.");
        if (!onboarding.SelectedPlatformPlanId.HasValue || !onboarding.BillingCycle.HasValue)
            return ApiResponse<SubscriptionResponse>.Fail("Selecione um plano/ciclo antes do checkout.");
        if (onboarding.Status == OnboardingStatus.Completed)
            return ApiResponse<SubscriptionResponse>.Fail("Onboarding já concluído.");

        var selectedPlan = await _db.PlatformPlans.FirstOrDefaultAsync(
            x => x.Id == onboarding.SelectedPlatformPlanId.Value && x.Active,
            cancellationToken);
        if (selectedPlan == null)
            return ApiResponse<SubscriptionResponse>.Fail("Plano não encontrado.");
        if (!selectedPlan.IsAvailableForPurchase)
            return ApiResponse<SubscriptionResponse>.Fail("Este plano ainda não está disponível para contratação.");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == onboarding.Email, cancellationToken);
        if (user == null)
        {
            user = new User
            {
                Name = onboarding.FullName,
                Email = onboarding.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                Role = UserRole.Trainer,
                IsActive = false,
                MustChangePassword = true
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync(cancellationToken);
        }
        else
        {
            user.IsActive = false;
            user.MustChangePassword = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }

        var trainer = await _db.Trainers.FirstOrDefaultAsync(t => t.UserId == user.Id, cancellationToken);
        if (trainer == null)
        {
            trainer = new Trainer
            {
                UserId = user.Id,
                BrandName = onboarding.BrandName ?? onboarding.FullName,
                Phone = onboarding.Phone,
                Bio = onboarding.Bio,
                CPF = onboarding.CPF,
                BirthDate = onboarding.BirthDate,
                CREF = onboarding.CREF,
                Specialties = onboarding.Specialties,
                Instagram = onboarding.Instagram,
                ProfilePhotoUrl = onboarding.ProfilePhotoUrl,
                LogoUrl = onboarding.LogoUrl,
                PrimaryColor = onboarding.PrimaryColor,
                SecondaryColor = onboarding.SecondaryColor,
                ZipCode = onboarding.ZipCode,
                Street = onboarding.Street,
                AddressNumber = onboarding.AddressNumber,
                Complement = onboarding.Complement,
                Neighborhood = onboarding.Neighborhood,
                City = onboarding.City,
                State = onboarding.State,
                PublicPageEnabled = false,
                PublicSearchEnabled = false,
                AcceptingStudents = false
            };
            _db.Trainers.Add(trainer);
            await _db.SaveChangesAsync(cancellationToken);
        }

        onboarding.CreatedUserId = user.Id;
        onboarding.CreatedTrainerId = trainer.Id;
        onboarding.Status = OnboardingStatus.WaitingPayment;
        onboarding.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return await _paymentService.CreateSubscriptionAsync(trainer.Id, new CreateSubscriptionRequest
        {
            PlatformPlanId = onboarding.SelectedPlatformPlanId.Value,
            PlatformPlanPriceId = onboarding.SelectedPlatformPlanPriceId,
            BillingCycle = onboarding.BillingCycle.Value,
            CouponCode = request.CouponCode
        }, cancellationToken);
    }

    public async Task<ApiResponse<TrainerOnboardingResponse>> SimulatePaymentApprovedAsync(Guid id)
    {
        var onboarding = await _db.TrainerOnboardings
            .Include(x => x.SelectedPlan)
            .Include(x => x.SelectedPlanPrice)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (onboarding == null) return ApiResponse<TrainerOnboardingResponse>.Fail("Onboarding não encontrado.");
        if (onboarding.SelectedPlatformPlanId == null) return ApiResponse<TrainerOnboardingResponse>.Fail("Selecione um plano antes de prosseguir.");
        if (await _db.Users.AnyAsync(u => u.Email == onboarding.Email))
            return ApiResponse<TrainerOnboardingResponse>.Fail("Este e-mail já está em uso.");

        var user = new User
        {
            Name = onboarding.FullName,
            Email = onboarding.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
            Role = UserRole.Trainer,
            IsActive = true,
            MustChangePassword = true
        };
        _db.Users.Add(user);

        var trainer = new Trainer { UserId = user.Id, BrandName = onboarding.BrandName ?? onboarding.FullName, Phone = onboarding.Phone };
        _db.Trainers.Add(trainer);

        var billingCycle = onboarding.BillingCycle ?? BillingFrequency.Monthly;
        var endDate = billingCycle switch
        {
            BillingFrequency.Semiannual => DateTime.UtcNow.AddMonths(6),
            BillingFrequency.Yearly => DateTime.UtcNow.AddYears(1),
            _ => DateTime.UtcNow.AddMonths(1)
        };

        var subscription = new TrainerSubscription
        {
            TrainerId = trainer.Id,
            TrainerOnboardingId = onboarding.Id,
            PlatformPlanId = onboarding.SelectedPlatformPlanId.Value,
            PlatformPlanPriceId = onboarding.SelectedPlatformPlanPriceId,
            BillingCycle = billingCycle,
            Status = TrainerSubscriptionStatus.Active,
            StartDate = DateTime.UtcNow,
            EndDate = endDate
        };
        _db.TrainerSubscriptions.Add(subscription);

        _db.TrainerPayments.Add(new TrainerPayment
        {
            TrainerId = trainer.Id,
            TrainerSubscriptionId = subscription.Id,
            Amount = onboarding.SelectedPlanPrice?.Price ?? 0,
            Status = PaymentStatus.Approved,
            Provider = "Simulation",
            ProviderPaymentId = $"SIM-{Guid.NewGuid():N}",
            PaidAt = DateTime.UtcNow
        });

        onboarding.Status = OnboardingStatus.Completed;
        onboarding.CreatedUserId = user.Id;
        onboarding.CreatedTrainerId = trainer.Id;
        onboarding.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await _passwordSetup.GenerateAndSendSetupTokenAsync(user, onboarding.SelectedPlan?.Name ?? "FitPlatform", isStudent: false);
        return ApiResponse<TrainerOnboardingResponse>.Ok(MapResponse(onboarding), "Conta criada com sucesso! Verifique o console para o link de definição de senha.");
    }

    private static TrainerOnboardingResponse MapResponse(TrainerOnboarding o) => new()
    {
        Id = o.Id,
        FullName = o.FullName,
        Email = o.Email,
        Phone = o.Phone,
        CPF = o.CPF,
        BirthDate = o.BirthDate,
        BrandName = o.BrandName,
        CREF = o.CREF,
        Bio = o.Bio,
        Specialties = o.Specialties,
        Instagram = o.Instagram,
        ProfilePhotoUrl = o.ProfilePhotoUrl,
        LogoUrl = o.LogoUrl,
        PrimaryColor = o.PrimaryColor,
        SecondaryColor = o.SecondaryColor,
        ZipCode = o.ZipCode,
        Street = o.Street,
        AddressNumber = o.AddressNumber,
        Complement = o.Complement,
        Neighborhood = o.Neighborhood,
        City = o.City,
        State = o.State,
        SelectedPlatformPlanId = o.SelectedPlatformPlanId,
        SelectedPlanName = o.SelectedPlan?.Name,
        SelectedPlatformPlanPriceId = o.SelectedPlatformPlanPriceId,
        BillingCycle = o.BillingCycle?.ToString(),
        SelectedPrice = o.SelectedPlanPrice?.Price,
        Status = o.Status.ToString(),
        CreatedAt = o.CreatedAt
    };

    private static bool IsSupportedBillingCycle(BillingFrequency cycle) =>
        cycle is BillingFrequency.Monthly or BillingFrequency.Semiannual or BillingFrequency.Yearly;
}
