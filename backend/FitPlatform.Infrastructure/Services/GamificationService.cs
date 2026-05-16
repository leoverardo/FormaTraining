using System.Globalization;
using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Gamification;
using FitPlatform.Application.Interfaces;
using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Infrastructure.Services;

public class GamificationService
{
    private const decimal HabitDayThreshold = 0.7m;
    private readonly AppDbContext _db;
    private readonly IDateTimeProvider _clock;
    private readonly NotificationService _notifications;

    private static readonly Dictionary<AchievementCode, (string Title, string Description)> Catalog = new()
    {
        [AchievementCode.FirstWorkoutCompleted] = ("Primeiro treino concluido", "Voce concluiu seu primeiro treino."),
        [AchievementCode.FiveWorkoutsCompleted] = ("5 treinos concluidos", "Voce concluiu 5 treinos."),
        [AchievementCode.TenWorkoutsCompleted] = ("10 treinos concluidos", "Voce concluiu 10 treinos."),
        [AchievementCode.ThirtyWorkoutsCompleted] = ("30 treinos concluidos", "Voce concluiu 30 treinos."),
        [AchievementCode.FirstCheckInSubmitted] = ("Primeiro check-in enviado", "Seu primeiro check-in semanal foi enviado."),
        [AchievementCode.FourCheckInsInARow] = ("4 check-ins seguidos", "Voce manteve 4 semanas seguidas de check-in."),
        [AchievementCode.FirstHabitDayCompleted] = ("Primeiro dia de habitos", "Voce concluiu seu primeiro dia valido de habitos."),
        [AchievementCode.SevenHabitDays] = ("7 dias de habitos", "Voce completou 7 dias validos de habitos."),
        [AchievementCode.ThirtyHabitDays] = ("30 dias de habitos", "Voce completou 30 dias validos de habitos."),
        [AchievementCode.TrainingStreak4Weeks] = ("4 semanas treinando", "Voce treinou por 4 semanas consecutivas."),
        [AchievementCode.HabitStreak7Days] = ("7 dias seguidos de habitos", "Voce manteve 7 dias seguidos de consistencia de habitos."),
        [AchievementCode.HabitStreak30Days] = ("30 dias seguidos de habitos", "Voce manteve 30 dias seguidos de consistencia de habitos.")
    };

    public GamificationService(AppDbContext db, IDateTimeProvider clock, NotificationService notifications)
    {
        _db = db;
        _clock = clock;
        _notifications = notifications;
    }

    public async Task<ApiResponse<GamificationSummaryResponse>> GetStudentSummaryAsync(Guid studentId) =>
        ApiResponse<GamificationSummaryResponse>.Ok(await BuildSummaryAsync(studentId));

    public async Task<ApiResponse<GamificationSummaryResponse>> GetTrainerStudentSummaryAsync(Guid trainerId, Guid studentId)
    {
        if (!await IsTrainerStudentAsync(trainerId, studentId))
            return ApiResponse<GamificationSummaryResponse>.Fail("Aluno nao encontrado.");
        return await GetStudentSummaryAsync(studentId);
    }

    public async Task<ApiResponse<List<AchievementCatalogItemResponse>>> GetStudentAchievementsAsync(Guid studentId)
    {
        var unlocked = await _db.StudentAchievements
            .Where(a => a.StudentId == studentId)
            .ToDictionaryAsync(a => a.AchievementCode, a => a.UnlockedAt);

        var items = Catalog.Select(c => new AchievementCatalogItemResponse
        {
            Code = c.Key.ToString(),
            Title = c.Value.Title,
            Description = c.Value.Description,
            Unlocked = unlocked.ContainsKey(c.Key),
            UnlockedAt = unlocked.TryGetValue(c.Key, out var at) ? at : null
        }).ToList();

        return ApiResponse<List<AchievementCatalogItemResponse>>.Ok(items);
    }

