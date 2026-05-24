using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using FitPlatform.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Tests;

public class PlansCommercialStrategyTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task PlatformPlanService_ShouldReturnBasicAsPurchasableAndUnlimited()
    {
        await using var db = CreateDb();
        var basic = new PlatformPlan
        {
            Code = "BASIC",
            Name = "Basic",
            MonthlyPrice = 59.90m,
            MaxActiveStudents = 0,
            HasUnlimitedStudents = true,
            Active = true,
            IsPublic = true,
            IsComingSoon = false,
            IsAvailableForPurchase = true
        };
        var pro = new PlatformPlan
        {
            Code = "PRO",
            Name = "Pro",
            MonthlyPrice = 197m,
            MaxActiveStudents = 50,
            HasUnlimitedStudents = false,
            Active = true,
            IsPublic = true,
            IsComingSoon = true,
            IsAvailableForPurchase = false
        };
        db.PlatformPlans.AddRange(basic, pro);
        await db.SaveChangesAsync();

        var service = new PlatformPlanService(db);
        var result = await service.GetAllAsync();

        Assert.True(result.Success);
        var basicResponse = Assert.Single(result.Data!.Where(x => x.Code == "BASIC"));
        Assert.Equal(59.90m, basicResponse.MonthlyPrice);
        Assert.True(basicResponse.HasUnlimitedStudents);
        Assert.True(basicResponse.IsAvailableForPurchase);
        Assert.False(basicResponse.IsComingSoon);

        var proResponse = Assert.Single(result.Data.Where(x => x.Code == "PRO"));
        Assert.False(proResponse.IsAvailableForPurchase);
        Assert.True(proResponse.IsComingSoon);
    }

    [Fact]
    public async Task StudentService_Activate_ShouldNotBlockWhenPlanIsUnlimited()
    {
        await using var db = CreateDb();
        var trainerUser = new User { Name = "Trainer", Email = "trainer@test.com", PasswordHash = "x", Role = UserRole.Trainer, IsActive = true };
        var activeStudentUser = new User { Name = "Student 1", Email = "s1@test.com", PasswordHash = "x", Role = UserRole.Student, IsActive = true };
        var inactiveStudentUser = new User { Name = "Student 2", Email = "s2@test.com", PasswordHash = "x", Role = UserRole.Student, IsActive = true };
        db.Users.AddRange(trainerUser, activeStudentUser, inactiveStudentUser);

        var trainer = new Trainer { UserId = trainerUser.Id, BrandName = "Brand" };
        db.Trainers.Add(trainer);

        var basic = new PlatformPlan
        {
            Code = "BASIC",
            Name = "Basic",
            MonthlyPrice = 59.90m,
            MaxActiveStudents = 0,
            HasUnlimitedStudents = true,
            Active = true,
            IsPublic = true,
            IsAvailableForPurchase = true
        };
        db.PlatformPlans.Add(basic);
        await db.SaveChangesAsync();

        db.TrainerSubscriptions.Add(new TrainerSubscription
        {
            TrainerId = trainer.Id,
            PlatformPlanId = basic.Id,
            BillingCycle = BillingFrequency.Monthly,
            Status = TrainerSubscriptionStatus.Active,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(1)
        });

        var activeStudent = new Student { UserId = activeStudentUser.Id, TrainerId = trainer.Id, Status = StudentStatus.Active };
        var inactiveStudent = new Student { UserId = inactiveStudentUser.Id, TrainerId = trainer.Id, Status = StudentStatus.Inactive };
        db.Students.AddRange(activeStudent, inactiveStudent);
        await db.SaveChangesAsync();

        var service = new StudentService(db, null!);
        var activation = await service.ActivateAsync(inactiveStudent.Id, trainer.Id);

        Assert.True(activation.Success);
        Assert.Equal("Active", activation.Data!.Status);
    }
}
