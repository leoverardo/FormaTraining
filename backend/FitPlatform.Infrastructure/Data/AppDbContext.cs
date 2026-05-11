using FitPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Trainer> Trainers => Set<Trainer>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
    public DbSet<TrainerFollower> TrainerFollowers => Set<TrainerFollower>();
    public DbSet<SavedTrainer> SavedTrainers => Set<SavedTrainer>();
    public DbSet<TrainerLead> TrainerLeads => Set<TrainerLead>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<Workout> Workouts => Set<Workout>();
    public DbSet<WorkoutExercise> WorkoutExercises => Set<WorkoutExercise>();
    public DbSet<StudentWorkoutSchedule> StudentWorkoutSchedules => Set<StudentWorkoutSchedule>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PlatformPlan> PlatformPlans => Set<PlatformPlan>();
    public DbSet<PlatformPlanPrice> PlatformPlanPrices => Set<PlatformPlanPrice>();
    public DbSet<TrainerSubscription> TrainerSubscriptions => Set<TrainerSubscription>();
    public DbSet<TrainerPayment> TrainerPayments => Set<TrainerPayment>();
    public DbSet<PaymentWebhookLog> PaymentWebhookLogs => Set<PaymentWebhookLog>();
    public DbSet<PlatformFeature> PlatformFeatures => Set<PlatformFeature>();
    public DbSet<PlatformPlanFeature> PlatformPlanFeatures => Set<PlatformPlanFeature>();
    public DbSet<TrainerOnboarding> TrainerOnboardings => Set<TrainerOnboarding>();
    public DbSet<PasswordSetupToken> PasswordSetupTokens => Set<PasswordSetupToken>();
    public DbSet<StudentInvite> StudentInvites => Set<StudentInvite>();
    public DbSet<StudentProgress> StudentProgressRecords => Set<StudentProgress>();
    public DbSet<StudentProgressPhoto> StudentProgressPhotos => Set<StudentProgressPhoto>();
    public DbSet<StudentWeeklyCheckIn> StudentWeeklyCheckIns => Set<StudentWeeklyCheckIn>();
    public DbSet<StudentAnamnesis> StudentAnamnesisRecords => Set<StudentAnamnesis>();
    public DbSet<ProgressComment> ProgressComments => Set<ProgressComment>();
    public DbSet<WorkoutSession> WorkoutSessions => Set<WorkoutSession>();
    public DbSet<WorkoutSessionExercise> WorkoutSessionExercises => Set<WorkoutSessionExercise>();
    public DbSet<ExerciseLibraryItem> ExerciseLibraryItems => Set<ExerciseLibraryItem>();
    public DbSet<WorkoutTemplate> WorkoutTemplates => Set<WorkoutTemplate>();
    public DbSet<WorkoutTemplateExercise> WorkoutTemplateExercises => Set<WorkoutTemplateExercise>();
    public DbSet<StudentTestimonial> StudentTestimonials => Set<StudentTestimonial>();
    public DbSet<StudentTransformation> StudentTransformations => Set<StudentTransformation>();
    public DbSet<TrainerStudentNote> TrainerStudentNotes => Set<TrainerStudentNote>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<MediaFile> MediaFiles => Set<MediaFile>();
    public DbSet<TermsDocument> TermsDocuments => Set<TermsDocument>();
    public DbSet<UserConsent> UserConsents => Set<UserConsent>();
    public DbSet<DataPrivacyRequest> DataPrivacyRequests => Set<DataPrivacyRequest>();
    public DbSet<FeedReaction> FeedReactions => Set<FeedReaction>();
    public DbSet<FeedSavedItem> FeedSavedItems => Set<FeedSavedItem>();
    public DbSet<FeedComment> FeedComments => Set<FeedComment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Role).HasConversion<int>();
        });

        modelBuilder.Entity<Trainer>(e =>
        {
            e.HasIndex(t => t.UserId).IsUnique();
            e.HasIndex(t => t.PublicSlug).IsUnique().HasFilter("[PublicSlug] IS NOT NULL");
            e.HasIndex(t => new { t.PublicPageEnabled, t.PublicSearchEnabled, t.AcceptingStudents });
            e.Property(t => t.Latitude).HasColumnType("float");
            e.Property(t => t.Longitude).HasColumnType("float");
            e.HasOne(t => t.User).WithOne(u => u.Trainer).HasForeignKey<Trainer>(t => t.UserId);
            e.HasOne(t => t.ProfilePhotoMedia).WithMany().HasForeignKey(t => t.ProfilePhotoMediaId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
            e.HasOne(t => t.LogoMedia).WithMany().HasForeignKey(t => t.LogoMediaId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
            e.HasOne(t => t.BannerMedia).WithMany().HasForeignKey(t => t.BannerMediaId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
            e.HasOne(t => t.PublicBannerMedia).WithMany().HasForeignKey(t => t.PublicBannerMediaId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
        });

        modelBuilder.Entity<Student>(e =>
        {
            e.HasIndex(s => s.UserId).IsUnique();
            e.HasIndex(s => s.TrainerId);
            e.Property(s => s.Status).HasConversion<int>();
            e.Property(s => s.MonitoringStatus).HasConversion<int>();
            e.HasOne(s => s.User).WithOne(u => u.Student).HasForeignKey<Student>(s => s.UserId);
            e.HasOne(s => s.Trainer).WithMany(t => t.Students).HasForeignKey(s => s.TrainerId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StudentProfile>(e =>
        {
            e.HasIndex(s => s.UserId).IsUnique();
            e.HasIndex(s => new { s.City, s.State });
            e.Property(s => s.AccountStatus).HasConversion<int>();
            e.HasOne(s => s.User).WithOne(u => u.StudentProfile).HasForeignKey<StudentProfile>(s => s.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TrainerFollower>(e =>
        {
            e.HasIndex(x => new { x.TrainerId, x.StudentProfileId }).IsUnique();
            e.HasOne(x => x.Trainer).WithMany().HasForeignKey(x => x.TrainerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.StudentProfile).WithMany(x => x.FollowingTrainers).HasForeignKey(x => x.StudentProfileId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SavedTrainer>(e =>
        {
            e.HasIndex(x => new { x.TrainerId, x.StudentProfileId }).IsUnique();
            e.HasOne(x => x.Trainer).WithMany().HasForeignKey(x => x.TrainerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.StudentProfile).WithMany(x => x.SavedTrainers).HasForeignKey(x => x.StudentProfileId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TrainerLead>(e =>
        {
            e.HasIndex(x => x.TrainerId);
            e.HasIndex(x => x.Status);
            e.Property(x => x.Status).HasConversion<int>();
            e.Property(x => x.Source).HasConversion<int>();
            e.HasOne(x => x.Trainer).WithMany().HasForeignKey(x => x.TrainerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.StudentProfile).WithMany().HasForeignKey(x => x.StudentProfileId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
        });

        modelBuilder.Entity<Exercise>(e =>
        {
            e.HasIndex(ex => ex.TrainerId);
            e.Property(ex => ex.Level).HasConversion<int>();
            e.HasOne(ex => ex.Trainer).WithMany(t => t.Exercises).HasForeignKey(ex => ex.TrainerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(ex => ex.ImageMedia).WithMany().HasForeignKey(ex => ex.ImageMediaId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
            e.HasOne(ex => ex.VideoMedia).WithMany().HasForeignKey(ex => ex.VideoMediaId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
        });

        modelBuilder.Entity<Workout>(e =>
        {
            e.HasIndex(w => w.TrainerId);
            e.Property(w => w.Status).HasConversion<int>();
            e.Property(w => w.Level).HasConversion<int>();
            e.HasOne(w => w.Trainer).WithMany(t => t.Workouts).HasForeignKey(w => w.TrainerId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WorkoutExercise>(e =>
        {
            e.HasOne(we => we.Workout).WithMany(w => w.WorkoutExercises).HasForeignKey(we => we.WorkoutId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(we => we.Exercise).WithMany(ex => ex.WorkoutExercises).HasForeignKey(we => we.ExerciseId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StudentWorkoutSchedule>(e =>
        {
            e.HasOne(s => s.Student).WithMany(st => st.WorkoutSchedules).HasForeignKey(s => s.StudentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.Workout).WithMany(w => w.Schedules).HasForeignKey(s => s.WorkoutId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.Trainer).WithMany().HasForeignKey(s => s.TrainerId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Post>(e =>
        {
            e.HasIndex(p => p.TrainerId);
            e.Property(p => p.Status).HasConversion<int>();
            e.Property(p => p.Visibility).HasConversion<int>();
            e.HasOne(p => p.Trainer).WithMany(t => t.Posts).HasForeignKey(p => p.TrainerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.CoverMedia).WithMany().HasForeignKey(p => p.CoverMediaId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
            e.HasOne(p => p.VideoMedia).WithMany().HasForeignKey(p => p.VideoMediaId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
        });

        modelBuilder.Entity<PlatformPlan>(e =>
        {
            e.HasIndex(p => p.Active);
            e.Property(p => p.MonthlyPrice).HasPrecision(18, 2);
        });

        modelBuilder.Entity<PlatformPlanPrice>(e =>
        {
            e.Property(p => p.Price).HasPrecision(18, 2);
            e.Property(p => p.BillingCycle).HasConversion<int>();
            e.HasOne(p => p.PlatformPlan).WithMany(pl => pl.Prices).HasForeignKey(p => p.PlatformPlanId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TrainerSubscription>(e =>
        {
            e.HasIndex(ts => ts.TrainerId);
            e.HasIndex(ts => ts.PlatformPlanId);
            e.Property(ts => ts.Status).HasConversion<int>();
            e.Property(ts => ts.BillingCycle).HasConversion<int>();
            e.HasOne(ts => ts.Trainer).WithMany(t => t.Subscriptions).HasForeignKey(ts => ts.TrainerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(ts => ts.PlatformPlan).WithMany(p => p.Subscriptions).HasForeignKey(ts => ts.PlatformPlanId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(ts => ts.PlatformPlanPrice).WithMany(p => p.Subscriptions).HasForeignKey(ts => ts.PlatformPlanPriceId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
        });

        modelBuilder.Entity<TrainerPayment>(e =>
        {
            e.HasIndex(tp => tp.TrainerSubscriptionId);
            e.Property(tp => tp.Amount).HasPrecision(18, 2);
            e.Property(tp => tp.Status).HasConversion<int>();
            e.HasOne(tp => tp.Trainer).WithMany().HasForeignKey(tp => tp.TrainerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(tp => tp.Subscription).WithMany(ts => ts.Payments).HasForeignKey(tp => tp.TrainerSubscriptionId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PaymentWebhookLog>(e =>
        {
            e.HasIndex(w => new { w.Provider, w.EventId }).IsUnique();
            e.HasIndex(w => w.ResourceId);
        });

        modelBuilder.Entity<TrainerOnboarding>(e =>
        {
            e.HasIndex(o => o.Email).IsUnique();
            e.Property(o => o.Status).HasConversion<int>();
            e.Property(o => o.BillingCycle).HasConversion<int>();
            e.HasOne(o => o.SelectedPlan).WithMany().HasForeignKey(o => o.SelectedPlatformPlanId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
            e.HasOne(o => o.SelectedPlanPrice).WithMany().HasForeignKey(o => o.SelectedPlatformPlanPriceId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
        });

        modelBuilder.Entity<PasswordSetupToken>(e =>
        {
            e.HasIndex(t => t.UserId);
            e.HasIndex(t => t.TokenHash).IsUnique();
            e.HasOne(t => t.User).WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StudentProgress>(e =>
        {
            e.HasIndex(p => p.StudentId);
            e.HasIndex(p => p.TrainerId);
            foreach (var col in new[] { nameof(StudentProgress.Weight), nameof(StudentProgress.Height), nameof(StudentProgress.Chest), nameof(StudentProgress.Waist), nameof(StudentProgress.Abdomen), nameof(StudentProgress.Hip), nameof(StudentProgress.RightArm), nameof(StudentProgress.LeftArm), nameof(StudentProgress.RightThigh), nameof(StudentProgress.LeftThigh) })
                e.Property(col).HasPrecision(5, 2);
            e.Property(p => p.BodyFatPercentage).HasPrecision(5, 2);
            e.Property(p => p.CreatedByRole).HasConversion<int>();
            e.HasOne(p => p.Student).WithMany(s => s.ProgressRecords).HasForeignKey(p => p.StudentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.Trainer).WithMany().HasForeignKey(p => p.TrainerId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StudentProgressPhoto>(e =>
        {
            e.HasIndex(p => p.StudentId);
            e.Property(p => p.CreatedByRole).HasConversion<int>();
            e.HasOne(p => p.Student).WithMany(s => s.ProgressPhotos).HasForeignKey(p => p.StudentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.Trainer).WithMany().HasForeignKey(p => p.TrainerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.MediaAsset).WithMany().HasForeignKey(p => p.MediaAssetId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
        });

        modelBuilder.Entity<StudentWeeklyCheckIn>(e =>
        {
            e.HasIndex(c => new { c.StudentId, c.WeekStartDate }).IsUnique();
            e.HasIndex(c => c.TrainerId);
            e.Property(c => c.Weight).HasPrecision(6, 2);
            e.HasOne(c => c.Student).WithMany(s => s.CheckIns).HasForeignKey(c => c.StudentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.Trainer).WithMany().HasForeignKey(c => c.TrainerId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WorkoutSession>(e =>
        {
            e.HasIndex(ws => ws.StudentId);
            e.HasIndex(ws => ws.TrainerId);
            e.Property(ws => ws.Status).HasConversion<int>();
            e.HasOne(ws => ws.Student).WithMany(s => s.WorkoutSessions).HasForeignKey(ws => ws.StudentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(ws => ws.Trainer).WithMany().HasForeignKey(ws => ws.TrainerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(ws => ws.Workout).WithMany().HasForeignKey(ws => ws.WorkoutId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WorkoutSessionExercise>(e =>
        {
            e.HasOne(wse => wse.WorkoutSession).WithMany(ws => ws.ExerciseSessions).HasForeignKey(wse => wse.WorkoutSessionId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(wse => wse.Exercise).WithMany().HasForeignKey(wse => wse.ExerciseId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ExerciseLibraryItem>(e =>
        {
            e.HasIndex(el => el.IsActive);
            e.Property(el => el.Level).HasConversion<int>();
        });

        modelBuilder.Entity<WorkoutTemplate>(e =>
        {
            e.HasIndex(wt => wt.IsActive);
            e.Property(wt => wt.Level).HasConversion<int>();
        });

        modelBuilder.Entity<WorkoutTemplateExercise>(e =>
        {
            e.HasOne(wte => wte.WorkoutTemplate).WithMany(wt => wt.TemplateExercises).HasForeignKey(wte => wte.WorkoutTemplateId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(wte => wte.ExerciseLibraryItem).WithMany(el => el.WorkoutTemplateExercises).HasForeignKey(wte => wte.ExerciseLibraryItemId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Notification>(e =>
        {
            e.HasIndex(n => n.UserId);
            e.HasIndex(n => n.IsRead);
            e.Property(n => n.Type).HasConversion<int>();
            e.HasOne(n => n.User).WithMany().HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProgressComment>(e =>
        {
            e.HasIndex(pc => pc.StudentProgressId);
            e.HasOne(pc => pc.StudentProgress).WithMany().HasForeignKey(pc => pc.StudentProgressId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
            e.HasOne(pc => pc.WeeklyCheckIn).WithMany(c => c.Comments).HasForeignKey(pc => pc.StudentWeeklyCheckInId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
            e.HasOne(pc => pc.Trainer).WithMany().HasForeignKey(pc => pc.TrainerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(pc => pc.Student).WithMany().HasForeignKey(pc => pc.StudentId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TrainerStudentNote>(e =>
        {
            e.HasIndex(n => new { n.TrainerId, n.StudentId });
            e.HasOne(n => n.Trainer).WithMany().HasForeignKey(n => n.TrainerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(n => n.Student).WithMany().HasForeignKey(n => n.StudentId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StudentAnamnesis>(e =>
        {
            e.HasOne(a => a.Student).WithMany().HasForeignKey(a => a.StudentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.Trainer).WithMany().HasForeignKey(a => a.TrainerId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PlatformFeature>(e =>
        {
            e.HasIndex(f => f.Code).IsUnique();
        });

        modelBuilder.Entity<PlatformPlanFeature>(e =>
        {
            e.HasIndex(pf => new { pf.PlatformPlanId, pf.PlatformFeatureId }).IsUnique();
            e.HasOne(pf => pf.PlatformPlan).WithMany().HasForeignKey(pf => pf.PlatformPlanId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(pf => pf.Feature).WithMany(f => f.PlanFeatures).HasForeignKey(pf => pf.PlatformFeatureId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StudentTestimonial>(e =>
        {
            e.HasOne(t => t.Trainer).WithMany().HasForeignKey(t => t.TrainerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(t => t.Student).WithMany().HasForeignKey(t => t.StudentId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StudentTransformation>(e =>
        {
            e.HasOne(t => t.Trainer).WithMany().HasForeignKey(t => t.TrainerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(t => t.Student).WithMany().HasForeignKey(t => t.StudentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(t => t.BeforeMedia).WithMany().HasForeignKey(t => t.BeforeMediaId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
            e.HasOne(t => t.AfterMedia).WithMany().HasForeignKey(t => t.AfterMediaId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
        });

        modelBuilder.Entity<MediaFile>(e =>
        {
            e.HasIndex(m => m.OwnerUserId);
            e.HasIndex(m => m.TrainerId);
            e.Property(m => m.Category).HasConversion<int>();
            e.Property(m => m.MediaType).HasConversion<int>();
            e.Property(m => m.Provider).HasConversion<int>();
            e.HasOne(m => m.OwnerUser).WithMany().HasForeignKey(m => m.OwnerUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TermsDocument>(e =>
        {
            e.Property(t => t.Type).HasConversion<int>();
        });

        modelBuilder.Entity<UserConsent>(e =>
        {
            e.HasIndex(c => new { c.UserId, c.TermsDocumentId });
            e.HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.TermsDocument).WithMany(t => t.Consents).HasForeignKey(c => c.TermsDocumentId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DataPrivacyRequest>(e =>
        {
            e.HasIndex(r => r.UserId);
            e.Property(r => r.RequestType).HasConversion<int>();
            e.Property(r => r.Status).HasConversion<int>();
            e.HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StudentInvite>(e =>
        {
            e.HasIndex(i => i.StudentId);
            e.Property(i => i.Status).HasConversion<int>();
            e.HasOne(i => i.Student).WithMany().HasForeignKey(i => i.StudentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(i => i.Trainer).WithMany().HasForeignKey(i => i.TrainerId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FeedReaction>(e =>
        {
            e.HasIndex(r => new { r.FeedItemKey, r.UserId, r.ReactionType }).IsUnique();
            e.HasIndex(r => r.UserId);
            e.Property(r => r.ReactionType).HasConversion<int>();
            e.HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FeedSavedItem>(e =>
        {
            e.HasIndex(s => new { s.FeedItemKey, s.UserId }).IsUnique();
            e.HasIndex(s => s.UserId);
            e.HasOne(s => s.User).WithMany().HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FeedComment>(e =>
        {
            e.HasIndex(c => c.FeedItemKey);
            e.HasIndex(c => c.UserId);
            e.HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