    public async Task<ApiResponse<List<AchievementCatalogItemResponse>>> GetTrainerStudentAchievementsAsync(Guid trainerId, Guid studentId)
    {
        if (!await IsTrainerStudentAsync(trainerId, studentId))
            return ApiResponse<List<AchievementCatalogItemResponse>>.Fail("Aluno nao encontrado.");
        return await GetStudentAchievementsAsync(studentId);
    }

    public async Task<ApiResponse<MonthlyGoalProgressResponse>> GetMonthlyGoalsAsync(Guid studentId, int? year = null, int? month = null)
    {
        var nowLocal = _clock.LocalNow;
        var y = year ?? nowLocal.Year;
        var m = month ?? nowLocal.Month;
        return ApiResponse<MonthlyGoalProgressResponse>.Ok(await BuildMonthlyGoalProgressAsync(studentId, y, m));
    }

    public async Task<ApiResponse<MonthlyGoalProgressResponse>> GetTrainerStudentMonthlyGoalsAsync(Guid trainerId, Guid studentId, int? year = null, int? month = null)
    {
        if (!await IsTrainerStudentAsync(trainerId, studentId))
            return ApiResponse<MonthlyGoalProgressResponse>.Fail("Aluno nao encontrado.");
        return await GetMonthlyGoalsAsync(studentId, year, month);
    }

    public async Task<ApiResponse<MonthlyGoalProgressResponse>> UpsertMonthlyGoalsAsync(Guid trainerId, Guid studentId, int year, int month, StudentMonthlyGoalRequest request)
    {
        var isOwnStudent = await _db.Students.AnyAsync(s => s.Id == studentId && s.TrainerId == trainerId);
        if (!isOwnStudent) return ApiResponse<MonthlyGoalProgressResponse>.Fail("Aluno nao encontrado.");
        if (month is < 1 or > 12) return ApiResponse<MonthlyGoalProgressResponse>.Fail("Mes invalido.");

        var goal = await _db.StudentMonthlyGoals.FirstOrDefaultAsync(g => g.StudentId == studentId && g.Year == year && g.Month == month);
        if (goal == null)
        {
            goal = new StudentMonthlyGoal
            {
                StudentId = studentId,
                Year = year,
                Month = month
            };
            _db.StudentMonthlyGoals.Add(goal);
        }

        goal.WorkoutTarget = request.WorkoutTarget;
        goal.HabitDaysTarget = request.HabitDaysTarget;
        goal.CheckInTarget = request.CheckInTarget;
        goal.UpdatedAt = _clock.UtcNow;
        await _db.SaveChangesAsync();

        return ApiResponse<MonthlyGoalProgressResponse>.Ok(await BuildMonthlyGoalProgressAsync(studentId, year, month), "Metas mensais atualizadas.");
    }

    public async Task EvaluateForWorkoutCompletedAsync(Guid studentId) => await EvaluateAndUnlockAsync(studentId);
    public async Task EvaluateForCheckInSubmittedAsync(Guid studentId) => await EvaluateAndUnlockAsync(studentId);
    public async Task EvaluateForHabitUpdatedAsync(Guid studentId) => await EvaluateAndUnlockAsync(studentId);

    private async Task<GamificationSummaryResponse> BuildSummaryAsync(Guid studentId)
    {
        var training = await CalculateTrainingStreakAsync(studentId);
        var habits = await CalculateHabitStreakAsync(studentId);
        var checkins = await CalculateCheckInStreakAsync(studentId);
        var month = _clock.LocalNow;
        var goals = await BuildMonthlyGoalProgressAsync(studentId, month.Year, month.Month);

        var latest = await _db.StudentAchievements
            .Where(a => a.StudentId == studentId)
            .OrderByDescending(a => a.UnlockedAt)
            .Take(5)
            .ToListAsync();

        return new GamificationSummaryResponse
        {
            TrainingStreak = training,
            HabitStreak = habits,
            CheckInStreak = checkins,
            MonthlyGoals = goals,
            LatestAchievements = latest.Select(MapAchievement).ToList()
        };
    }

