using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class StudentAchievement : BaseEntity
{
    public Guid StudentId { get; set; }
    public AchievementCode AchievementCode { get; set; }
    public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;
    public string? MetadataJson { get; set; }

    public Student Student { get; set; } = null!;
}
