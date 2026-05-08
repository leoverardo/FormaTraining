using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Onboarding;
using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Infrastructure.Services;

public class OnboardingService
{
    private readonly AppDbContext _db;
    private readonly PasswordSetupService _passwordSetup;

    public OnboardingService(AppDbContext db, PasswordSetupService passwordSetup)
    {
        _db = db;
        _passwordSetup = passwordSetup;
    }

    public async Task<ApiResponse<TrainerOnboardingResponse>> StartAsync(StartOnboardingRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email.ToLower()))
            return ApiResponse<TrainerOnboardingResponse>.Fail("Este e-mail já está cadastrado.");

        if (await _db.TrainerOnboardings.AnyAsync(o => o.Email == request.Email.ToLower() && o.Status != OnboardingStatus.Canceled))
            return ApiResponse<TrainerOnboardingResponse>.Fail("Já existe um cadastro em andamento para este e-mail.");

        var onboarding = new TrainerOnboarding
        {
            FullName = request.FullName,
            Email = request.Email.ToLower(),
            Phone = request.Phone,
            CPF = request.CPF,
            BirthDate = request.BirthDate,
            Status = OnboardingStatus.Draft
        };
        _db.TrainerOnboardings.Add(onboarding);
        await _db.SaveChangesAsync();

        return ApiResponse<TrainerOnboardingResponse>.Ok(MapResponse(onboarding));
    }

    public async Task<ApiResponse<TrainerOnboardingResponse>> GetAsync(Guid id)
    {
        var o = await _db.TrainerOnboardings
            .Include(x => x.SelectedPlan)
            .Include(x => x.SelectedPlanPrice)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (o == null) return ApiResponse<TrainerOnboardingResponse>.Fail("Onboarding não encontrado.");
        return ApiResponse<TrainerOnboardingResponse>.Ok(MapResponse(o));
    }

    public async Task<ApiResponse<TrainerOnboardingResponse>> UpdateProfessionalDataAsync(Guid id, UpdateProfessionalDataRequest request)
    {
        var o = await _db.TrainerOnboardings.FindAsync(id);
        if (o == null) return ApiResponse<TrainerOnboardingResponse>.Fail("Onboarding não encontrado.");

        o.BrandName = request.BrandName;
        o.CREF = request.CREF;
        o.Bio = request.Bio;
        o.Specialties = request.Specialties;
        o.Instagram = request.Instagram;
        o.ProfilePhotoUrl = request.ProfilePhotoUrl;
        o.LogoUrl = request.LogoUrl;
        o.PrimaryColor = request.PrimaryColor;
        o.SecondaryColor = request.SecondaryColor;
        o.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return ApiResponse<TrainerOnboardingResponse>.Ok(MapResponse(o));
    }

    public async Task<ApiResponse<TrainerOnboardingResponse>> UpdateAddressAsync(Guid id, UpdateAddressRequest request)
    {
        var o = await _db.TrainerOnboardings.FindAsync(id);
        if (o == null) return ApiResponse<TrainerOnboardingResponse>.Fail("Onboarding não encontrado.");

        o.ZipCode = request.ZipCode;
        o.Street = request.Street;
        o.AddressNumber = request.AddressNumber;
        o.Complement = request.Complement;
        o.Neighborhood = request.Neighborhood;
        o.City = request.City;
        o.State = request.State;
        o.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return ApiResponse<TrainerOnboardingResponse>.Ok(MapResponse(o));
    }

    public async Task<ApiResponse<TrainerOnboardingResponse>> SelectPlanAsync(Guid id, SelectPlanRequest request)
    {
        var o = await _db.TrainerOnboardings.FindAsync(id);
        if (o == null) return ApiResponse<TrainerOnboardingResponse>.Fail("Onboarding não encontrado.");

        var price = await _db.PlatformPlanPrices
            .Include(p => p.PlatformPlan)
            .FirstOrDefaultAsync(p => p.Id == request.PlatformPlanPriceId && p.PlatformPlanId == request.PlatformPlanId && p.Active);

        if (price == null) return ApiResponse<TrainerOnboardingResponse>.Fail("Plano ou preço não encontrado.");

        o.SelectedPlatformPlanId = request.PlatformPlanId;
        o.SelectedPlatformPlanPriceId = request.PlatformPlanPriceId;
        o.BillingCycle = request.BillingCycle;
        o.Status = OnboardingStatus.WaitingPayment;
        o.UpdatedAt = DateTime.UtcNow;
        o.SelectedPlan = price.PlatformPlan;
        o.SelectedPlanPrice = price;
        await _db.SaveChangesAsync();

        return ApiResponse<TrainerOnboardingResponse>.Ok(MapResponse(o));
    }

    public async Task<ApiResponse<TrainerOnboardingResponse>> SimulatePaymentApprovedAsync(Guid id)
    {
        var o = await _db.TrainerOnboardings
            .Include(x => x.SelectedPlan)
            .Include(x => x.SelectedPlanPrice)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (o == null) return ApiResponse<TrainerOnboardingResponse>.Fail("Onboarding não encontrado.");
        if (o.SelectedPlatformPlanId == null) return ApiResponse<TrainerOnboardingResponse>.Fail("Selecione um plano antes de prosseguir.");

        if (await _db.Users.AnyAsync(u => u.Email == o.Email))
            return ApiResponse<TrainerOnboardingResponse>.Fail("Este e-mail já está em uso.");

        // Create User (inactive until password is set — will activate on set-password)
        var user = new User
        {
            Name = o.FullName,
            Email = o.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
            Role = UserRole.Trainer,
            IsActive = true,
            MustChangePassword = true
        };
        _db.Users.Add(user);

        // Create Trainer
        var trainer = new Trainer
        {
            UserId = user.Id,
            BrandName = o.BrandName ?? o.FullName,
            Phone = o.Phone,
            Bio = o.Bio,
            CPF = o.CPF,
            BirthDate = o.BirthDate,
            CREF = o.CREF,
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
            State = o.State
        };
        _db.Trainers.Add(trainer);

        // Create Subscription
        var billingCycle = o.BillingCycle ?? BillingFrequency.Monthly;
        var endDate = billingCycle switch
        {
            BillingFrequency.Quarterly => DateTime.UtcNow.AddMonths(3),
            BillingFrequency.Yearly => DateTime.UtcNow.AddYears(1),
            _ => DateTime.UtcNow.AddMonths(1)
        };

        var subscription = new TrainerSubscription
        {
            TrainerId = trainer.Id,
            PlatformPlanId = o.SelectedPlatformPlanId!.Value,
            PlatformPlanPriceId = o.SelectedPlatformPlanPriceId,
            BillingCycle = billingCycle,
            Status = TrainerSubscriptionStatus.Active,
            StartDate = DateTime.UtcNow,
            EndDate = endDate
        };
        _db.TrainerSubscriptions.Add(subscription);

        // Create Payment record
        _db.TrainerPayments.Add(new TrainerPayment
        {
            TrainerId = trainer.Id,
            TrainerSubscriptionId = subscription.Id,
            Amount = o.SelectedPlanPrice?.Price ?? 0,
            Status = PaymentStatus.Approved,
            Provider = "Simulation",
            ProviderPaymentId = $"SIM-{Guid.NewGuid():N}",
            PaidAt = DateTime.UtcNow
        });

        o.Status = OnboardingStatus.Completed;
        o.CreatedUserId = user.Id;
        o.CreatedTrainerId = trainer.Id;
        o.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        // Send setup email (non-blocking; log to console in MVP)
        var planName = o.SelectedPlan?.Name ?? "FitPlatform";
        await _passwordSetup.GenerateAndSendSetupTokenAsync(user, planName, isStudent: false);

        return ApiResponse<TrainerOnboardingResponse>.Ok(MapResponse(o), "Conta criada com sucesso! Verifique o console para o link de definição de senha.");
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
}
