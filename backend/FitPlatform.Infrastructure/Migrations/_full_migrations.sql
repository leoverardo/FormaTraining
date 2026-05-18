IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [PlatformPlans] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [MonthlyPrice] decimal(18,2) NOT NULL,
    [MaxActiveStudents] int NOT NULL,
    [Active] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_PlatformPlans] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Users] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [Email] nvarchar(450) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [Role] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Trainers] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [BrandName] nvarchar(max) NOT NULL,
    [Phone] nvarchar(max) NULL,
    [Bio] nvarchar(max) NULL,
    [LogoUrl] nvarchar(max) NULL,
    [PrimaryColor] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Trainers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Trainers_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Exercises] (
    [Id] uniqueidentifier NOT NULL,
    [TrainerId] uniqueidentifier NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [MuscleGroup] nvarchar(max) NULL,
    [Description] nvarchar(max) NULL,
    [Instructions] nvarchar(max) NULL,
    [ImageUrl] nvarchar(max) NULL,
    [VideoUrl] nvarchar(max) NULL,
    [Level] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Exercises] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Exercises_Trainers_TrainerId] FOREIGN KEY ([TrainerId]) REFERENCES [Trainers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Posts] (
    [Id] uniqueidentifier NOT NULL,
    [TrainerId] uniqueidentifier NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [ImageUrl] nvarchar(max) NULL,
    [VideoUrl] nvarchar(max) NULL,
    [Status] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Posts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Posts_Trainers_TrainerId] FOREIGN KEY ([TrainerId]) REFERENCES [Trainers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Students] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [TrainerId] uniqueidentifier NOT NULL,
    [Phone] nvarchar(max) NULL,
    [Goal] nvarchar(max) NULL,
    [Notes] nvarchar(max) NULL,
    [Status] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Students] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Students_Trainers_TrainerId] FOREIGN KEY ([TrainerId]) REFERENCES [Trainers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Students_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [TrainerSubscriptions] (
    [Id] uniqueidentifier NOT NULL,
    [TrainerId] uniqueidentifier NOT NULL,
    [PlatformPlanId] uniqueidentifier NOT NULL,
    [Status] int NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [MercadoPagoSubscriptionId] nvarchar(max) NULL,
    [LastPaymentStatus] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_TrainerSubscriptions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TrainerSubscriptions_PlatformPlans_PlatformPlanId] FOREIGN KEY ([PlatformPlanId]) REFERENCES [PlatformPlans] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_TrainerSubscriptions_Trainers_TrainerId] FOREIGN KEY ([TrainerId]) REFERENCES [Trainers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Workouts] (
    [Id] uniqueidentifier NOT NULL,
    [TrainerId] uniqueidentifier NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [Goal] nvarchar(max) NULL,
    [Level] int NOT NULL,
    [Description] nvarchar(max) NULL,
    [Status] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Workouts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Workouts_Trainers_TrainerId] FOREIGN KEY ([TrainerId]) REFERENCES [Trainers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [TrainerPayments] (
    [Id] uniqueidentifier NOT NULL,
    [TrainerId] uniqueidentifier NOT NULL,
    [TrainerSubscriptionId] uniqueidentifier NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Status] int NOT NULL,
    [Provider] nvarchar(max) NOT NULL,
    [ProviderPaymentId] nvarchar(max) NULL,
    [PaidAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_TrainerPayments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TrainerPayments_TrainerSubscriptions_TrainerSubscriptionId] FOREIGN KEY ([TrainerSubscriptionId]) REFERENCES [TrainerSubscriptions] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_TrainerPayments_Trainers_TrainerId] FOREIGN KEY ([TrainerId]) REFERENCES [Trainers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [StudentWorkoutSchedules] (
    [Id] uniqueidentifier NOT NULL,
    [TrainerId] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [WorkoutId] uniqueidentifier NOT NULL,
    [DayOfWeek] int NOT NULL,
    [Notes] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_StudentWorkoutSchedules] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_StudentWorkoutSchedules_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StudentWorkoutSchedules_Trainers_TrainerId] FOREIGN KEY ([TrainerId]) REFERENCES [Trainers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StudentWorkoutSchedules_Workouts_WorkoutId] FOREIGN KEY ([WorkoutId]) REFERENCES [Workouts] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [WorkoutExercises] (
    [Id] uniqueidentifier NOT NULL,
    [WorkoutId] uniqueidentifier NOT NULL,
    [ExerciseId] uniqueidentifier NOT NULL,
    [Sets] int NOT NULL,
    [Reps] nvarchar(max) NULL,
    [SuggestedLoad] nvarchar(max) NULL,
    [RestSeconds] int NULL,
    [Notes] nvarchar(max) NULL,
    [OrderIndex] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_WorkoutExercises] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_WorkoutExercises_Exercises_ExerciseId] FOREIGN KEY ([ExerciseId]) REFERENCES [Exercises] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_WorkoutExercises_Workouts_WorkoutId] FOREIGN KEY ([WorkoutId]) REFERENCES [Workouts] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Exercises_TrainerId] ON [Exercises] ([TrainerId]);
GO

CREATE INDEX [IX_PlatformPlans_Active] ON [PlatformPlans] ([Active]);
GO

CREATE INDEX [IX_Posts_TrainerId] ON [Posts] ([TrainerId]);
GO

CREATE INDEX [IX_Students_TrainerId] ON [Students] ([TrainerId]);
GO

CREATE UNIQUE INDEX [IX_Students_UserId] ON [Students] ([UserId]);
GO

CREATE INDEX [IX_StudentWorkoutSchedules_StudentId] ON [StudentWorkoutSchedules] ([StudentId]);
GO

CREATE INDEX [IX_StudentWorkoutSchedules_TrainerId] ON [StudentWorkoutSchedules] ([TrainerId]);
GO

CREATE INDEX [IX_StudentWorkoutSchedules_WorkoutId] ON [StudentWorkoutSchedules] ([WorkoutId]);
GO

CREATE INDEX [IX_TrainerPayments_TrainerId] ON [TrainerPayments] ([TrainerId]);
GO

CREATE INDEX [IX_TrainerPayments_TrainerSubscriptionId] ON [TrainerPayments] ([TrainerSubscriptionId]);
GO

CREATE UNIQUE INDEX [IX_Trainers_UserId] ON [Trainers] ([UserId]);
GO

CREATE INDEX [IX_TrainerSubscriptions_PlatformPlanId] ON [TrainerSubscriptions] ([PlatformPlanId]);
GO

CREATE INDEX [IX_TrainerSubscriptions_TrainerId] ON [TrainerSubscriptions] ([TrainerId]);
GO

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
GO

CREATE INDEX [IX_WorkoutExercises_ExerciseId] ON [WorkoutExercises] ([ExerciseId]);
GO

CREATE INDEX [IX_WorkoutExercises_WorkoutId] ON [WorkoutExercises] ([WorkoutId]);
GO

