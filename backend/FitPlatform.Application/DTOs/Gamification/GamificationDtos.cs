using System.ComponentModel.DataAnnotations;

namespace FitPlatform.Application.DTOs.Gamification;

public class GamificationSummaryResponse
{
    public StreakResponse TrainingStreak { get; set; } = new();
    public StreakResponse HabitStreak { get; set; } = new();
    public StreakResponse CheckInStreak { get; set; } = new();
    public MonthlyGoalProgressResponse MonthlyGoals { get; set; } = new();
    public List<StudentAchievementResponse> LatestAchievements { get; set; } = new();
}

public class StreakResponse
{
    public int Current { get; set; }
    public int Best { get; set; }
    public string Rule { get; set; } = string.Empty;
}

public class StudentAchievementResponse
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime UnlockedAt { get; set; }
}

public class AchievementCatalogItemResponse
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Unlocked { get; set; }
    public DateTime? UnlockedAt { get; set; }
}

public class MonthlyGoalProgressResponse
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int WorkoutTarget { get; set; }
    public int HabitDaysTarget { get; set; }
    public int CheckInTarget { get; set; }
    public int WorkoutsCompleted { get; set; }
    public int HabitDaysCompleted { get; set; }
    public int CheckInsCompleted { get; set; }
}

public class StudentMonthlyGoalRequest
{
    [Range(1, 60)] public int WorkoutTarget { get; set; } = 8;
    [Range(1, 31)] public int HabitDaysTarget { get; set; } = 20;
    [Range(1, 8)] public int CheckInTarget { get; set; } = 4;
}
