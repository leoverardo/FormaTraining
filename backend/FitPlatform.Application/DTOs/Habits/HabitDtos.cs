using System.ComponentModel.DataAnnotations;

namespace FitPlatform.Application.DTOs.Habits;

public class StudentHabitRequest
{
    [Required, MaxLength(120)]
    public string Title { get; set; } = string.Empty;
    [MaxLength(400)]
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? Frequency { get; set; }
    public decimal? TargetValue { get; set; }
    [MaxLength(30)]
    public string? TargetUnit { get; set; }
}

public class StudentHabitStatusRequest
{
    public bool IsActive { get; set; }
}

public class StudentHabitResponse
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public decimal? TargetValue { get; set; }
    public string? TargetUnit { get; set; }
    public bool IsActive { get; set; }
    public DateTime? InactivatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class StudentHabitTodayResponse
{
    public DateTime Date { get; set; }
    public int TotalHabits { get; set; }
    public int CompletedHabits { get; set; }
    public List<StudentHabitTodayItemResponse> Items { get; set; } = new();
}

public class StudentHabitTodayItemResponse
{
    public Guid HabitId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal? TargetValue { get; set; }
    public string? TargetUnit { get; set; }
    public bool IsCompleted { get; set; }
    public decimal? Value { get; set; }
    public string? Note { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class StudentHabitTodayUpdateRequest
{
    public bool IsCompleted { get; set; }
    public decimal? Value { get; set; }
    [MaxLength(200)]
    public string? Note { get; set; }
}

public class HabitAdherenceResponse
{
    public Guid StudentId { get; set; }
    public int Days { get; set; }
    public int TotalExpected { get; set; }
    public int TotalCompleted { get; set; }
    public decimal CompletionRate { get; set; }
    public string? LowestHabitTitle { get; set; }
    public decimal? LowestHabitRate { get; set; }
    public List<HabitAdherenceHabitItem> Habits { get; set; } = new();
    public List<HabitAdherenceDayItem> DaysSummary { get; set; } = new();
}

public class HabitAdherenceHabitItem
{
    public Guid HabitId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Expected { get; set; }
    public int Completed { get; set; }
    public decimal CompletionRate { get; set; }
}

public class HabitAdherenceDayItem
{
    public DateTime Date { get; set; }
    public int Expected { get; set; }
    public int Completed { get; set; }
}

public class StudentNutritionGuidanceRequest
{
    [Required, MaxLength(5000)]
    public string GuidanceText { get; set; } = string.Empty;
    [MaxLength(2000)]
    public string? StrategicNotes { get; set; }
    public Guid? MediaId { get; set; }
}

public class StudentNutritionGuidanceResponse
{
    public Guid StudentId { get; set; }
    public string GuidanceText { get; set; } = string.Empty;
    public string? StrategicNotes { get; set; }
    public Guid? MediaId { get; set; }
    public DateTime UpdatedAt { get; set; }
}