CREATE INDEX [IX_Workouts_TrainerId] ON [Workouts] ([TrainerId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260429232408_InitialCreate', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Users] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Users] ADD [MustChangePassword] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [TrainerSubscriptions] ADD [BillingCycle] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [TrainerSubscriptions] ADD [PlatformPlanPriceId] uniqueidentifier NULL;
GO

ALTER TABLE [Trainers] ADD [AddressNumber] nvarchar(max) NULL;
GO

ALTER TABLE [Trainers] ADD [BirthDate] datetime2 NULL;
GO

ALTER TABLE [Trainers] ADD [CPF] nvarchar(max) NULL;
GO

ALTER TABLE [Trainers] ADD [CREF] nvarchar(max) NULL;
GO

ALTER TABLE [Trainers] ADD [City] nvarchar(max) NULL;
GO

ALTER TABLE [Trainers] ADD [Complement] nvarchar(max) NULL;
GO

ALTER TABLE [Trainers] ADD [Instagram] nvarchar(max) NULL;
GO

ALTER TABLE [Trainers] ADD [Neighborhood] nvarchar(max) NULL;
GO

ALTER TABLE [Trainers] ADD [ProfilePhotoUrl] nvarchar(max) NULL;
GO

ALTER TABLE [Trainers] ADD [SecondaryColor] nvarchar(max) NULL;
GO

ALTER TABLE [Trainers] ADD [Specialties] nvarchar(max) NULL;
GO

ALTER TABLE [Trainers] ADD [State] nvarchar(max) NULL;
GO

ALTER TABLE [Trainers] ADD [Street] nvarchar(max) NULL;
GO

ALTER TABLE [Trainers] ADD [ZipCode] nvarchar(max) NULL;
GO

ALTER TABLE [Students] ADD [BirthDate] datetime2 NULL;
GO

CREATE TABLE [PasswordSetupTokens] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [TokenHash] nvarchar(450) NOT NULL,
    [ExpiresAt] datetime2 NOT NULL,
    [UsedAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_PasswordSetupTokens] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PasswordSetupTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [PlatformPlanPrices] (
    [Id] uniqueidentifier NOT NULL,
    [PlatformPlanId] uniqueidentifier NOT NULL,
    [BillingCycle] int NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    [Active] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_PlatformPlanPrices] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PlatformPlanPrices_PlatformPlans_PlatformPlanId] FOREIGN KEY ([PlatformPlanId]) REFERENCES [PlatformPlans] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [StudentProgressPhotos] (
    [Id] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [TrainerId] uniqueidentifier NOT NULL,
    [ImageUrl] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [PhotoDate] datetime2 NOT NULL,
    [CreatedByUserId] uniqueidentifier NOT NULL,
    [CreatedByRole] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_StudentProgressPhotos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_StudentProgressPhotos_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StudentProgressPhotos_Trainers_TrainerId] FOREIGN KEY ([TrainerId]) REFERENCES [Trainers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [StudentProgressRecords] (
    [Id] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [TrainerId] uniqueidentifier NOT NULL,
    [Weight] decimal(6,2) NULL,
    [Height] decimal(5,2) NULL,
    [Chest] decimal(5,2) NULL,
    [Waist] decimal(5,2) NULL,
    [Abdomen] decimal(5,2) NULL,
    [Hip] decimal(5,2) NULL,
    [RightArm] decimal(5,2) NULL,
    [LeftArm] decimal(5,2) NULL,
    [RightThigh] decimal(5,2) NULL,
    [LeftThigh] decimal(5,2) NULL,
    [BodyFatPercentage] decimal(5,2) NULL,
    [Notes] nvarchar(max) NULL,
    [ProgressDate] datetime2 NOT NULL,
    [CreatedByUserId] uniqueidentifier NOT NULL,
    [CreatedByRole] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_StudentProgressRecords] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_StudentProgressRecords_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StudentProgressRecords_Trainers_TrainerId] FOREIGN KEY ([TrainerId]) REFERENCES [Trainers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [TrainerOnboardings] (
    [Id] uniqueidentifier NOT NULL,
    [FullName] nvarchar(max) NOT NULL,
    [Email] nvarchar(450) NOT NULL,
    [Phone] nvarchar(max) NULL,
    [CPF] nvarchar(max) NULL,
    [BirthDate] datetime2 NULL,
    [BrandName] nvarchar(max) NULL,
    [CREF] nvarchar(max) NULL,
    [Bio] nvarchar(max) NULL,
    [Specialties] nvarchar(max) NULL,
    [Instagram] nvarchar(max) NULL,
    [ProfilePhotoUrl] nvarchar(max) NULL,
    [LogoUrl] nvarchar(max) NULL,
    [PrimaryColor] nvarchar(max) NULL,
    [SecondaryColor] nvarchar(max) NULL,
    [ZipCode] nvarchar(max) NULL,
    [Street] nvarchar(max) NULL,
    [AddressNumber] nvarchar(max) NULL,
    [Complement] nvarchar(max) NULL,
    [Neighborhood] nvarchar(max) NULL,
    [City] nvarchar(max) NULL,
    [State] nvarchar(max) NULL,
    [SelectedPlatformPlanId] uniqueidentifier NULL,
    [SelectedPlatformPlanPriceId] uniqueidentifier NULL,
    [BillingCycle] int NULL,
    [Status] int NOT NULL,
    [CreatedUserId] uniqueidentifier NULL,
    [CreatedTrainerId] uniqueidentifier NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_TrainerOnboardings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TrainerOnboardings_PlatformPlanPrices_SelectedPlatformPlanPriceId] FOREIGN KEY ([SelectedPlatformPlanPriceId]) REFERENCES [PlatformPlanPrices] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_TrainerOnboardings_PlatformPlans_SelectedPlatformPlanId] FOREIGN KEY ([SelectedPlatformPlanId]) REFERENCES [PlatformPlans] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_TrainerSubscriptions_PlatformPlanPriceId] ON [TrainerSubscriptions] ([PlatformPlanPriceId]);
GO

CREATE UNIQUE INDEX [IX_PasswordSetupTokens_TokenHash] ON [PasswordSetupTokens] ([TokenHash]);
GO

CREATE INDEX [IX_PasswordSetupTokens_UserId] ON [PasswordSetupTokens] ([UserId]);
GO

CREATE INDEX [IX_PlatformPlanPrices_PlatformPlanId] ON [PlatformPlanPrices] ([PlatformPlanId]);
GO

CREATE INDEX [IX_StudentProgressPhotos_StudentId] ON [StudentProgressPhotos] ([StudentId]);
GO

CREATE INDEX [IX_StudentProgressPhotos_TrainerId] ON [StudentProgressPhotos] ([TrainerId]);
GO

CREATE INDEX [IX_StudentProgressRecords_StudentId] ON [StudentProgressRecords] ([StudentId]);
GO

CREATE INDEX [IX_StudentProgressRecords_TrainerId] ON [StudentProgressRecords] ([TrainerId]);
GO

CREATE UNIQUE INDEX [IX_TrainerOnboardings_Email] ON [TrainerOnboardings] ([Email]);
GO

CREATE INDEX [IX_TrainerOnboardings_SelectedPlatformPlanId] ON [TrainerOnboardings] ([SelectedPlatformPlanId]);
GO

CREATE INDEX [IX_TrainerOnboardings_SelectedPlatformPlanPriceId] ON [TrainerOnboardings] ([SelectedPlatformPlanPriceId]);
GO

ALTER TABLE [TrainerSubscriptions] ADD CONSTRAINT [FK_TrainerSubscriptions_PlatformPlanPrices_PlatformPlanPriceId] FOREIGN KEY ([PlatformPlanPriceId]) REFERENCES [PlatformPlanPrices] ([Id]) ON DELETE NO ACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260501213227_AddOnboardingProgressAndExtendedProfile', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Users] ADD [LastActivityAt] datetime2 NULL;
GO

ALTER TABLE [Users] ADD [LastLoginAt] datetime2 NULL;
GO

ALTER TABLE [Trainers] ADD [BannerUrl] nvarchar(max) NULL;
GO

ALTER TABLE [Trainers] ADD [PublicDescription] nvarchar(max) NULL;
GO

ALTER TABLE [Trainers] ADD [PublicHeadline] nvarchar(max) NULL;
GO

ALTER TABLE [Trainers] ADD [PublicPageEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Trainers] ADD [PublicSlug] nvarchar(450) NULL;
GO

ALTER TABLE [Trainers] ADD [ShowInstagram] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Trainers] ADD [ShowTestimonials] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Trainers] ADD [WelcomeMessage] nvarchar(max) NULL;
GO

ALTER TABLE [Trainers] ADD [WhatsappNumber] nvarchar(max) NULL;
GO

ALTER TABLE [Students] ADD [LastMonitoringStatusCalculatedAt] datetime2 NULL;
GO

ALTER TABLE [Students] ADD [MonitoringStatus] int NOT NULL DEFAULT 0;
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StudentProgressRecords]') AND [c].[name] = N'Weight');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [StudentProgressRecords] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [StudentProgressRecords] ALTER COLUMN [Weight] decimal(5,2) NULL;
GO

