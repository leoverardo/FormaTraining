using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class StudentProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Neighborhood { get; set; }
    public string? Goal { get; set; }
    public string? Interests { get; set; }
    public string? TrainingLevel { get; set; }
    public string? PreferredTrainingMode { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public StudentAccountStatus AccountStatus { get; set; } = StudentAccountStatus.Explorer;

    public User User { get; set; } = null!;
    public ICollection<TrainerFollower> FollowingTrainers { get; set; } = new List<TrainerFollower>();
    public ICollection<SavedTrainer> SavedTrainers { get; set; } = new List<SavedTrainer>();
}