    private async Task EvaluateAndUnlockAsync(Guid studentId)
    {
        var unlockedList = await _db.StudentAchievements
            .Where(a => a.StudentId == studentId)
            .Select(a => a.AchievementCode)
            .ToListAsync();
        var unlocked = unlockedList.ToHashSet();

        var workoutCount = await _db.WorkoutSessions.CountAsync(w => w.StudentId == studentId && w.Status == WorkoutSessionStatus.Completed);
        var checkInCount = await _db.StudentWeeklyCheckIns.CountAsync(c => c.StudentId == studentId);

        var habitDays = await CalculateCompletedHabitDaysAsync(studentId, null, null);
        var trainingStreak = await CalculateTrainingStreakAsync(studentId);
        var habitStreak = await CalculateHabitStreakAsync(studentId);
        var checkInStreak = await CalculateCheckInStreakAsync(studentId);

        var candidates = new List<AchievementCode>();
        if (workoutCount >= 1) candidates.Add(AchievementCode.FirstWorkoutCompleted);
        if (workoutCount >= 5) candidates.Add(AchievementCode.FiveWorkoutsCompleted);
        if (workoutCount >= 10) candidates.Add(AchievementCode.TenWorkoutsCompleted);
        if (workoutCount >= 30) candidates.Add(AchievementCode.ThirtyWorkoutsCompleted);
        if (checkInCount >= 1) candidates.Add(AchievementCode.FirstCheckInSubmitted);
        if (checkInStreak.Current >= 4) candidates.Add(AchievementCode.FourCheckInsInARow);
        if (habitDays >= 1) candidates.Add(AchievementCode.FirstHabitDayCompleted);
        if (habitDays >= 7) candidates.Add(AchievementCode.SevenHabitDays);
        if (habitDays >= 30) candidates.Add(AchievementCode.ThirtyHabitDays);
        if (trainingStreak.Current >= 4) candidates.Add(AchievementCode.TrainingStreak4Weeks);
        if (habitStreak.Current >= 7) candidates.Add(AchievementCode.HabitStreak7Days);
        if (habitStreak.Current >= 30) candidates.Add(AchievementCode.HabitStreak30Days);

        var unlockedNow = new List<AchievementCode>();
        foreach (var code in candidates.Where(c => !unlocked.Contains(c)))
        {
            var achievement = new StudentAchievement
            {
                StudentId = studentId,
                AchievementCode = code,
                UnlockedAt = _clock.UtcNow
            };
            _db.StudentAchievements.Add(achievement);
            unlocked.Add(code);
            unlockedNow.Add(code);
        }

        if (!_db.ChangeTracker.HasChanges()) return;
        await _db.SaveChangesAsync();

        var studentUser = await _db.Students.Select(s => new { s.Id, s.UserId }).FirstOrDefaultAsync(s => s.Id == studentId);
        if (studentUser == null) return;

        foreach (var code in unlockedNow)
        {
            var catalog = Catalog[code];
            await _notifications.CreateAsync(studentUser.UserId, "Nova conquista desbloqueada", catalog.Title, NotificationType.AchievementUnlocked, null, studentId);
        }
    }

    private async Task<MonthlyGoalProgressResponse> BuildMonthlyGoalProgressAsync(Guid studentId, int year, int month)
    {
        var goal = await _db.StudentMonthlyGoals.FirstOrDefaultAsync(g => g.StudentId == studentId && g.Year == year && g.Month == month);
        if (goal == null)
        {
            goal = new StudentMonthlyGoal
            {
                StudentId = studentId,
                Year = year,
                Month = month
            };
            _db.StudentMonthlyGoals.Add(goal);
            await _db.SaveChangesAsync();
        }

        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1).AddDays(-1);
        var startUtc = start.AddHours(3);
        var endUtcExclusive = end.AddDays(1).AddHours(3);

        var workouts = await _db.WorkoutSessions.CountAsync(w =>
            w.StudentId == studentId &&
            w.Status == WorkoutSessionStatus.Completed &&
            w.CompletedAt.HasValue &&
            w.CompletedAt.Value >= startUtc &&
            w.CompletedAt.Value < endUtcExclusive);