CREATE TABLE [DataPrivacyRequests] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [RequestType] int NOT NULL,
    [Status] int NOT NULL,
    [RequestedAt] datetime2 NOT NULL,
    [CompletedAt] datetime2 NULL,
    [Notes] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_DataPrivacyRequests] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DataPrivacyRequests_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [ExerciseLibraryItems] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [MuscleGroup] nvarchar(max) NULL,
    [Description] nvarchar(max) NULL,
    [Instructions] nvarchar(max) NULL,
    [ImageUrl] nvarchar(max) NULL,
    [VideoUrl] nvarchar(max) NULL,
    [Level] int NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_ExerciseLibraryItems] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [MediaFiles] (
    [Id] uniqueidentifier NOT NULL,
    [OwnerUserId] uniqueidentifier NOT NULL,
    [TrainerId] uniqueidentifier NULL,
    [StudentId] uniqueidentifier NULL,
    [FileName] nvarchar(max) NOT NULL,
    [OriginalFileName] nvarchar(max) NOT NULL,
    [ContentType] nvarchar(max) NOT NULL,
    [Size] bigint NOT NULL,
    [Url] nvarchar(max) NOT NULL,
    [Category] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_MediaFiles] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MediaFiles_Users_OwnerUserId] FOREIGN KEY ([OwnerUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Notifications] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [TrainerId] uniqueidentifier NULL,
    [StudentId] uniqueidentifier NULL,
    [Title] nvarchar(max) NOT NULL,
    [Message] nvarchar(max) NOT NULL,
    [Type] int NOT NULL,
    [IsRead] bit NOT NULL,
    [ReadAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Notifications_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [PlatformFeatures] (
    [Id] uniqueidentifier NOT NULL,
    [Code] nvarchar(450) NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [Active] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_PlatformFeatures] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [StudentAnamnesisRecords] (
    [Id] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [TrainerId] uniqueidentifier NOT NULL,
    [MainGoal] nvarchar(max) NULL,
    [TrainingExperience] nvarchar(max) NULL,
    [Injuries] nvarchar(max) NULL,
    [HealthRestrictions] nvarchar(max) NULL,
    [AvailableDaysPerWeek] int NULL,
    [TrainingLocation] nvarchar(max) NULL,
    [AvailableEquipment] nvarchar(max) NULL,
    [SleepQuality] int NULL,
    [StressLevel] int NULL,
    [FoodRoutineNotes] nvarchar(max) NULL,
    [AdditionalNotes] nvarchar(max) NULL,
    [SubmittedAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_StudentAnamnesisRecords] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_StudentAnamnesisRecords_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StudentAnamnesisRecords_Trainers_TrainerId] FOREIGN KEY ([TrainerId]) REFERENCES [Trainers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [StudentInvites] (
    [Id] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [TrainerId] uniqueidentifier NOT NULL,
    [Email] nvarchar(max) NOT NULL,
    [TokenHash] nvarchar(max) NOT NULL,
    [Status] int NOT NULL,
    [ExpiresAt] datetime2 NOT NULL,
    [AcceptedAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_StudentInvites] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_StudentInvites_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StudentInvites_Trainers_TrainerId] FOREIGN KEY ([TrainerId]) REFERENCES [Trainers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [StudentTestimonials] (
    [Id] uniqueidentifier NOT NULL,
    [TrainerId] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [Text] nvarchar(max) NOT NULL,
    [Rating] int NULL,
    [ApprovedByStudent] bit NOT NULL,
    [Published] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_StudentTestimonials] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_StudentTestimonials_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StudentTestimonials_Trainers_TrainerId] FOREIGN KEY ([TrainerId]) REFERENCES [Trainers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [StudentTransformations] (
    [Id] uniqueidentifier NOT NULL,
    [TrainerId] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [BeforePhotoUrl] nvarchar(max) NULL,
    [AfterPhotoUrl] nvarchar(max) NULL,
    [Description] nvarchar(max) NULL,
    [ApprovedByStudent] bit NOT NULL,
    [Published] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_StudentTransformations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_StudentTransformations_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StudentTransformations_Trainers_TrainerId] FOREIGN KEY ([TrainerId]) REFERENCES [Trainers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [StudentWeeklyCheckIns] (
    [Id] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [TrainerId] uniqueidentifier NOT NULL,
    [WeekStartDate] datetime2 NOT NULL,
    [WeekEndDate] datetime2 NOT NULL,
    [Weight] decimal(6,2) NULL,
    [MoodLevel] int NULL,
    [EnergyLevel] int NULL,
    [SleepQuality] int NULL,
    [DietAdherence] int NULL,
    [TrainingAdherence] int NULL,
    [CompletedWorkoutsCount] int NULL,
    [HasPain] bit NOT NULL,
    [PainDescription] nvarchar(max) NULL,
    [Notes] nvarchar(max) NULL,
    [PhotoUrl] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_StudentWeeklyCheckIns] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_StudentWeeklyCheckIns_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StudentWeeklyCheckIns_Trainers_TrainerId] FOREIGN KEY ([TrainerId]) REFERENCES [Trainers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [TermsDocuments] (
    [Id] uniqueidentifier NOT NULL,
    [Type] int NOT NULL,
    [Version] nvarchar(max) NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [Active] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_TermsDocuments] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [TrainerStudentNotes] (
    [Id] uniqueidentifier NOT NULL,
    [TrainerId] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [Note] nvarchar(max) NOT NULL,
    [IsPinned] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_TrainerStudentNotes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TrainerStudentNotes_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_TrainerStudentNotes_Trainers_TrainerId] FOREIGN KEY ([TrainerId]) REFERENCES [Trainers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [WorkoutSessions] (
    [Id] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [TrainerId] uniqueidentifier NOT NULL,
    [WorkoutId] uniqueidentifier NOT NULL,
    [ScheduledDate] datetime2 NOT NULL,
    [StartedAt] datetime2 NULL,
    [CompletedAt] datetime2 NULL,
    [Status] int NOT NULL,
    [Notes] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_WorkoutSessions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_WorkoutSessions_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_WorkoutSessions_Trainers_TrainerId] FOREIGN KEY ([TrainerId]) REFERENCES [Trainers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_WorkoutSessions_Workouts_WorkoutId] FOREIGN KEY ([WorkoutId]) REFERENCES [Workouts] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [WorkoutTemplates] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [Goal] nvarchar(max) NULL,
    [Level] int NOT NULL,
    [Description] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_WorkoutTemplates] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [PlatformPlanFeatures] (
    [Id] uniqueidentifier NOT NULL,
    [PlatformPlanId] uniqueidentifier NOT NULL,
    [PlatformFeatureId] uniqueidentifier NOT NULL,
    [Enabled] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_PlatformPlanFeatures] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PlatformPlanFeatures_PlatformFeatures_PlatformFeatureId] FOREIGN KEY ([PlatformFeatureId]) REFERENCES [PlatformFeatures] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PlatformPlanFeatures_PlatformPlans_PlatformPlanId] FOREIGN KEY ([PlatformPlanId]) REFERENCES [PlatformPlans] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [ProgressComments] (
    [Id] uniqueidentifier NOT NULL,
    [StudentProgressId] uniqueidentifier NULL,
    [StudentWeeklyCheckInId] uniqueidentifier NULL,
    [TrainerId] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [Comment] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_ProgressComments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProgressComments_StudentProgressRecords_StudentProgressId] FOREIGN KEY ([StudentProgressId]) REFERENCES [StudentProgressRecords] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ProgressComments_StudentWeeklyCheckIns_StudentWeeklyCheckInId] FOREIGN KEY ([StudentWeeklyCheckInId]) REFERENCES [StudentWeeklyCheckIns] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ProgressComments_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ProgressComments_Trainers_TrainerId] FOREIGN KEY ([TrainerId]) REFERENCES [Trainers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [UserConsents] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [TermsDocumentId] uniqueidentifier NOT NULL,
    [AcceptedAt] datetime2 NOT NULL,
    [IpAddress] nvarchar(max) NULL,
    [UserAgent] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_UserConsents] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserConsents_TermsDocuments_TermsDocumentId] FOREIGN KEY ([TermsDocumentId]) REFERENCES [TermsDocuments] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_UserConsents_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [WorkoutSessionExercises] (
    [Id] uniqueidentifier NOT NULL,
    [WorkoutSessionId] uniqueidentifier NOT NULL,
    [ExerciseId] uniqueidentifier NOT NULL,
    [SetsCompleted] int NULL,
    [RepsCompleted] nvarchar(max) NULL,
    [LoadUsed] nvarchar(max) NULL,
    [DifficultyLevel] int NULL,
    [Notes] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_WorkoutSessionExercises] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_WorkoutSessionExercises_Exercises_ExerciseId] FOREIGN KEY ([ExerciseId]) REFERENCES [Exercises] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_WorkoutSessionExercises_WorkoutSessions_WorkoutSessionId] FOREIGN KEY ([WorkoutSessionId]) REFERENCES [WorkoutSessions] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [WorkoutTemplateExercises] (
    [Id] uniqueidentifier NOT NULL,
    [WorkoutTemplateId] uniqueidentifier NOT NULL,
    [ExerciseLibraryItemId] uniqueidentifier NOT NULL,
    [Sets] int NOT NULL,
    [Reps] nvarchar(max) NULL,
    [SuggestedLoad] nvarchar(max) NULL,
    [RestSeconds] int NULL,
    [Notes] nvarchar(max) NULL,
    [OrderIndex] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_WorkoutTemplateExercises] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_WorkoutTemplateExercises_ExerciseLibraryItems_ExerciseLibraryItemId] FOREIGN KEY ([ExerciseLibraryItemId]) REFERENCES [ExerciseLibraryItems] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_WorkoutTemplateExercises_WorkoutTemplates_WorkoutTemplateId] FOREIGN KEY ([WorkoutTemplateId]) REFERENCES [WorkoutTemplates] ([Id]) ON DELETE CASCADE
);
GO

CREATE UNIQUE INDEX [IX_Trainers_PublicSlug] ON [Trainers] ([PublicSlug]) WHERE [PublicSlug] IS NOT NULL;
GO

CREATE INDEX [IX_DataPrivacyRequests_UserId] ON [DataPrivacyRequests] ([UserId]);
GO

CREATE INDEX [IX_ExerciseLibraryItems_IsActive] ON [ExerciseLibraryItems] ([IsActive]);
GO

CREATE INDEX [IX_MediaFiles_OwnerUserId] ON [MediaFiles] ([OwnerUserId]);
GO

CREATE INDEX [IX_Notifications_IsRead] ON [Notifications] ([IsRead]);
GO

CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);
GO

CREATE UNIQUE INDEX [IX_PlatformFeatures_Code] ON [PlatformFeatures] ([Code]);
GO

CREATE INDEX [IX_PlatformPlanFeatures_PlatformFeatureId] ON [PlatformPlanFeatures] ([PlatformFeatureId]);
GO

CREATE UNIQUE INDEX [IX_PlatformPlanFeatures_PlatformPlanId_PlatformFeatureId] ON [PlatformPlanFeatures] ([PlatformPlanId], [PlatformFeatureId]);
GO

CREATE INDEX [IX_ProgressComments_StudentId] ON [ProgressComments] ([StudentId]);
GO

CREATE INDEX [IX_ProgressComments_StudentProgressId] ON [ProgressComments] ([StudentProgressId]);
GO

CREATE INDEX [IX_ProgressComments_StudentWeeklyCheckInId] ON [ProgressComments] ([StudentWeeklyCheckInId]);
GO

CREATE INDEX [IX_ProgressComments_TrainerId] ON [ProgressComments] ([TrainerId]);
GO

CREATE INDEX [IX_StudentAnamnesisRecords_StudentId] ON [StudentAnamnesisRecords] ([StudentId]);
GO

CREATE INDEX [IX_StudentAnamnesisRecords_TrainerId] ON [StudentAnamnesisRecords] ([TrainerId]);
GO

CREATE INDEX [IX_StudentInvites_StudentId] ON [StudentInvites] ([StudentId]);
GO

CREATE INDEX [IX_StudentInvites_TrainerId] ON [StudentInvites] ([TrainerId]);
GO

CREATE INDEX [IX_StudentTestimonials_StudentId] ON [StudentTestimonials] ([StudentId]);
GO

CREATE INDEX [IX_StudentTestimonials_TrainerId] ON [StudentTestimonials] ([TrainerId]);
GO

CREATE INDEX [IX_StudentTransformations_StudentId] ON [StudentTransformations] ([StudentId]);
GO

CREATE INDEX [IX_StudentTransformations_TrainerId] ON [StudentTransformations] ([TrainerId]);
GO

