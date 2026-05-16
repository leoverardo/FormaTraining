using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class Student : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid TrainerId { get; set; }
    public string? Phone { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Goal { get; set; }
    public string? Notes { get; set; }
    public StudentStatus Status { get; set; } = StudentStatus.Active;
    public StudentMonitoringStatus MonitoringStatus { get; set; } = StudentMonitoringStatus.OnTrack;
    public DateTime? LastMonitoringStatusCalculatedAt { get; set; }

    public User User { get; set; } = null!;
    public Trainer Trainer { get; set; } = null!;
    public ICollection<StudentWorkoutSchedule> WorkoutSchedules { get; set; } = new List<StudentWorkoutSchedule>();
    public ICollection<StudentProgress> ProgressRecords { get; set; } = new List<StudentProgress>();
    public ICollection<StudentProgressPhoto> ProgressPhotos { get; set; } = new List<StudentProgressPhoto>();
    public ICollection<StudentWeeklyCheckIn> CheckIns { get; set; } = new List<StudentWeeklyCheckIn>();
    public ICollection<WorkoutSession> WorkoutSessions { get; set; } = new List<WorkoutSession>();
    public ICollection<StudentHabit> Habits { get; set; } = new List<StudentHabit>();
    public ICollection<StudentHabitLog> HabitLogs { get; set; } = new List<StudentHabitLog>();
    public ICollection<StudentAchievement> Achievements { get; set; } = new List<StudentAchievement>();
    public ICollection<StudentMonthlyGoal> MonthlyGoals { get; set; } = new List<StudentMonthlyGoal>();
    public ICollection<TrainerServiceOrder> ServiceOrders { get; set; } = new List<TrainerServiceOrder>();
}
