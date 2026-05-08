using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class V3_CheckInSessionsLibraryNotificationsCompliance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastActivityAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BannerUrl",
                table: "Trainers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicDescription",
                table: "Trainers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicHeadline",
                table: "Trainers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PublicPageEnabled",
                table: "Trainers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PublicSlug",
                table: "Trainers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowInstagram",
                table: "Trainers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowTestimonials",
                table: "Trainers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "WelcomeMessage",
                table: "Trainers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsappNumber",
                table: "Trainers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastMonitoringStatusCalculatedAt",
                table: "Students",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MonitoringStatus",
                table: "Students",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<decimal>(
                name: "Weight",
                table: "StudentProgressRecords",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(6,2)",
                oldPrecision: 6,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "DataPrivacyRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataPrivacyRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataPrivacyRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseLibraryItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MuscleGroup = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Instructions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VideoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Level = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseLibraryItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MediaFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaFiles_Users_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlatformFeatures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformFeatures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudentAnamnesisRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MainGoal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrainingExperience = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Injuries = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HealthRestrictions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AvailableDaysPerWeek = table.Column<int>(type: "int", nullable: true),
                    TrainingLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AvailableEquipment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SleepQuality = table.Column<int>(type: "int", nullable: true),
                    StressLevel = table.Column<int>(type: "int", nullable: true),
                    FoodRoutineNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdditionalNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentAnamnesisRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentAnamnesisRecords_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentAnamnesisRecords_Trainers_TrainerId",
                        column: x => x.TrainerId,
                        principalTable: "Trainers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentInvites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentInvites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentInvites_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentInvites_Trainers_TrainerId",
                        column: x => x.TrainerId,
                        principalTable: "Trainers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentTestimonials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: true),
                    ApprovedByStudent = table.Column<bool>(type: "bit", nullable: false),
                    Published = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentTestimonials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentTestimonials_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentTestimonials_Trainers_TrainerId",
                        column: x => x.TrainerId,
                        principalTable: "Trainers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentTransformations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BeforePhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AfterPhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedByStudent = table.Column<bool>(type: "bit", nullable: false),
                    Published = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentTransformations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentTransformations_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentTransformations_Trainers_TrainerId",
                        column: x => x.TrainerId,
                        principalTable: "Trainers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentWeeklyCheckIns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WeekStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WeekEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    MoodLevel = table.Column<int>(type: "int", nullable: true),
                    EnergyLevel = table.Column<int>(type: "int", nullable: true),
                    SleepQuality = table.Column<int>(type: "int", nullable: true),
                    DietAdherence = table.Column<int>(type: "int", nullable: true),
                    TrainingAdherence = table.Column<int>(type: "int", nullable: true),
                    CompletedWorkoutsCount = table.Column<int>(type: "int", nullable: true),
                    HasPain = table.Column<bool>(type: "bit", nullable: false),
                    PainDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentWeeklyCheckIns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentWeeklyCheckIns_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentWeeklyCheckIns_Trainers_TrainerId",
                        column: x => x.TrainerId,
                        principalTable: "Trainers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TermsDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TermsDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrainerStudentNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPinned = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainerStudentNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainerStudentNotes_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrainerStudentNotes_Trainers_TrainerId",
                        column: x => x.TrainerId,
                        principalTable: "Trainers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkoutId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutSessions_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkoutSessions_Trainers_TrainerId",
                        column: x => x.TrainerId,
                        principalTable: "Trainers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkoutSessions_Workouts_WorkoutId",
                        column: x => x.WorkoutId,
                        principalTable: "Workouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Goal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlatformPlanFeatures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlatformPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlatformFeatureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformPlanFeatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformPlanFeatures_PlatformFeatures_PlatformFeatureId",
                        column: x => x.PlatformFeatureId,
                        principalTable: "PlatformFeatures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlatformPlanFeatures_PlatformPlans_PlatformPlanId",
                        column: x => x.PlatformPlanId,
                        principalTable: "PlatformPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProgressComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentProgressId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StudentWeeklyCheckInId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TrainerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgressComments_StudentProgressRecords_StudentProgressId",
                        column: x => x.StudentProgressId,
                        principalTable: "StudentProgressRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgressComments_StudentWeeklyCheckIns_StudentWeeklyCheckInId",
                        column: x => x.StudentWeeklyCheckInId,
                        principalTable: "StudentWeeklyCheckIns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgressComments_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgressComments_Trainers_TrainerId",
                        column: x => x.TrainerId,
                        principalTable: "Trainers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserConsents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TermsDocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserConsents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserConsents_TermsDocuments_TermsDocumentId",
                        column: x => x.TermsDocumentId,
                        principalTable: "TermsDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserConsents_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutSessionExercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkoutSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SetsCompleted = table.Column<int>(type: "int", nullable: true),
                    RepsCompleted = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoadUsed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DifficultyLevel = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutSessionExercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutSessionExercises_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkoutSessionExercises_WorkoutSessions_WorkoutSessionId",
                        column: x => x.WorkoutSessionId,
                        principalTable: "WorkoutSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutTemplateExercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkoutTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExerciseLibraryItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Sets = table.Column<int>(type: "int", nullable: false),
                    Reps = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SuggestedLoad = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RestSeconds = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutTemplateExercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutTemplateExercises_ExerciseLibraryItems_ExerciseLibraryItemId",
                        column: x => x.ExerciseLibraryItemId,
                        principalTable: "ExerciseLibraryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkoutTemplateExercises_WorkoutTemplates_WorkoutTemplateId",
                        column: x => x.WorkoutTemplateId,
                        principalTable: "WorkoutTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Trainers_PublicSlug",
                table: "Trainers",
                column: "PublicSlug",
                unique: true,
                filter: "[PublicSlug] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DataPrivacyRequests_UserId",
                table: "DataPrivacyRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseLibraryItems_IsActive",
                table: "ExerciseLibraryItems",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_OwnerUserId",
                table: "MediaFiles",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IsRead",
                table: "Notifications",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformFeatures_Code",
                table: "PlatformFeatures",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlatformPlanFeatures_PlatformFeatureId",
                table: "PlatformPlanFeatures",
                column: "PlatformFeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformPlanFeatures_PlatformPlanId_PlatformFeatureId",
                table: "PlatformPlanFeatures",
                columns: new[] { "PlatformPlanId", "PlatformFeatureId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProgressComments_StudentId",
                table: "ProgressComments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressComments_StudentProgressId",
                table: "ProgressComments",
                column: "StudentProgressId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressComments_StudentWeeklyCheckInId",
                table: "ProgressComments",
                column: "StudentWeeklyCheckInId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressComments_TrainerId",
                table: "ProgressComments",
                column: "TrainerId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAnamnesisRecords_StudentId",
                table: "StudentAnamnesisRecords",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAnamnesisRecords_TrainerId",
                table: "StudentAnamnesisRecords",
                column: "TrainerId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentInvites_StudentId",
                table: "StudentInvites",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentInvites_TrainerId",
                table: "StudentInvites",
                column: "TrainerId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentTestimonials_StudentId",
                table: "StudentTestimonials",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentTestimonials_TrainerId",
                table: "StudentTestimonials",
                column: "TrainerId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentTransformations_StudentId",
                table: "StudentTransformations",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentTransformations_TrainerId",
                table: "StudentTransformations",
                column: "TrainerId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentWeeklyCheckIns_StudentId_WeekStartDate",
                table: "StudentWeeklyCheckIns",
                columns: new[] { "StudentId", "WeekStartDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentWeeklyCheckIns_TrainerId",
                table: "StudentWeeklyCheckIns",
                column: "TrainerId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainerStudentNotes_StudentId",
                table: "TrainerStudentNotes",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainerStudentNotes_TrainerId_StudentId",
                table: "TrainerStudentNotes",
                columns: new[] { "TrainerId", "StudentId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserConsents_TermsDocumentId",
                table: "UserConsents",
                column: "TermsDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserConsents_UserId_TermsDocumentId",
                table: "UserConsents",
                columns: new[] { "UserId", "TermsDocumentId" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessionExercises_ExerciseId",
                table: "WorkoutSessionExercises",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessionExercises_WorkoutSessionId",
                table: "WorkoutSessionExercises",
                column: "WorkoutSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessions_StudentId",
                table: "WorkoutSessions",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessions_TrainerId",
                table: "WorkoutSessions",
                column: "TrainerId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessions_WorkoutId",
                table: "WorkoutSessions",
                column: "WorkoutId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutTemplateExercises_ExerciseLibraryItemId",
                table: "WorkoutTemplateExercises",
                column: "ExerciseLibraryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutTemplateExercises_WorkoutTemplateId",
                table: "WorkoutTemplateExercises",
                column: "WorkoutTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutTemplates_IsActive",
                table: "WorkoutTemplates",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataPrivacyRequests");

            migrationBuilder.DropTable(
                name: "MediaFiles");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PlatformPlanFeatures");

            migrationBuilder.DropTable(
                name: "ProgressComments");

            migrationBuilder.DropTable(
                name: "StudentAnamnesisRecords");

            migrationBuilder.DropTable(
                name: "StudentInvites");

            migrationBuilder.DropTable(
                name: "StudentTestimonials");

            migrationBuilder.DropTable(
                name: "StudentTransformations");

            migrationBuilder.DropTable(
                name: "TrainerStudentNotes");

            migrationBuilder.DropTable(
                name: "UserConsents");

            migrationBuilder.DropTable(
                name: "WorkoutSessionExercises");

            migrationBuilder.DropTable(
                name: "WorkoutTemplateExercises");

            migrationBuilder.DropTable(
                name: "PlatformFeatures");

            migrationBuilder.DropTable(
                name: "StudentWeeklyCheckIns");

            migrationBuilder.DropTable(
                name: "TermsDocuments");

            migrationBuilder.DropTable(
                name: "WorkoutSessions");

            migrationBuilder.DropTable(
                name: "ExerciseLibraryItems");

            migrationBuilder.DropTable(
                name: "WorkoutTemplates");

            migrationBuilder.DropIndex(
                name: "IX_Trainers_PublicSlug",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "LastActivityAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastLoginAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BannerUrl",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "PublicDescription",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "PublicHeadline",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "PublicPageEnabled",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "PublicSlug",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "ShowInstagram",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "ShowTestimonials",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "WelcomeMessage",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "WhatsappNumber",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "LastMonitoringStatusCalculatedAt",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "MonitoringStatus",
                table: "Students");

            migrationBuilder.AlterColumn<decimal>(
                name: "Weight",
                table: "StudentProgressRecords",
                type: "decimal(6,2)",
                precision: 6,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2,
                oldNullable: true);
        }
    }
}