CREATE UNIQUE INDEX [IX_StudentWeeklyCheckIns_StudentId_WeekStartDate] ON [StudentWeeklyCheckIns] ([StudentId], [WeekStartDate]);
GO

CREATE INDEX [IX_StudentWeeklyCheckIns_TrainerId] ON [StudentWeeklyCheckIns] ([TrainerId]);
GO

CREATE INDEX [IX_TrainerStudentNotes_StudentId] ON [TrainerStudentNotes] ([StudentId]);
GO

CREATE INDEX [IX_TrainerStudentNotes_TrainerId_StudentId] ON [TrainerStudentNotes] ([TrainerId], [StudentId]);
GO

CREATE INDEX [IX_UserConsents_TermsDocumentId] ON [UserConsents] ([TermsDocumentId]);
GO

CREATE INDEX [IX_UserConsents_UserId_TermsDocumentId] ON [UserConsents] ([UserId], [TermsDocumentId]);
GO

CREATE INDEX [IX_WorkoutSessionExercises_ExerciseId] ON [WorkoutSessionExercises] ([ExerciseId]);
GO

CREATE INDEX [IX_WorkoutSessionExercises_WorkoutSessionId] ON [WorkoutSessionExercises] ([WorkoutSessionId]);
GO

CREATE INDEX [IX_WorkoutSessions_StudentId] ON [WorkoutSessions] ([StudentId]);
GO

CREATE INDEX [IX_WorkoutSessions_TrainerId] ON [WorkoutSessions] ([TrainerId]);
GO

CREATE INDEX [IX_WorkoutSessions_WorkoutId] ON [WorkoutSessions] ([WorkoutId]);
GO

CREATE INDEX [IX_WorkoutTemplateExercises_ExerciseLibraryItemId] ON [WorkoutTemplateExercises] ([ExerciseLibraryItemId]);
GO

CREATE INDEX [IX_WorkoutTemplateExercises_WorkoutTemplateId] ON [WorkoutTemplateExercises] ([WorkoutTemplateId]);
GO

