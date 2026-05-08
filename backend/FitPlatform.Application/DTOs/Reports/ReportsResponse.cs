namespace FitPlatform.Application.DTOs.Reports;

public class OverviewReportResponse
{
    public int TotalStudents { get; set; }
    public int ActiveStudents { get; set; }
    public int InactiveStudents { get; set; }
    public int StudentsWithCheckInThisWeek { get; set; }
    public int StudentsWithoutCheckInThisWeek { get; set; }
    public int WorkoutSessionsThisWeek { get; set; }
    public int WorkoutsCompletedThisWeek { get; set; }
    public int ProgressRecordsThisWeek { get; set; }
    public int UnreadNotifications { get; set; }
    public List<StudentEngagementItem> StudentEngagement { get; set; } = new();
}

public class StudentEngagementItem
{
    public Guid StudentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string MonitoringStatus { get; set; } = string.Empty;
    public DateTime? LastLogin { get; set; }
    public DateTime? LastCheckIn { get; set; }
    public int CheckInsThisMonth { get; set; }
    public int WorkoutsCompletedThisMonth { get; set; }
}