        var checkIns = await _db.StudentWeeklyCheckIns.CountAsync(c =>
            c.StudentId == studentId &&
            c.WeekStartDate >= start &&
            c.WeekStartDate <= end);

        var habitDays = await CalculateCompletedHabitDaysAsync(studentId, start, end);

        return new MonthlyGoalProgressResponse
        {
            Year = year,
            Month = month,
            WorkoutTarget = goal.WorkoutTarget,
            HabitDaysTarget = goal.HabitDaysTarget,
            CheckInTarget = goal.CheckInTarget,
            WorkoutsCompleted = workouts,
            HabitDaysCompleted = habitDays,
            CheckInsCompleted = checkIns
        };
    }

    private async Task<StreakResponse> CalculateTrainingStreakAsync(Guid studentId)
    {
        var completedWeeks = await _db.WorkoutSessions
            .Where(w => w.StudentId == studentId && w.Status == WorkoutSessionStatus.Completed && w.CompletedAt.HasValue)
            .Select(w => w.CompletedAt!.Value)
            .ToListAsync();

        var weekSet = completedWeeks
            .Select(GetWeekKeyFromUtc)
            .Distinct()
            .ToHashSet();

        var current = 0;
        var best = 0;
        var cursor = GetWeekKey(_clock.LocalDate);
        while (weekSet.Contains(cursor))
        {
            current++;
            var prevWeek = cursor.Week - 1;
            var prevYear = cursor.Year;
            if (prevWeek <= 0)
            {
                prevYear -= 1;
                prevWeek = ISOWeek.GetWeeksInYear(prevYear);
            }
            cursor = (prevYear, prevWeek);
        }

        foreach (var week in weekSet)
        {
            var chain = 1;
            var scan = week;
            while (true)
            {
                scan = (scan.Year, scan.Week + 1);
                if (scan.Week > ISOWeek.GetWeeksInYear(scan.Year)) scan = (scan.Year + 1, 1);
                if (!weekSet.Contains(scan)) break;
                chain++;
            }
            best = Math.Max(best, chain);
        }

        return new StreakResponse
        {
            Current = current,
            Best = best,
            Rule = "Semanas consecutivas com pelo menos 1 treino concluido."
        };
    }

    private async Task<StreakResponse> CalculateCheckInStreakAsync(Guid studentId)
    {
        var weekStarts = await _db.StudentWeeklyCheckIns
            .Where(c => c.StudentId == studentId)
            .Select(c => c.WeekStartDate.Date)
            .Distinct()
            .ToListAsync();

        var weekSet = weekStarts.Select(GetWeekKey).ToHashSet();
        var current = 0;
        var best = 0;
        var cursor = GetWeekKey(_clock.LocalDate);
        while (weekSet.Contains(cursor))
        {
            current++;
            var prevWeek = cursor.Week - 1;
            var prevYear = cursor.Year;
            if (prevWeek <= 0)
            {
                prevYear -= 1;
                prevWeek = ISOWeek.GetWeeksInYear(prevYear);
            }
            cursor = (prevYear, prevWeek);
        }

        foreach (var week in weekSet)
        {
            var chain = 1;
            var scan = week;
            while (true)
            {
                scan = (scan.Year, scan.Week + 1);
                if (scan.Week > ISOWeek.GetWeeksInYear(scan.Year)) scan = (scan.Year + 1, 1);
                if (!weekSet.Contains(scan)) break;
                chain++;
            }
            best = Math.Max(best, chain);
        }

        return new StreakResponse
        {
            Current = current,
            Best = best,
            Rule = "Semanas consecutivas com check-in enviado."
        };
    }

    private async Task<StreakResponse> CalculateHabitStreakAsync(Guid studentId)
    {
        var firstHabitDate = await _db.StudentHabits
            .Where(h => h.StudentId == studentId)
            .MinAsync(h => (DateTime?)h.CreatedAt.Date);
        if (!firstHabitDate.HasValue)
            return new StreakResponse { Current = 0, Best = 0, Rule = "Dias consecutivos com no minimo 70% dos habitos do dia concluidos." };

        var end = _clock.LocalDate;
        var logs = await _db.StudentHabitLogs
            .Where(l => l.StudentId == studentId && l.Date >= firstHabitDate.Value && l.Date <= end)
            .ToListAsync();
        var habits = await _db.StudentHabits.Where(h => h.StudentId == studentId).ToListAsync();

        var dayStatus = new Dictionary<DateTime, bool>();
        for (var day = firstHabitDate.Value.Date; day <= end; day = day.AddDays(1))
        {
            var expected = habits.Count(h => IsHabitActiveOnDay(h, day));
            if (expected == 0) continue;
            var completed = logs.Count(l => l.Date == day && l.IsCompleted);
            dayStatus[day] = completed >= Math.Ceiling(expected * HabitDayThreshold);
        }

        var current = 0;
        for (var cursor = end; cursor >= firstHabitDate.Value; cursor = cursor.AddDays(-1))
        {
            if (!dayStatus.TryGetValue(cursor, out var isValid)) continue;
            if (!isValid) break;
            current++;
        }

        var best = 0;
        var chain = 0;
        foreach (var day in dayStatus.OrderBy(d => d.Key))
        {
            if (day.Value) chain++;
            else chain = 0;
            if (chain > best) best = chain;
        }

        return new StreakResponse
        {
            Current = current,
            Best = best,
            Rule = "Dias consecutivos com no minimo 70% dos habitos esperados concluidos."
        };
    }

    private async Task<int> CalculateCompletedHabitDaysAsync(Guid studentId, DateTime? start, DateTime? end)
    {
        var firstHabitDate = await _db.StudentHabits
            .Where(h => h.StudentId == studentId)
            .MinAsync(h => (DateTime?)h.CreatedAt.Date);
        var from = start ?? firstHabitDate ?? _clock.LocalDate;
        var to = end ?? _clock.LocalDate;
        if (to < from) return 0;

        var logs = await _db.StudentHabitLogs
            .Where(l => l.StudentId == studentId && l.Date >= from && l.Date <= to)
            .ToListAsync();
        var habits = await _db.StudentHabits.Where(h => h.StudentId == studentId).ToListAsync();

        var validDays = 0;
        for (var day = from.Date; day <= to.Date; day = day.AddDays(1))
        {
            var expected = habits.Count(h => IsHabitActiveOnDay(h, day));
            if (expected == 0) continue;
            var completed = logs.Count(l => l.Date == day && l.IsCompleted);
            if (completed >= Math.Ceiling(expected * HabitDayThreshold)) validDays++;
        }

        return validDays;
    }

    private static bool IsHabitActiveOnDay(StudentHabit habit, DateTime day)
    {
        if (habit.CreatedAt.Date > day) return false;
        return !habit.InactivatedAt.HasValue || habit.InactivatedAt.Value.Date >= day;
    }

    private static (int Year, int Week) GetWeekKeyFromUtc(DateTime utc)
    {
        var local = utc.AddHours(-3).Date;
        return GetWeekKey(local);
    }

    private static (int Year, int Week) GetWeekKey(DateTime date) => (ISOWeek.GetYear(date), ISOWeek.GetWeekOfYear(date));

    private static StudentAchievementResponse MapAchievement(StudentAchievement achievement)
    {
        var catalog = Catalog[achievement.AchievementCode];
        return new StudentAchievementResponse
        {
            Code = achievement.AchievementCode.ToString(),
            Title = catalog.Title,
            Description = catalog.Description,
            UnlockedAt = achievement.UnlockedAt
        };
    }

    private Task<bool> IsTrainerStudentAsync(Guid trainerId, Guid studentId) =>
        _db.Students.AnyAsync(s => s.Id == studentId && s.TrainerId == trainerId);
}