CREATE INDEX [IX_WorkoutTemplates_IsActive] ON [WorkoutTemplates] ([IsActive]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260502151646_V4_DryRun', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Trainers] ADD [BannerMediaId] uniqueidentifier NULL;
GO

ALTER TABLE [Trainers] ADD [LogoMediaId] uniqueidentifier NULL;
GO

ALTER TABLE [Trainers] ADD [ProfilePhotoMediaId] uniqueidentifier NULL;
GO

ALTER TABLE [StudentProgressPhotos] ADD [MediaFileId] uniqueidentifier NULL;
GO

ALTER TABLE [Posts] ADD [CoverMediaId] uniqueidentifier NULL;
GO

ALTER TABLE [Posts] ADD [VideoMediaId] uniqueidentifier NULL;
GO

ALTER TABLE [MediaFiles] ADD [IsPublic] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [MediaFiles] ADD [MediaType] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [MediaFiles] ADD [Provider] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [MediaFiles] ADD [ProviderKey] nvarchar(max) NULL;
GO

ALTER TABLE [MediaFiles] ADD [ThumbnailUrl] nvarchar(max) NULL;
GO

ALTER TABLE [Exercises] ADD [ImageMediaId] uniqueidentifier NULL;
GO

ALTER TABLE [Exercises] ADD [VideoMediaId] uniqueidentifier NULL;
GO

CREATE INDEX [IX_Trainers_BannerMediaId] ON [Trainers] ([BannerMediaId]);
GO

CREATE INDEX [IX_Trainers_LogoMediaId] ON [Trainers] ([LogoMediaId]);
GO

CREATE INDEX [IX_Trainers_ProfilePhotoMediaId] ON [Trainers] ([ProfilePhotoMediaId]);
GO

CREATE INDEX [IX_StudentProgressPhotos_MediaFileId] ON [StudentProgressPhotos] ([MediaFileId]);
GO

CREATE INDEX [IX_Posts_CoverMediaId] ON [Posts] ([CoverMediaId]);
GO

CREATE INDEX [IX_Posts_VideoMediaId] ON [Posts] ([VideoMediaId]);
GO

CREATE INDEX [IX_MediaFiles_TrainerId] ON [MediaFiles] ([TrainerId]);
GO

CREATE INDEX [IX_Exercises_ImageMediaId] ON [Exercises] ([ImageMediaId]);
GO

CREATE INDEX [IX_Exercises_VideoMediaId] ON [Exercises] ([VideoMediaId]);
GO

ALTER TABLE [Exercises] ADD CONSTRAINT [FK_Exercises_MediaFiles_ImageMediaId] FOREIGN KEY ([ImageMediaId]) REFERENCES [MediaFiles] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [Exercises] ADD CONSTRAINT [FK_Exercises_MediaFiles_VideoMediaId] FOREIGN KEY ([VideoMediaId]) REFERENCES [MediaFiles] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [Posts] ADD CONSTRAINT [FK_Posts_MediaFiles_CoverMediaId] FOREIGN KEY ([CoverMediaId]) REFERENCES [MediaFiles] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [Posts] ADD CONSTRAINT [FK_Posts_MediaFiles_VideoMediaId] FOREIGN KEY ([VideoMediaId]) REFERENCES [MediaFiles] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [StudentProgressPhotos] ADD CONSTRAINT [FK_StudentProgressPhotos_MediaFiles_MediaFileId] FOREIGN KEY ([MediaFileId]) REFERENCES [MediaFiles] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [Trainers] ADD CONSTRAINT [FK_Trainers_MediaFiles_BannerMediaId] FOREIGN KEY ([BannerMediaId]) REFERENCES [MediaFiles] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [Trainers] ADD CONSTRAINT [FK_Trainers_MediaFiles_LogoMediaId] FOREIGN KEY ([LogoMediaId]) REFERENCES [MediaFiles] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [Trainers] ADD CONSTRAINT [FK_Trainers_MediaFiles_ProfilePhotoMediaId] FOREIGN KEY ([ProfilePhotoMediaId]) REFERENCES [MediaFiles] ([Id]) ON DELETE NO ACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260502162252_V5_MediaUploadSystem', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Posts] ADD [Content] nvarchar(max) NULL;
GO

ALTER TABLE [Posts] ADD [PublishedAt] datetime2 NULL;
GO

ALTER TABLE [Posts] ADD [Tags] nvarchar(max) NULL;
GO

ALTER TABLE [Posts] ADD [Visibility] int NOT NULL DEFAULT 0;
GO

CREATE TABLE [FeedComments] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [TrainerId] uniqueidentifier NULL,
    [StudentId] uniqueidentifier NULL,
    [FeedItemKey] nvarchar(450) NOT NULL,
    [RelatedEntityType] nvarchar(max) NOT NULL,
    [RelatedEntityId] uniqueidentifier NOT NULL,
    [Comment] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_FeedComments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_FeedComments_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [FeedReactions] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [TrainerId] uniqueidentifier NULL,
    [StudentId] uniqueidentifier NULL,
    [FeedItemKey] nvarchar(450) NOT NULL,
    [RelatedEntityType] nvarchar(max) NOT NULL,
    [RelatedEntityId] uniqueidentifier NOT NULL,
    [ReactionType] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_FeedReactions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_FeedReactions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [FeedSavedItems] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [TrainerId] uniqueidentifier NULL,
    [StudentId] uniqueidentifier NULL,
    [FeedItemKey] nvarchar(450) NOT NULL,
    [RelatedEntityType] nvarchar(max) NOT NULL,
    [RelatedEntityId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_FeedSavedItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_FeedSavedItems_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_FeedComments_FeedItemKey] ON [FeedComments] ([FeedItemKey]);
GO

CREATE INDEX [IX_FeedComments_UserId] ON [FeedComments] ([UserId]);
GO

CREATE UNIQUE INDEX [IX_FeedReactions_FeedItemKey_UserId_ReactionType] ON [FeedReactions] ([FeedItemKey], [UserId], [ReactionType]);
GO

CREATE INDEX [IX_FeedReactions_UserId] ON [FeedReactions] ([UserId]);
GO

CREATE UNIQUE INDEX [IX_FeedSavedItems_FeedItemKey_UserId] ON [FeedSavedItems] ([FeedItemKey], [UserId]);
GO

CREATE INDEX [IX_FeedSavedItems_UserId] ON [FeedSavedItems] ([UserId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260506232637_V6_FeedSocialAndPostVisibility', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260507000210_V7_MediaAssetCloudinaryPreparation', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [StudentProgressPhotos] DROP CONSTRAINT [FK_StudentProgressPhotos_MediaFiles_MediaFileId];
GO

EXEC sp_rename N'[StudentProgressPhotos].[MediaFileId]', N'MediaAssetId', N'COLUMN';
GO

EXEC sp_rename N'[StudentProgressPhotos].[IX_StudentProgressPhotos_MediaFileId]', N'IX_StudentProgressPhotos_MediaAssetId', N'INDEX';
GO

EXEC sp_rename N'[MediaFiles].[Size]', N'SizeInBytes', N'COLUMN';
GO

ALTER TABLE [Trainers] ADD [PublicBannerMediaId] uniqueidentifier NULL;
GO

ALTER TABLE [StudentTransformations] ADD [AfterMediaId] uniqueidentifier NULL;
GO

ALTER TABLE [StudentTransformations] ADD [BeforeMediaId] uniqueidentifier NULL;
GO

ALTER TABLE [MediaFiles] ADD [Folder] nvarchar(max) NULL;
GO

ALTER TABLE [MediaFiles] ADD [PublicId] nvarchar(max) NULL;
GO

ALTER TABLE [MediaFiles] ADD [SecureUrl] nvarchar(max) NULL;
GO

CREATE INDEX [IX_Trainers_PublicBannerMediaId] ON [Trainers] ([PublicBannerMediaId]);
GO

CREATE INDEX [IX_StudentTransformations_AfterMediaId] ON [StudentTransformations] ([AfterMediaId]);
GO

CREATE INDEX [IX_StudentTransformations_BeforeMediaId] ON [StudentTransformations] ([BeforeMediaId]);
GO

ALTER TABLE [StudentProgressPhotos] ADD CONSTRAINT [FK_StudentProgressPhotos_MediaFiles_MediaAssetId] FOREIGN KEY ([MediaAssetId]) REFERENCES [MediaFiles] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [StudentTransformations] ADD CONSTRAINT [FK_StudentTransformations_MediaFiles_AfterMediaId] FOREIGN KEY ([AfterMediaId]) REFERENCES [MediaFiles] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [StudentTransformations] ADD CONSTRAINT [FK_StudentTransformations_MediaFiles_BeforeMediaId] FOREIGN KEY ([BeforeMediaId]) REFERENCES [MediaFiles] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [Trainers] ADD CONSTRAINT [FK_Trainers_MediaFiles_PublicBannerMediaId] FOREIGN KEY ([PublicBannerMediaId]) REFERENCES [MediaFiles] ([Id]) ON DELETE NO ACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260507013920_V8_AddTrainerPublicBannerMediaId', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF OBJECT_ID(N'[Conversations]', N'U') IS NULL
BEGIN
    CREATE TABLE [Conversations](
        [Id] uniqueidentifier NOT NULL,
        [TrainerId] uniqueidentifier NOT NULL,
        [StudentId] uniqueidentifier NOT NULL,
        [LastMessageAt] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Conversations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Conversations_Trainers_TrainerId] FOREIGN KEY ([TrainerId]) REFERENCES [Trainers]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Conversations_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students]([Id]) ON DELETE NO ACTION
    );
END
GO

IF OBJECT_ID(N'[ChatMessages]', N'U') IS NULL
BEGIN
    CREATE TABLE [ChatMessages](
        [Id] uniqueidentifier NOT NULL,
        [ConversationId] uniqueidentifier NOT NULL,
        [SenderUserId] uniqueidentifier NOT NULL,
        [SenderRole] int NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        [ReadAt] datetime2 NULL,
        [AttachmentMediaId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ChatMessages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ChatMessages_Conversations_ConversationId] FOREIGN KEY ([ConversationId]) REFERENCES [Conversations]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ChatMessages_Users_SenderUserId] FOREIGN KEY ([SenderUserId]) REFERENCES [Users]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ChatMessages_MediaFiles_AttachmentMediaId] FOREIGN KEY ([AttachmentMediaId]) REFERENCES [MediaFiles]([Id]) ON DELETE NO ACTION
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Conversations_TrainerId_StudentId' AND object_id = OBJECT_ID('Conversations'))
    CREATE UNIQUE INDEX [IX_Conversations_TrainerId_StudentId] ON [Conversations]([TrainerId], [StudentId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Conversations_LastMessageAt' AND object_id = OBJECT_ID('Conversations'))
    CREATE INDEX [IX_Conversations_LastMessageAt] ON [Conversations]([LastMessageAt]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Conversations_StudentId' AND object_id = OBJECT_ID('Conversations'))
    CREATE INDEX [IX_Conversations_StudentId] ON [Conversations]([StudentId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChatMessages_ConversationId_CreatedAt' AND object_id = OBJECT_ID('ChatMessages'))
    CREATE INDEX [IX_ChatMessages_ConversationId_CreatedAt] ON [ChatMessages]([ConversationId], [CreatedAt]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChatMessages_ConversationId_ReadAt' AND object_id = OBJECT_ID('ChatMessages'))
    CREATE INDEX [IX_ChatMessages_ConversationId_ReadAt] ON [ChatMessages]([ConversationId], [ReadAt]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChatMessages_SenderUserId' AND object_id = OBJECT_ID('ChatMessages'))
    CREATE INDEX [IX_ChatMessages_SenderUserId] ON [ChatMessages]([SenderUserId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChatMessages_AttachmentMediaId' AND object_id = OBJECT_ID('ChatMessages'))
    CREATE INDEX [IX_ChatMessages_AttachmentMediaId] ON [ChatMessages]([AttachmentMediaId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260512231037_V10_ChatModuleV2', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP INDEX [IX_WorkoutSessionExercises_WorkoutSessionId] ON [WorkoutSessionExercises];
GO

ALTER TABLE [WorkoutSessionExercises] ADD [CompletedAt] datetime2 NULL;
GO

ALTER TABLE [WorkoutSessionExercises] ADD [IsCompleted] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [WorkoutSessionExercises] ADD [OrderIndex] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [WorkoutSessionExercises] ADD [PrescribedLoad] nvarchar(max) NULL;
GO

ALTER TABLE [WorkoutSessionExercises] ADD [PrescribedReps] nvarchar(max) NULL;
GO

ALTER TABLE [WorkoutSessionExercises] ADD [PrescribedRestSeconds] int NULL;
GO

ALTER TABLE [WorkoutSessionExercises] ADD [PrescribedSets] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [WorkoutSessionExercises] ADD [WorkoutExerciseId] uniqueidentifier NULL;
GO

CREATE TABLE [WorkoutSessionSets] (
    [Id] uniqueidentifier NOT NULL,
    [WorkoutSessionExerciseId] uniqueidentifier NOT NULL,
    [SetNumber] int NOT NULL,
    [PrescribedReps] nvarchar(max) NULL,
    [PrescribedLoad] nvarchar(max) NULL,
    [PrescribedRestSeconds] int NULL,
    [ActualReps] nvarchar(max) NULL,
    [ActualLoad] nvarchar(max) NULL,
    [IsCompleted] bit NOT NULL,
    [CompletedAt] datetime2 NULL,
    [Notes] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_WorkoutSessionSets] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_WorkoutSessionSets_WorkoutSessionExercises_WorkoutSessionExerciseId] FOREIGN KEY ([WorkoutSessionExerciseId]) REFERENCES [WorkoutSessionExercises] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_WorkoutSessionExercises_WorkoutExerciseId] ON [WorkoutSessionExercises] ([WorkoutExerciseId]);
GO

CREATE INDEX [IX_WorkoutSessionExercises_WorkoutSessionId_ExerciseId] ON [WorkoutSessionExercises] ([WorkoutSessionId], [ExerciseId]);
GO

CREATE INDEX [IX_WorkoutSessionExercises_WorkoutSessionId_OrderIndex] ON [WorkoutSessionExercises] ([WorkoutSessionId], [OrderIndex]);
GO

CREATE INDEX [IX_WorkoutSessionSets_WorkoutSessionExerciseId_IsCompleted] ON [WorkoutSessionSets] ([WorkoutSessionExerciseId], [IsCompleted]);
GO

CREATE UNIQUE INDEX [IX_WorkoutSessionSets_WorkoutSessionExerciseId_SetNumber] ON [WorkoutSessionSets] ([WorkoutSessionExerciseId], [SetNumber]);
GO

ALTER TABLE [WorkoutSessionExercises] ADD CONSTRAINT [FK_WorkoutSessionExercises_WorkoutExercises_WorkoutExerciseId] FOREIGN KEY ([WorkoutExerciseId]) REFERENCES [WorkoutExercises] ([Id]) ON DELETE NO ACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260512232344_V11_WorkoutSessionExecutionSets', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [WorkoutSessionExercises] ADD [PrescribedNotes] nvarchar(max) NULL;
GO

CREATE INDEX [IX_WorkoutSessions_StudentId_Status_CompletedAt] ON [WorkoutSessions] ([StudentId], [Status], [CompletedAt]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260512233058_V12_WorkoutExecutionReviewHardening', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [StudentHabits] (
    [Id] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [TrainerId] uniqueidentifier NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [Category] int NOT NULL,
    [Frequency] int NOT NULL,
    [TargetValue] decimal(10,2) NULL,
    [TargetUnit] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_StudentHabits] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_StudentHabits_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_StudentHabits_Trainers_TrainerId] FOREIGN KEY ([TrainerId]) REFERENCES [Trainers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [StudentNutritionGuidances] (
    [Id] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [TrainerId] uniqueidentifier NOT NULL,
    [GuidanceText] nvarchar(max) NOT NULL,
    [StrategicNotes] nvarchar(max) NULL,
    [MediaId] uniqueidentifier NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_StudentNutritionGuidances] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_StudentNutritionGuidances_MediaFiles_MediaId] FOREIGN KEY ([MediaId]) REFERENCES [MediaFiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StudentNutritionGuidances_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_StudentNutritionGuidances_Trainers_TrainerId] FOREIGN KEY ([TrainerId]) REFERENCES [Trainers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [StudentHabitLogs] (
    [Id] uniqueidentifier NOT NULL,
    [HabitId] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [Date] datetime2 NOT NULL,
    [IsCompleted] bit NOT NULL,
    [Value] decimal(10,2) NULL,
    [Note] nvarchar(max) NULL,
    [CompletedAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_StudentHabitLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_StudentHabitLogs_StudentHabits_HabitId] FOREIGN KEY ([HabitId]) REFERENCES [StudentHabits] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_StudentHabitLogs_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE NO ACTION
);
GO

CREATE UNIQUE INDEX [IX_StudentHabitLogs_HabitId_Date] ON [StudentHabitLogs] ([HabitId], [Date]);
GO

CREATE INDEX [IX_StudentHabitLogs_StudentId_Date] ON [StudentHabitLogs] ([StudentId], [Date]);
GO

CREATE INDEX [IX_StudentHabits_StudentId_IsActive] ON [StudentHabits] ([StudentId], [IsActive]);
GO

CREATE INDEX [IX_StudentHabits_TrainerId] ON [StudentHabits] ([TrainerId]);
GO

CREATE INDEX [IX_StudentNutritionGuidances_MediaId] ON [StudentNutritionGuidances] ([MediaId]);
GO

CREATE UNIQUE INDEX [IX_StudentNutritionGuidances_StudentId] ON [StudentNutritionGuidances] ([StudentId]);
GO

CREATE INDEX [IX_StudentNutritionGuidances_TrainerId] ON [StudentNutritionGuidances] ([TrainerId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260512233859_V13_HabitsAndNutritionLightModule', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [StudentHabitLogs] DROP CONSTRAINT [FK_StudentHabitLogs_StudentHabits_HabitId];
GO

DROP INDEX [IX_StudentHabits_StudentId_IsActive] ON [StudentHabits];
GO

ALTER TABLE [StudentHabits] ADD [InactivatedAt] datetime2 NULL;
GO

CREATE INDEX [IX_StudentHabits_StudentId_IsActive_InactivatedAt] ON [StudentHabits] ([StudentId], [IsActive], [InactivatedAt]);
GO

ALTER TABLE [StudentHabitLogs] ADD CONSTRAINT [FK_StudentHabitLogs_StudentHabits_HabitId] FOREIGN KEY ([HabitId]) REFERENCES [StudentHabits] ([Id]) ON DELETE NO ACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260512234548_V14_HabitsTimezoneAndArchivalConsolidation', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Appointments] (
    [Id] uniqueidentifier NOT NULL,
    [TrainerId] uniqueidentifier NOT NULL,
    [StudentId] uniqueidentifier NULL,
    [Title] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [Type] int NOT NULL,
    [Status] int NOT NULL,
    [StartAt] datetime2 NOT NULL,
    [EndAt] datetime2 NOT NULL,
    [Location] nvarchar(max) NULL,
    [OnlineMeetingUrl] nvarchar(max) NULL,
    [CancellationReason] nvarchar(max) NULL,
    [ConfirmationAt] datetime2 NULL,
    [CancelledAt] datetime2 NULL,
    [CompletedAt] datetime2 NULL,
    [RescheduledFromAppointmentId] uniqueidentifier NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Appointments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Appointments_Appointments_RescheduledFromAppointmentId] FOREIGN KEY ([RescheduledFromAppointmentId]) REFERENCES [Appointments] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Appointments_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Appointments_Trainers_TrainerId] FOREIGN KEY ([TrainerId]) REFERENCES [Trainers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_Appointments_RescheduledFromAppointmentId] ON [Appointments] ([RescheduledFromAppointmentId]);
GO

CREATE INDEX [IX_Appointments_StudentId_StartAt] ON [Appointments] ([StudentId], [StartAt]);
GO

CREATE INDEX [IX_Appointments_TrainerId_StartAt_EndAt] ON [Appointments] ([TrainerId], [StartAt], [EndAt]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260513000249_V15_CommercialAppointmentsAgenda', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260513001138_V16_GamificationLight', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF OBJECT_ID(N'[StudentAchievements]', N'U') IS NULL
BEGIN
    CREATE TABLE [StudentAchievements](
        [Id] uniqueidentifier NOT NULL PRIMARY KEY,
        [StudentId] uniqueidentifier NOT NULL,
        [AchievementCode] int NOT NULL,
        [UnlockedAt] datetime2 NOT NULL,
        [MetadataJson] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [FK_StudentAchievements_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students]([Id]) ON DELETE CASCADE
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_StudentAchievements_StudentId_AchievementCode')
    CREATE UNIQUE INDEX [IX_StudentAchievements_StudentId_AchievementCode] ON [StudentAchievements]([StudentId], [AchievementCode]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_StudentAchievements_StudentId_UnlockedAt')
    CREATE INDEX [IX_StudentAchievements_StudentId_UnlockedAt] ON [StudentAchievements]([StudentId], [UnlockedAt]);
IF OBJECT_ID(N'[StudentMonthlyGoals]', N'U') IS NULL
BEGIN
    CREATE TABLE [StudentMonthlyGoals](
        [Id] uniqueidentifier NOT NULL PRIMARY KEY,
        [StudentId] uniqueidentifier NOT NULL,
        [Year] int NOT NULL,
        [Month] int NOT NULL,
        [WorkoutTarget] int NOT NULL,
        [HabitDaysTarget] int NOT NULL,
        [CheckInTarget] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [FK_StudentMonthlyGoals_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students]([Id]) ON DELETE CASCADE
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_StudentMonthlyGoals_StudentId_Year_Month')
    CREATE UNIQUE INDEX [IX_StudentMonthlyGoals_StudentId_Year_Month] ON [StudentMonthlyGoals]([StudentId], [Year], [Month]);
IF OBJECT_ID(N'[TrainerServiceOffers]', N'U') IS NULL
BEGIN
    CREATE TABLE [TrainerServiceOffers](
        [Id] uniqueidentifier NOT NULL PRIMARY KEY,
        [TrainerId] uniqueidentifier NOT NULL,
        [Title] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [Price] decimal(18,2) NOT NULL,
        [BillingType] int NOT NULL,
        [DurationDays] int NULL,
        [IsActive] bit NOT NULL,
        [IsPublic] bit NOT NULL,
        [DisplayOrder] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [FK_TrainerServiceOffers_Trainers_TrainerId] FOREIGN KEY ([TrainerId]) REFERENCES [Trainers]([Id])
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TrainerServiceOffers_TrainerId_IsActive_IsPublic_DisplayOrder')
    CREATE INDEX [IX_TrainerServiceOffers_TrainerId_IsActive_IsPublic_DisplayOrder] ON [TrainerServiceOffers]([TrainerId], [IsActive], [IsPublic], [DisplayOrder]);
IF OBJECT_ID(N'[TrainerServiceOrders]', N'U') IS NULL
BEGIN
    CREATE TABLE [TrainerServiceOrders](
        [Id] uniqueidentifier NOT NULL PRIMARY KEY,
        [TrainerId] uniqueidentifier NOT NULL,
        [ServiceOfferId] uniqueidentifier NOT NULL,
        [StudentId] uniqueidentifier NULL,
        [LeadId] uniqueidentifier NULL,
        [BuyerName] nvarchar(max) NOT NULL,
        [BuyerEmail] nvarchar(450) NOT NULL,
        [BuyerPhone] nvarchar(max) NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Status] int NOT NULL,
        [PaymentProvider] nvarchar(max) NOT NULL,
        [ProviderPaymentId] nvarchar(450) NULL,
        [ProviderPreferenceId] nvarchar(450) NULL,
        [ProviderPaymentStatus] nvarchar(max) NULL,
        [PaidAt] datetime2 NULL,
        [CancelledAt] datetime2 NULL,
        [ExpiresAt] datetime2 NULL,
        [ServiceTitleSnapshot] nvarchar(max) NOT NULL,
        [ServiceDescriptionSnapshot] nvarchar(max) NULL,
        [BillingTypeSnapshot] int NOT NULL,
        [DurationDaysSnapshot] int NULL,
        [RequiresManualStudentLinking] bit NOT NULL,
        [InternalNotes] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [FK_TrainerServiceOrders_Trainers_TrainerId] FOREIGN KEY ([TrainerId]) REFERENCES [Trainers]([Id]),
        CONSTRAINT [FK_TrainerServiceOrders_TrainerServiceOffers_ServiceOfferId] FOREIGN KEY ([ServiceOfferId]) REFERENCES [TrainerServiceOffers]([Id]),
        CONSTRAINT [FK_TrainerServiceOrders_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students]([Id])
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TrainerServiceOrders_TrainerId_Status_CreatedAt')
    CREATE INDEX [IX_TrainerServiceOrders_TrainerId_Status_CreatedAt] ON [TrainerServiceOrders]([TrainerId], [Status], [CreatedAt]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TrainerServiceOrders_ProviderPreferenceId')
    CREATE UNIQUE INDEX [IX_TrainerServiceOrders_ProviderPreferenceId] ON [TrainerServiceOrders]([ProviderPreferenceId]) WHERE [ProviderPreferenceId] IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TrainerServiceOrders_ProviderPaymentId')
    CREATE INDEX [IX_TrainerServiceOrders_ProviderPaymentId] ON [TrainerServiceOrders]([ProviderPaymentId]) WHERE [ProviderPaymentId] IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TrainerServiceOrders_BuyerEmail_TrainerId')
    CREATE INDEX [IX_TrainerServiceOrders_BuyerEmail_TrainerId] ON [TrainerServiceOrders]([BuyerEmail], [TrainerId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TrainerServiceOrders_ServiceOfferId')
    CREATE INDEX [IX_TrainerServiceOrders_ServiceOfferId] ON [TrainerServiceOrders]([ServiceOfferId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TrainerServiceOrders_StudentId')
    CREATE INDEX [IX_TrainerServiceOrders_StudentId] ON [TrainerServiceOrders]([StudentId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260513003531_V17b_TrainerServiceSalesB2C', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_TrainerServiceOrders_TrainerLeads_LeadId'
)
BEGIN
    ALTER TABLE [TrainerServiceOrders] DROP CONSTRAINT [FK_TrainerServiceOrders_TrainerLeads_LeadId];
END
GO

IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_TrainerServiceOrders_LeadId'
      AND object_id = OBJECT_ID(N'[TrainerServiceOrders]')
)
BEGIN
    DROP INDEX [IX_TrainerServiceOrders_LeadId] ON [TrainerServiceOrders];
END
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260513224034_V18_AbacatePayBillingCyclesCoupons', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [TrainerSubscriptions] ADD [AbacatePayCheckoutId] nvarchar(max) NULL;
GO

ALTER TABLE [TrainerSubscriptions] ADD [AbacatePayCustomerId] nvarchar(max) NULL;
GO

ALTER TABLE [TrainerSubscriptions] ADD [AbacatePaySubscriptionId] nvarchar(450) NULL;
GO

ALTER TABLE [TrainerSubscriptions] ADD [BaseAmountInCents] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [TrainerSubscriptions] ADD [CouponCodeApplied] nvarchar(max) NULL;
GO

ALTER TABLE [TrainerSubscriptions] ADD [CouponDiscountAmountInCents] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [TrainerSubscriptions] ADD [CycleDiscountAmountInCents] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [TrainerSubscriptions] ADD [FinalAmountInCents] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [TrainerSubscriptions] ADD [Provider] nvarchar(450) NOT NULL DEFAULT N'';
GO

ALTER TABLE [TrainerPayments] ADD [AbacatePayCheckoutId] nvarchar(450) NULL;
GO

CREATE TABLE [DiscountCoupons] (
    [Id] uniqueidentifier NOT NULL,
    [Code] nvarchar(450) NOT NULL,
    [Description] nvarchar(max) NULL,
    [DiscountType] int NOT NULL,
    [DiscountValue] decimal(18,2) NOT NULL,
    [MaxUsesTotal] int NULL,
    [MaxUsesPerCustomer] int NULL,
    [CurrentUses] int NOT NULL,
    [StartsAt] datetime2 NULL,
    [ExpiresAt] datetime2 NULL,
    [IsActive] bit NOT NULL,
    [AppliesToPlanId] uniqueidentifier NULL,
    [AppliesToBillingCycle] int NULL,
    [MinimumPurchaseAmountInCents] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_DiscountCoupons] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [PlanBillingOptions] (
    [Id] uniqueidentifier NOT NULL,
    [PlatformPlanId] uniqueidentifier NOT NULL,
    [BillingCycle] int NOT NULL,
    [MonthsCount] int NOT NULL,
    [CycleDiscountPercent] decimal(5,2) NOT NULL,
    [BasePriceInCents] int NOT NULL,
    [FinalPriceInCents] int NOT NULL,
    [AbacatePayProductId] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_PlanBillingOptions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PlanBillingOptions_PlatformPlans_PlatformPlanId] FOREIGN KEY ([PlatformPlanId]) REFERENCES [PlatformPlans] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [DiscountCouponRedemptions] (
    [Id] uniqueidentifier NOT NULL,
    [CouponId] uniqueidentifier NOT NULL,
    [TrainerId] uniqueidentifier NULL,
    [SubscriptionId] uniqueidentifier NULL,
    [PaymentId] uniqueidentifier NULL,
    [RedeemedAt] datetime2 NOT NULL,
    [DiscountAmountInCents] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_DiscountCouponRedemptions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DiscountCouponRedemptions_DiscountCoupons_CouponId] FOREIGN KEY ([CouponId]) REFERENCES [DiscountCoupons] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_TrainerSubscriptions_AbacatePaySubscriptionId] ON [TrainerSubscriptions] ([AbacatePaySubscriptionId]);
GO

CREATE INDEX [IX_TrainerSubscriptions_Provider] ON [TrainerSubscriptions] ([Provider]);
GO

CREATE INDEX [IX_TrainerPayments_AbacatePayCheckoutId] ON [TrainerPayments] ([AbacatePayCheckoutId]);
GO

CREATE UNIQUE INDEX [IX_DiscountCouponRedemptions_CouponId_TrainerId_SubscriptionId] ON [DiscountCouponRedemptions] ([CouponId], [TrainerId], [SubscriptionId]) WHERE [TrainerId] IS NOT NULL AND [SubscriptionId] IS NOT NULL;
GO

CREATE UNIQUE INDEX [IX_DiscountCoupons_Code] ON [DiscountCoupons] ([Code]);
GO

CREATE UNIQUE INDEX [IX_PlanBillingOptions_PlatformPlanId_BillingCycle] ON [PlanBillingOptions] ([PlatformPlanId], [BillingCycle]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260513232448_V19_SaasOnboardingCouponFinalization', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [TrainerSubscriptions] ADD [TrainerOnboardingId] uniqueidentifier NULL;
GO

CREATE INDEX [IX_TrainerSubscriptions_TrainerOnboardingId] ON [TrainerSubscriptions] ([TrainerOnboardingId]);
GO

UPDATE pbo
SET pbo.AbacatePayProductId = 'prod_QTnE1M6UHFwqhxpTJhHahrxB'
FROM PlanBillingOptions pbo
INNER JOIN PlatformPlans pp ON pp.Id = pbo.PlatformPlanId
WHERE pp.Name = 'Starter' AND pbo.BillingCycle = 1;
UPDATE pbo
SET pbo.AbacatePayProductId = 'prod_26kYBznyFUgmSYUNZ25dY6kj'
FROM PlanBillingOptions pbo
INNER JOIN PlatformPlans pp ON pp.Id = pbo.PlatformPlanId
WHERE pp.Name = 'Starter' AND pbo.BillingCycle = 3;
UPDATE pbo
SET pbo.AbacatePayProductId = 'prod_bn3nSLyu2mfwePQj4sAu3kxu'
FROM PlanBillingOptions pbo
INNER JOIN PlatformPlans pp ON pp.Id = pbo.PlatformPlanId
WHERE pp.Name = 'Starter' AND pbo.BillingCycle = 4;
UPDATE pbo
SET pbo.AbacatePayProductId = 'prod_UwDswqSBuexnuubjkEjf5tmK'
FROM PlanBillingOptions pbo
INNER JOIN PlatformPlans pp ON pp.Id = pbo.PlatformPlanId
WHERE pp.Name = 'Pro' AND pbo.BillingCycle = 1;
UPDATE pbo
SET pbo.AbacatePayProductId = 'prod_3BcYD4jbJjMpYwrTkGHEZCzL'
FROM PlanBillingOptions pbo
INNER JOIN PlatformPlans pp ON pp.Id = pbo.PlatformPlanId
WHERE pp.Name = 'Pro' AND pbo.BillingCycle = 3;
UPDATE pbo
SET pbo.AbacatePayProductId = 'prod_JYXpgmD1Ch1wcNT3YZKLNq41'
FROM PlanBillingOptions pbo
INNER JOIN PlatformPlans pp ON pp.Id = pbo.PlatformPlanId
WHERE pp.Name = 'Pro' AND pbo.BillingCycle = 4;
UPDATE pbo
SET pbo.AbacatePayProductId = 'prod_hFfqP2jgtwnmyccdShUZfmLM'
FROM PlanBillingOptions pbo
INNER JOIN PlatformPlans pp ON pp.Id = pbo.PlatformPlanId
WHERE pp.Name = 'Growth' AND pbo.BillingCycle = 1;
UPDATE pbo
SET pbo.AbacatePayProductId = 'prod_3pcSXZYRSQSTbkMnraNp4Sdm'
FROM PlanBillingOptions pbo
INNER JOIN PlatformPlans pp ON pp.Id = pbo.PlatformPlanId
WHERE pp.Name = 'Growth' AND pbo.BillingCycle = 3;
UPDATE pbo
SET pbo.AbacatePayProductId = 'prod_mKU52WXcjTJpDRw6xpEJmmDp'
FROM PlanBillingOptions pbo
INNER JOIN PlatformPlans pp ON pp.Id = pbo.PlatformPlanId
WHERE pp.Name = 'Growth' AND pbo.BillingCycle = 4;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260514000419_V20_SetAbacatePayProductIds', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [DataPrivacyRequests] DROP CONSTRAINT [FK_DataPrivacyRequests_Users_UserId];
GO

EXEC sp_rename N'[DataPrivacyRequests].[Notes]', N'RejectionReason', N'COLUMN';
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DataPrivacyRequests]') AND [c].[name] = N'UserId');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [DataPrivacyRequests] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [DataPrivacyRequests] ALTER COLUMN [UserId] uniqueidentifier NULL;
GO

ALTER TABLE [DataPrivacyRequests] ADD [AdminNotes] nvarchar(max) NULL;
GO

ALTER TABLE [DataPrivacyRequests] ADD [Description] nvarchar(max) NULL;
GO

ALTER TABLE [DataPrivacyRequests] ADD [RejectedAt] datetime2 NULL;
GO

ALTER TABLE [DataPrivacyRequests] ADD [RequesterEmail] nvarchar(max) NOT NULL DEFAULT N'';
GO

CREATE TABLE [ConsentDefinitions] (
    [Id] uniqueidentifier NOT NULL,
    [Code] nvarchar(450) NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [IsRequired] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [Category] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_ConsentDefinitions] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [DataProcessorVendors] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Purpose] nvarchar(max) NOT NULL,
    [DataCategories] nvarchar(max) NOT NULL,
    [CountryOrRegion] nvarchar(max) NOT NULL,
    [HasInternationalTransfer] bit NOT NULL,
    [PrivacyPolicyReference] nvarchar(max) NULL,
    [ContractualBasisNotes] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_DataProcessorVendors] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [PrivacyPolicyVersions] (
    [Id] uniqueidentifier NOT NULL,
    [DocumentType] int NOT NULL,
    [Version] nvarchar(450) NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [ContentMarkdown] nvarchar(max) NOT NULL,
    [IsActive] bit NOT NULL,
    [PublishedAt] datetime2 NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_PrivacyPolicyVersions] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [SecurityIncidents] (
    [Id] uniqueidentifier NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [Severity] int NOT NULL,
    [Status] int NOT NULL,
    [DetectedAt] datetime2 NOT NULL,
    [ConfirmedAt] datetime2 NULL,
    [ReportedToAuthorityAt] datetime2 NULL,
    [ReportedToUsersAt] datetime2 NULL,
    [ClosedAt] datetime2 NULL,
    [Notes] nvarchar(max) NULL,
    [CreatedByUserId] uniqueidentifier NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_SecurityIncidents] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SecurityIncidents_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL
);
GO

CREATE TABLE [UserDataExports] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [RequestedAt] datetime2 NOT NULL,
    [GeneratedAt] datetime2 NULL,
    [ExpiresAt] datetime2 NULL,
    [FileUrl] nvarchar(max) NULL,
    [Status] nvarchar(max) NOT NULL,
    [ErrorMessage] nvarchar(max) NULL,
    [PayloadJson] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_UserDataExports] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserDataExports_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [UserConsentHistories] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [ConsentDefinitionId] uniqueidentifier NOT NULL,
    [Action] int NOT NULL,
    [ChangedAt] datetime2 NOT NULL,
    [IpAddress] nvarchar(max) NULL,
    [UserAgent] nvarchar(max) NULL,
    [MetadataJson] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_UserConsentHistories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserConsentHistories_ConsentDefinitions_ConsentDefinitionId] FOREIGN KEY ([ConsentDefinitionId]) REFERENCES [ConsentDefinitions] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_UserConsentHistories_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [UserPrivacyConsents] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [ConsentDefinitionId] uniqueidentifier NOT NULL,
    [IsGranted] bit NOT NULL,
    [GrantedAt] datetime2 NULL,
    [RevokedAt] datetime2 NULL,
    [LastChangedAt] datetime2 NOT NULL,
    [IpAddress] nvarchar(max) NULL,
    [UserAgent] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_UserPrivacyConsents] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserPrivacyConsents_ConsentDefinitions_ConsentDefinitionId] FOREIGN KEY ([ConsentDefinitionId]) REFERENCES [ConsentDefinitions] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_UserPrivacyConsents_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [UserLegalAcceptances] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NULL,
    [Email] nvarchar(max) NULL,
    [PrivacyPolicyVersionId] uniqueidentifier NOT NULL,
    [TermsOfUseVersionId] uniqueidentifier NOT NULL,
    [AcceptedAt] datetime2 NOT NULL,
    [IpAddress] nvarchar(max) NULL,
    [UserAgent] nvarchar(max) NULL,
    [Source] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_UserLegalAcceptances] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserLegalAcceptances_PrivacyPolicyVersions_PrivacyPolicyVersionId] FOREIGN KEY ([PrivacyPolicyVersionId]) REFERENCES [PrivacyPolicyVersions] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_UserLegalAcceptances_PrivacyPolicyVersions_TermsOfUseVersionId] FOREIGN KEY ([TermsOfUseVersionId]) REFERENCES [PrivacyPolicyVersions] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_UserLegalAcceptances_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL
);
GO

CREATE UNIQUE INDEX [IX_ConsentDefinitions_Code] ON [ConsentDefinitions] ([Code]);
GO

CREATE INDEX [IX_DataProcessorVendors_IsActive_Name] ON [DataProcessorVendors] ([IsActive], [Name]);
GO

CREATE INDEX [IX_PrivacyPolicyVersions_DocumentType_IsActive] ON [PrivacyPolicyVersions] ([DocumentType], [IsActive]);
GO

CREATE UNIQUE INDEX [IX_PrivacyPolicyVersions_DocumentType_Version] ON [PrivacyPolicyVersions] ([DocumentType], [Version]);
GO

CREATE INDEX [IX_SecurityIncidents_CreatedByUserId] ON [SecurityIncidents] ([CreatedByUserId]);
GO

CREATE INDEX [IX_SecurityIncidents_Status_Severity_DetectedAt] ON [SecurityIncidents] ([Status], [Severity], [DetectedAt]);
GO

CREATE INDEX [IX_UserConsentHistories_ConsentDefinitionId] ON [UserConsentHistories] ([ConsentDefinitionId]);
GO

CREATE INDEX [IX_UserConsentHistories_UserId_ConsentDefinitionId_ChangedAt] ON [UserConsentHistories] ([UserId], [ConsentDefinitionId], [ChangedAt]);
GO

CREATE INDEX [IX_UserDataExports_UserId_RequestedAt] ON [UserDataExports] ([UserId], [RequestedAt]);
GO

CREATE INDEX [IX_UserLegalAcceptances_PrivacyPolicyVersionId] ON [UserLegalAcceptances] ([PrivacyPolicyVersionId]);
GO

CREATE INDEX [IX_UserLegalAcceptances_TermsOfUseVersionId] ON [UserLegalAcceptances] ([TermsOfUseVersionId]);
GO

CREATE INDEX [IX_UserLegalAcceptances_UserId] ON [UserLegalAcceptances] ([UserId]);
GO

CREATE INDEX [IX_UserPrivacyConsents_ConsentDefinitionId] ON [UserPrivacyConsents] ([ConsentDefinitionId]);
GO

CREATE UNIQUE INDEX [IX_UserPrivacyConsents_UserId_ConsentDefinitionId] ON [UserPrivacyConsents] ([UserId], [ConsentDefinitionId]);
GO

ALTER TABLE [DataPrivacyRequests] ADD CONSTRAINT [FK_DataPrivacyRequests_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260515234913_V21_PrivacyLgpdFoundation', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260518195917_AddTrainerPublicDiscoveryFields', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF COL_LENGTH('Trainers', 'AcceptingStudents') IS NULL
    ALTER TABLE [Trainers] ADD [AcceptingStudents] bit NOT NULL CONSTRAINT [DF_Trainers_AcceptingStudents_Fix] DEFAULT(1);
GO

IF COL_LENGTH('Trainers', 'Latitude') IS NULL
    ALTER TABLE [Trainers] ADD [Latitude] float NULL;
GO

IF COL_LENGTH('Trainers', 'Longitude') IS NULL
    ALTER TABLE [Trainers] ADD [Longitude] float NULL;
GO

IF COL_LENGTH('Trainers', 'PublicSearchEnabled') IS NULL
    ALTER TABLE [Trainers] ADD [PublicSearchEnabled] bit NOT NULL CONSTRAINT [DF_Trainers_PublicSearchEnabled_Fix] DEFAULT(0);
GO

IF COL_LENGTH('Trainers', 'ServiceMode') IS NULL
    ALTER TABLE [Trainers] ADD [ServiceMode] nvarchar(max) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260518201245_FixTrainerPublicDiscoveryColumnsSchema', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF COL_LENGTH('TrainerSubscriptions', 'InitPoint') IS NULL
    ALTER TABLE [TrainerSubscriptions] ADD [InitPoint] nvarchar(max) NULL;
GO

IF COL_LENGTH('TrainerSubscriptions', 'MercadoPagoPayerId') IS NULL
    ALTER TABLE [TrainerSubscriptions] ADD [MercadoPagoPayerId] nvarchar(max) NULL;
GO

IF COL_LENGTH('TrainerPayments', 'ProviderSubscriptionId') IS NULL
    ALTER TABLE [TrainerPayments] ADD [ProviderSubscriptionId] nvarchar(max) NULL;
GO

IF COL_LENGTH('TrainerPayments', 'RawPayload') IS NULL
    ALTER TABLE [TrainerPayments] ADD [RawPayload] nvarchar(max) NULL;
GO

IF OBJECT_ID(N'[PaymentWebhookLogs]', N'U') IS NULL
BEGIN
    CREATE TABLE [PaymentWebhookLogs](
        [Id] uniqueidentifier NOT NULL,
        [Provider] nvarchar(450) NOT NULL,
        [EventId] nvarchar(450) NOT NULL,
        [Type] nvarchar(max) NOT NULL,
        [ResourceId] nvarchar(450) NULL,
        [RawPayload] nvarchar(max) NOT NULL,
        [ProcessedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PaymentWebhookLogs] PRIMARY KEY ([Id])
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_PaymentWebhookLogs_Provider_EventId'
      AND object_id = OBJECT_ID('PaymentWebhookLogs')
)
    CREATE UNIQUE INDEX [IX_PaymentWebhookLogs_Provider_EventId]
    ON [PaymentWebhookLogs] ([Provider], [EventId]);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_PaymentWebhookLogs_ResourceId'
      AND object_id = OBJECT_ID('PaymentWebhookLogs')
)
    CREATE INDEX [IX_PaymentWebhookLogs_ResourceId]
    ON [PaymentWebhookLogs] ([ResourceId]);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Trainers_PublicPageEnabled_PublicSearchEnabled_AcceptingStudents'
      AND object_id = OBJECT_ID('Trainers')
)
    CREATE INDEX [IX_Trainers_PublicPageEnabled_PublicSearchEnabled_AcceptingStudents]
    ON [Trainers] ([PublicPageEnabled], [PublicSearchEnabled], [AcceptingStudents]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260518202007_FixOrphanedV9MigrationsSchema', N'8.0.0');
GO

COMMIT;
GO

