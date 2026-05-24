using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Plans;
using FitPlatform.Domain.Entities;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Infrastructure.Services;

public class PlatformPlanService
{
    private readonly AppDbContext _db;

    public PlatformPlanService(AppDbContext db) => _db = db;

    public async Task<ApiResponse<List<PlatformPlanResponse>>> GetAllAsync()
    {
        var plans = await _db.PlatformPlans
            .Include(p => p.Prices)
            .OrderBy(p => p.MonthlyPrice)
            .ToListAsync();
        return ApiResponse<List<PlatformPlanResponse>>.Ok(plans.Select(MapResponse).ToList());
    }

    public async Task<ApiResponse<PlatformPlanResponse>> GetByIdAsync(Guid id)
    {
        var plan = await _db.PlatformPlans.Include(p => p.Prices).FirstOrDefaultAsync(p => p.Id == id);
        if (plan == null) return ApiResponse<PlatformPlanResponse>.Fail("Plano não encontrado.");
        return ApiResponse<PlatformPlanResponse>.Ok(MapResponse(plan));
    }

    public async Task<ApiResponse<PlatformPlanResponse>> CreateAsync(PlatformPlanRequest request)
    {
        var planCode = string.IsNullOrWhiteSpace(request.Code)
            ? request.Name.Trim().ToUpperInvariant()
            : request.Code.Trim().ToUpperInvariant();

        var plan = new PlatformPlan
        {
            Code = planCode,
            Name = request.Name,
            Description = request.Description,
            MonthlyPrice = request.MonthlyPrice,
            MaxActiveStudents = request.MaxActiveStudents,
            HasUnlimitedStudents = request.HasUnlimitedStudents,
            Active = request.Active,
            IsPublic = request.IsPublic,
            IsComingSoon = request.IsComingSoon,
            IsAvailableForPurchase = request.IsAvailableForPurchase
        };
        _db.PlatformPlans.Add(plan);
        await _db.SaveChangesAsync();
        await _db.Entry(plan).Collection(p => p.Prices).LoadAsync();
        return ApiResponse<PlatformPlanResponse>.Ok(MapResponse(plan), "Plano criado.");
    }

    public async Task<ApiResponse<PlatformPlanResponse>> UpdateAsync(Guid id, PlatformPlanRequest request)
    {
        var plan = await _db.PlatformPlans.Include(p => p.Prices).FirstOrDefaultAsync(p => p.Id == id);
        if (plan == null) return ApiResponse<PlatformPlanResponse>.Fail("Plano não encontrado.");

        plan.Code = string.IsNullOrWhiteSpace(request.Code)
            ? request.Name.Trim().ToUpperInvariant()
            : request.Code.Trim().ToUpperInvariant();
        plan.Name = request.Name;
        plan.Description = request.Description;
        plan.MonthlyPrice = request.MonthlyPrice;
        plan.MaxActiveStudents = request.MaxActiveStudents;
        plan.HasUnlimitedStudents = request.HasUnlimitedStudents;
        plan.Active = request.Active;
        plan.IsPublic = request.IsPublic;
        plan.IsComingSoon = request.IsComingSoon;
        plan.IsAvailableForPurchase = request.IsAvailableForPurchase;
        plan.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ApiResponse<PlatformPlanResponse>.Ok(MapResponse(plan));
    }

    public async Task<ApiResponse> DeleteAsync(Guid id)
    {
        var plan = await _db.PlatformPlans.FindAsync(id);
        if (plan == null) return ApiResponse.Fail("Plano não encontrado.");
        _db.PlatformPlans.Remove(plan);
        await _db.SaveChangesAsync();
        return ApiResponse.Ok("Plano removido.");
    }

    private static PlatformPlanResponse MapResponse(PlatformPlan p) => new()
    {
        Id = p.Id,
        Code = p.Code,
        Name = p.Name,
        Description = p.Description,
        MonthlyPrice = p.MonthlyPrice,
        MaxActiveStudents = p.MaxActiveStudents,
        HasUnlimitedStudents = p.HasUnlimitedStudents,
        Active = p.Active,
        IsPublic = p.IsPublic,
        IsComingSoon = p.IsComingSoon,
        IsAvailableForPurchase = p.IsAvailableForPurchase,
        CreatedAt = p.CreatedAt,
        Prices = p.Prices.Where(pr => pr.Active).OrderBy(pr => pr.BillingCycle).Select(pr => new PlanPriceResponse
        {
            Id = pr.Id,
            BillingCycle = pr.BillingCycle.ToString(),
            Price = pr.Price,
            Active = pr.Active
        }).ToList()
    };
}
