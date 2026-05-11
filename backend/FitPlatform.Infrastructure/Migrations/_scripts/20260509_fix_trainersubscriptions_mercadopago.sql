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

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429232408_InitialCreate'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429232408_InitialCreate'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429232408_InitialCreate'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429232408_InitialCreate'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429232408_InitialCreate'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429232408_InitialCreate'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429232408_InitialCreate'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429232408_InitialCreate'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429232408_InitialCreate'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429232408_InitialCreate'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429232408_InitialCreate'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429232408_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Exercises_TrainerId] ON [Exercises] ([TrainerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429232408_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PlatformPlans_Active] ON [PlatformPlans] ([Active]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429232408_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Posts_TrainerId] ON [Posts] ([TrainerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429232408_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Students_TrainerId] ON [Students] ([TrainerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429232408_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Students_UserId] ON [Students] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429232408_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StudentWorkoutSchedules_StudentId] ON [StudentWorkoutSchedules] ([StudentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429232408_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StudentWorkoutSchedules_TrainerId] ON [StudentWorkoutSchedules] ([TrainerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429232408_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StudentWorkoutSchedules_WorkoutId] ON [StudentWorkoutSchedules] ([WorkoutId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429232408_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TrainerPayments_TrainerId] ON [TrainerPayments] ([TrainerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429232408_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TrainerPayments_TrainerSubscriptionId] ON [TrainerPayments] ([TrainerSubscriptionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429232408_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Trainers_UserId] ON [Trainers] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429232408_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TrainerSubscriptions_PlatformPlanId] ON [TrainerSubscriptions] ([PlatformPlanId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429232408_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TrainerSubscriptions_TrainerId] ON [TrainerSubscriptions] ([TrainerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429232408_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429232408_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WorkoutExercises_ExerciseId] ON [WorkoutExercises] ([ExerciseId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429232408_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WorkoutExercises_WorkoutId] ON [WorkoutExercises] ([WorkoutId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429232408_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Workouts_TrainerId] ON [Workouts] ([TrainerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429232408_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260429232408_InitialCreate', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501213227_AddOnboardingProgressAndExtendedProfile'
)
BEGIN
    ALTER TABLE [Users] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501213227_AddOnboardingProgressAndExtendedProfile'
)
BEGIN
    ALTER TABLE [Users] ADD [MustChangePassword] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501213227_AddOnboardingProgressAndExtendedProfile'
)
BEGIN
    ALTER TABLE [TrainerSubscriptions] ADD [BillingCycle] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501213227_AddOnboardingProgressAndExtendedProfile'
)
BEGIN
    ALTER TABLE [TrainerSubscriptions] ADD [PlatformPlanPriceId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501213227_AddOnboardingProgressAndExtendedProfile'
)
BEGIN
    ALTER TABLE [Trainers] ADD [AddressNumber] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501213227_AddOnboardingProgressAndExtendedProfile'
)
BEGIN
    ALTER TABLE [Trainers] ADD [BirthDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501213227_AddOnboardingProgressAndExtendedProfile'
)
BEGIN
    ALTER TABLE [Trainers] ADD [CPF] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501213227_AddOnboardingProgressAndExtendedProfile'
)
BEGIN
    ALTER TABLE [Trainers] ADD [CREF] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501213227_AddOnboardingProgressAndExtendedProfile'
)
BEGIN
    ALTER TABLE [Trainers] ADD [City] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501213227_AddOnboardingProgressAndExtendedProfile'
)
BEGIN
    ALTER TABLE [Trainers] ADD [Complement] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501213227_AddOnboardingProgressAndExtendedProfile'
)
BEGIN
    ALTER TABLE [Trainers] ADD [Instagram] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501213227_AddOnboardingProgressAndExtendedProfile'
)
BEGIN
    ALTER TABLE [Trainers] ADD [Neighborhood] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501213227_AddOnboardingProgressAndExtendedProfile'
)
BEGIN
    ALTER TABLE [Trainers] ADD [ProfilePhotoUrl] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501213227_AddOnboardingProgressAndExtendedProfile'
)
BEGIN
    ALTER TABLE [Trainers] ADD [SecondaryColor] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501213227_AddOnboardingProgressAndExtendedProfile'
)
BEGIN
    ALTER TABLE [Trainers] ADD [Specialties] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501213227_AddOnboardingProgressAndExtendedProfile'
)
BEGIN
    ALTER TABLE [Trainers] ADD [State] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501213227_AddOnboardingProgressAndExtendedProfile'
)
BEGIN
    ALTER TABLE [Trainers] ADD [Street] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501213227_AddOnboardingProgressAndExtendedProfile'
)
BEGIN
    ALTER TABLE [Trainers] ADD [ZipCode] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501213227_AddOnboardingProgressAndExtendedProfile'
)
BEGIN
    ALTER TABLE [Students] ADD [BirthDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501213227_AddOnboardingProgressAndExtendedProfile'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501213227_AddOnboardingProgressAndExtendedProfile'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501213227_AddOnboardingProgressAndExtendedProfile'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501213227_AddOnboardingProgressAndExtendedProfile'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501213227_AddOnboardingProgressAndExtendedProfile'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501213227_AddOnboardingProgressAndExtendedProfile'
)
BEGIN
    CREATE INDEX [IX_TrainerSubscriptions_PlatformPlanPriceId] ON [TrainerSubscriptions] ([PlatformPlanPriceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501213227_AddOnboardingProgressAndExtendedProfile'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PasswordSetupTokens_TokenHash] ON [PasswordSetupTokens] ([TokenHash]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501213227_AddOnboardingProgressAndExtendedProfile'
)
BEGIN
    CREATE INDEX [IX_PasswordSetupTokens_UserId] ON [PasswordSetupTokens] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501213227_AddOnboardingProgressAndExtendedProfile'
)
BEGIN
    CREATE INDEX [IX_PlatformPlanPrices_PlatformPlanId] ON [PlatformPlanPrices] ([PlatformPlanId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501213227_AddOnboardingProgressAndExtendedProfile'
)
BEGIN
    CREATE INDEX [IX_StudentProgressPhotos_StudentId] ON [StudentProgressPhotos] ([StudentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501213227_AddOnboardingProgressAndExtendedProfile'
)
BEGIN
    CREATE INDEX [IX_StudentProgressPhotos_TrainerId] ON [StudentProgressPhotos] ([TrainerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501213227_AddOnboardingProgressAndExtendedProfile'
)
BEGIN
    CREATE INDEX [IX_StudentProgressRecords_StudentId] ON [StudentProgressRecords] ([StudentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501213227_AddOnboardingProgressAndExtendedProfile'
)
BEGIN
    CREATE INDEX [IX_StudentProgressRecords_TrainerId] ON [StudentProgressRecords] ([TrainerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501213227_AddOnboardingProgressAndExtendedProfile'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TrainerOnboardings_Email] ON [TrainerOnboardings] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501213227_AddOnboardingProgressAndExtendedProfile'
)
BEGIN
    CREATE INDEX [IX_TrainerOnboardings_SelectedPlatformPlanId] ON [TrainerOnboardings] ([SelectedPlatformPlanId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501213227_AddOnboardingProgressAndExtendedProfile'
)
BEGIN
    CREATE INDEX [IX_TrainerOnboardings_SelectedPlatformPlanPriceId] ON [TrainerOnboardings] ([SelectedPlatformPlanPriceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501213227_AddOnboardingProgressAndExtendedProfile'
)
BEGIN
    ALTER TABLE [TrainerSubscriptions] ADD CONSTRAINT [FK_TrainerSubscriptions_PlatformPlanPrices_PlatformPlanPriceId] FOREIGN KEY ([PlatformPlanPriceId]) REFERENCES [PlatformPlanPrices] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501213227_AddOnboardingProgressAndExtendedProfile'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260501213227_AddOnboardingProgressAndExtendedProfile', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    ALTER TABLE [Users] ADD [LastActivityAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    ALTER TABLE [Users] ADD [LastLoginAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    ALTER TABLE [Trainers] ADD [BannerUrl] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    ALTER TABLE [Trainers] ADD [PublicDescription] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    ALTER TABLE [Trainers] ADD [PublicHeadline] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    ALTER TABLE [Trainers] ADD [PublicPageEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    ALTER TABLE [Trainers] ADD [PublicSlug] nvarchar(450) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    ALTER TABLE [Trainers] ADD [ShowInstagram] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    ALTER TABLE [Trainers] ADD [ShowTestimonials] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    ALTER TABLE [Trainers] ADD [WelcomeMessage] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    ALTER TABLE [Trainers] ADD [WhatsappNumber] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    ALTER TABLE [Students] ADD [LastMonitoringStatusCalculatedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    ALTER TABLE [Students] ADD [MonitoringStatus] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StudentProgressRecords]') AND [c].[name] = N'Weight');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [StudentProgressRecords] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [StudentProgressRecords] ALTER COLUMN [Weight] decimal(5,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Trainers_PublicSlug] ON [Trainers] ([PublicSlug]) WHERE [PublicSlug] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    CREATE INDEX [IX_DataPrivacyRequests_UserId] ON [DataPrivacyRequests] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    CREATE INDEX [IX_ExerciseLibraryItems_IsActive] ON [ExerciseLibraryItems] ([IsActive]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    CREATE INDEX [IX_MediaFiles_OwnerUserId] ON [MediaFiles] ([OwnerUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    CREATE INDEX [IX_Notifications_IsRead] ON [Notifications] ([IsRead]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PlatformFeatures_Code] ON [PlatformFeatures] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    CREATE INDEX [IX_PlatformPlanFeatures_PlatformFeatureId] ON [PlatformPlanFeatures] ([PlatformFeatureId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PlatformPlanFeatures_PlatformPlanId_PlatformFeatureId] ON [PlatformPlanFeatures] ([PlatformPlanId], [PlatformFeatureId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    CREATE INDEX [IX_ProgressComments_StudentId] ON [ProgressComments] ([StudentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    CREATE INDEX [IX_ProgressComments_StudentProgressId] ON [ProgressComments] ([StudentProgressId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    CREATE INDEX [IX_ProgressComments_StudentWeeklyCheckInId] ON [ProgressComments] ([StudentWeeklyCheckInId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    CREATE INDEX [IX_ProgressComments_TrainerId] ON [ProgressComments] ([TrainerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    CREATE INDEX [IX_StudentAnamnesisRecords_StudentId] ON [StudentAnamnesisRecords] ([StudentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    CREATE INDEX [IX_StudentAnamnesisRecords_TrainerId] ON [StudentAnamnesisRecords] ([TrainerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    CREATE INDEX [IX_StudentInvites_StudentId] ON [StudentInvites] ([StudentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    CREATE INDEX [IX_StudentInvites_TrainerId] ON [StudentInvites] ([TrainerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    CREATE INDEX [IX_StudentTestimonials_StudentId] ON [StudentTestimonials] ([StudentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    CREATE INDEX [IX_StudentTestimonials_TrainerId] ON [StudentTestimonials] ([TrainerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    CREATE INDEX [IX_StudentTransformations_StudentId] ON [StudentTransformations] ([StudentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    CREATE INDEX [IX_StudentTransformations_TrainerId] ON [StudentTransformations] ([TrainerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StudentWeeklyCheckIns_StudentId_WeekStartDate] ON [StudentWeeklyCheckIns] ([StudentId], [WeekStartDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    CREATE INDEX [IX_StudentWeeklyCheckIns_TrainerId] ON [StudentWeeklyCheckIns] ([TrainerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    CREATE INDEX [IX_TrainerStudentNotes_StudentId] ON [TrainerStudentNotes] ([StudentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    CREATE INDEX [IX_TrainerStudentNotes_TrainerId_StudentId] ON [TrainerStudentNotes] ([TrainerId], [StudentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    CREATE INDEX [IX_UserConsents_TermsDocumentId] ON [UserConsents] ([TermsDocumentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    CREATE INDEX [IX_UserConsents_UserId_TermsDocumentId] ON [UserConsents] ([UserId], [TermsDocumentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    CREATE INDEX [IX_WorkoutSessionExercises_ExerciseId] ON [WorkoutSessionExercises] ([ExerciseId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    CREATE INDEX [IX_WorkoutSessionExercises_WorkoutSessionId] ON [WorkoutSessionExercises] ([WorkoutSessionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    CREATE INDEX [IX_WorkoutSessions_StudentId] ON [WorkoutSessions] ([StudentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    CREATE INDEX [IX_WorkoutSessions_TrainerId] ON [WorkoutSessions] ([TrainerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    CREATE INDEX [IX_WorkoutSessions_WorkoutId] ON [WorkoutSessions] ([WorkoutId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    CREATE INDEX [IX_WorkoutTemplateExercises_ExerciseLibraryItemId] ON [WorkoutTemplateExercises] ([ExerciseLibraryItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    CREATE INDEX [IX_WorkoutTemplateExercises_WorkoutTemplateId] ON [WorkoutTemplateExercises] ([WorkoutTemplateId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    CREATE INDEX [IX_WorkoutTemplates_IsActive] ON [WorkoutTemplates] ([IsActive]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260501215500_V3_CheckInSessionsLibraryNotificationsCompliance', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502151646_V4_DryRun'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260502151646_V4_DryRun', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502162252_V5_MediaUploadSystem'
)
BEGIN
    ALTER TABLE [Trainers] ADD [BannerMediaId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502162252_V5_MediaUploadSystem'
)
BEGIN
    ALTER TABLE [Trainers] ADD [LogoMediaId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502162252_V5_MediaUploadSystem'
)
BEGIN
    ALTER TABLE [Trainers] ADD [ProfilePhotoMediaId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502162252_V5_MediaUploadSystem'
)
BEGIN
    ALTER TABLE [StudentProgressPhotos] ADD [MediaFileId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502162252_V5_MediaUploadSystem'
)
BEGIN
    ALTER TABLE [Posts] ADD [CoverMediaId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502162252_V5_MediaUploadSystem'
)
BEGIN
    ALTER TABLE [Posts] ADD [VideoMediaId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502162252_V5_MediaUploadSystem'
)
BEGIN
    ALTER TABLE [MediaFiles] ADD [IsPublic] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502162252_V5_MediaUploadSystem'
)
BEGIN
    ALTER TABLE [MediaFiles] ADD [MediaType] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502162252_V5_MediaUploadSystem'
)
BEGIN
    ALTER TABLE [MediaFiles] ADD [Provider] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502162252_V5_MediaUploadSystem'
)
BEGIN
    ALTER TABLE [MediaFiles] ADD [ProviderKey] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502162252_V5_MediaUploadSystem'
)
BEGIN
    ALTER TABLE [MediaFiles] ADD [ThumbnailUrl] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502162252_V5_MediaUploadSystem'
)
BEGIN
    ALTER TABLE [Exercises] ADD [ImageMediaId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502162252_V5_MediaUploadSystem'
)
BEGIN
    ALTER TABLE [Exercises] ADD [VideoMediaId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502162252_V5_MediaUploadSystem'
)
BEGIN
    CREATE INDEX [IX_Trainers_BannerMediaId] ON [Trainers] ([BannerMediaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502162252_V5_MediaUploadSystem'
)
BEGIN
    CREATE INDEX [IX_Trainers_LogoMediaId] ON [Trainers] ([LogoMediaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502162252_V5_MediaUploadSystem'
)
BEGIN
    CREATE INDEX [IX_Trainers_ProfilePhotoMediaId] ON [Trainers] ([ProfilePhotoMediaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502162252_V5_MediaUploadSystem'
)
BEGIN
    CREATE INDEX [IX_StudentProgressPhotos_MediaFileId] ON [StudentProgressPhotos] ([MediaFileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502162252_V5_MediaUploadSystem'
)
BEGIN
    CREATE INDEX [IX_Posts_CoverMediaId] ON [Posts] ([CoverMediaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502162252_V5_MediaUploadSystem'
)
BEGIN
    CREATE INDEX [IX_Posts_VideoMediaId] ON [Posts] ([VideoMediaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502162252_V5_MediaUploadSystem'
)
BEGIN
    CREATE INDEX [IX_MediaFiles_TrainerId] ON [MediaFiles] ([TrainerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502162252_V5_MediaUploadSystem'
)
BEGIN
    CREATE INDEX [IX_Exercises_ImageMediaId] ON [Exercises] ([ImageMediaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502162252_V5_MediaUploadSystem'
)
BEGIN
    CREATE INDEX [IX_Exercises_VideoMediaId] ON [Exercises] ([VideoMediaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502162252_V5_MediaUploadSystem'
)
BEGIN
    ALTER TABLE [Exercises] ADD CONSTRAINT [FK_Exercises_MediaFiles_ImageMediaId] FOREIGN KEY ([ImageMediaId]) REFERENCES [MediaFiles] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502162252_V5_MediaUploadSystem'
)
BEGIN
    ALTER TABLE [Exercises] ADD CONSTRAINT [FK_Exercises_MediaFiles_VideoMediaId] FOREIGN KEY ([VideoMediaId]) REFERENCES [MediaFiles] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502162252_V5_MediaUploadSystem'
)
BEGIN
    ALTER TABLE [Posts] ADD CONSTRAINT [FK_Posts_MediaFiles_CoverMediaId] FOREIGN KEY ([CoverMediaId]) REFERENCES [MediaFiles] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502162252_V5_MediaUploadSystem'
)
BEGIN
    ALTER TABLE [Posts] ADD CONSTRAINT [FK_Posts_MediaFiles_VideoMediaId] FOREIGN KEY ([VideoMediaId]) REFERENCES [MediaFiles] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502162252_V5_MediaUploadSystem'
)
BEGIN
    ALTER TABLE [StudentProgressPhotos] ADD CONSTRAINT [FK_StudentProgressPhotos_MediaFiles_MediaFileId] FOREIGN KEY ([MediaFileId]) REFERENCES [MediaFiles] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502162252_V5_MediaUploadSystem'
)
BEGIN
    ALTER TABLE [Trainers] ADD CONSTRAINT [FK_Trainers_MediaFiles_BannerMediaId] FOREIGN KEY ([BannerMediaId]) REFERENCES [MediaFiles] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502162252_V5_MediaUploadSystem'
)
BEGIN
    ALTER TABLE [Trainers] ADD CONSTRAINT [FK_Trainers_MediaFiles_LogoMediaId] FOREIGN KEY ([LogoMediaId]) REFERENCES [MediaFiles] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502162252_V5_MediaUploadSystem'
)
BEGIN
    ALTER TABLE [Trainers] ADD CONSTRAINT [FK_Trainers_MediaFiles_ProfilePhotoMediaId] FOREIGN KEY ([ProfilePhotoMediaId]) REFERENCES [MediaFiles] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502162252_V5_MediaUploadSystem'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260502162252_V5_MediaUploadSystem', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506232637_V6_FeedSocialAndPostVisibility'
)
BEGIN
    ALTER TABLE [Posts] ADD [Content] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506232637_V6_FeedSocialAndPostVisibility'
)
BEGIN
    ALTER TABLE [Posts] ADD [PublishedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506232637_V6_FeedSocialAndPostVisibility'
)
BEGIN
    ALTER TABLE [Posts] ADD [Tags] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506232637_V6_FeedSocialAndPostVisibility'
)
BEGIN
    ALTER TABLE [Posts] ADD [Visibility] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506232637_V6_FeedSocialAndPostVisibility'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506232637_V6_FeedSocialAndPostVisibility'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506232637_V6_FeedSocialAndPostVisibility'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506232637_V6_FeedSocialAndPostVisibility'
)
BEGIN
    CREATE INDEX [IX_FeedComments_FeedItemKey] ON [FeedComments] ([FeedItemKey]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506232637_V6_FeedSocialAndPostVisibility'
)
BEGIN
    CREATE INDEX [IX_FeedComments_UserId] ON [FeedComments] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506232637_V6_FeedSocialAndPostVisibility'
)
BEGIN
    CREATE UNIQUE INDEX [IX_FeedReactions_FeedItemKey_UserId_ReactionType] ON [FeedReactions] ([FeedItemKey], [UserId], [ReactionType]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506232637_V6_FeedSocialAndPostVisibility'
)
BEGIN
    CREATE INDEX [IX_FeedReactions_UserId] ON [FeedReactions] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506232637_V6_FeedSocialAndPostVisibility'
)
BEGIN
    CREATE UNIQUE INDEX [IX_FeedSavedItems_FeedItemKey_UserId] ON [FeedSavedItems] ([FeedItemKey], [UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506232637_V6_FeedSocialAndPostVisibility'
)
BEGIN
    CREATE INDEX [IX_FeedSavedItems_UserId] ON [FeedSavedItems] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506232637_V6_FeedSocialAndPostVisibility'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260506232637_V6_FeedSocialAndPostVisibility', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260507000210_V7_MediaAssetCloudinaryPreparation'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260507000210_V7_MediaAssetCloudinaryPreparation', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260507013920_V8_AddTrainerPublicBannerMediaId'
)
BEGIN
    ALTER TABLE [StudentProgressPhotos] DROP CONSTRAINT [FK_StudentProgressPhotos_MediaFiles_MediaFileId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260507013920_V8_AddTrainerPublicBannerMediaId'
)
BEGIN
    EXEC sp_rename N'[StudentProgressPhotos].[MediaFileId]', N'MediaAssetId', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260507013920_V8_AddTrainerPublicBannerMediaId'
)
BEGIN
    EXEC sp_rename N'[StudentProgressPhotos].[IX_StudentProgressPhotos_MediaFileId]', N'IX_StudentProgressPhotos_MediaAssetId', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260507013920_V8_AddTrainerPublicBannerMediaId'
)
BEGIN
    EXEC sp_rename N'[MediaFiles].[Size]', N'SizeInBytes', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260507013920_V8_AddTrainerPublicBannerMediaId'
)
BEGIN
    ALTER TABLE [Trainers] ADD [PublicBannerMediaId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260507013920_V8_AddTrainerPublicBannerMediaId'
)
BEGIN
    ALTER TABLE [StudentTransformations] ADD [AfterMediaId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260507013920_V8_AddTrainerPublicBannerMediaId'
)
BEGIN
    ALTER TABLE [StudentTransformations] ADD [BeforeMediaId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260507013920_V8_AddTrainerPublicBannerMediaId'
)
BEGIN
    ALTER TABLE [MediaFiles] ADD [Folder] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260507013920_V8_AddTrainerPublicBannerMediaId'
)
BEGIN
    ALTER TABLE [MediaFiles] ADD [PublicId] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260507013920_V8_AddTrainerPublicBannerMediaId'
)
BEGIN
    ALTER TABLE [MediaFiles] ADD [SecureUrl] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260507013920_V8_AddTrainerPublicBannerMediaId'
)
BEGIN
    CREATE INDEX [IX_Trainers_PublicBannerMediaId] ON [Trainers] ([PublicBannerMediaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260507013920_V8_AddTrainerPublicBannerMediaId'
)
BEGIN
    CREATE INDEX [IX_StudentTransformations_AfterMediaId] ON [StudentTransformations] ([AfterMediaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260507013920_V8_AddTrainerPublicBannerMediaId'
)
BEGIN
    CREATE INDEX [IX_StudentTransformations_BeforeMediaId] ON [StudentTransformations] ([BeforeMediaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260507013920_V8_AddTrainerPublicBannerMediaId'
)
BEGIN
    ALTER TABLE [StudentProgressPhotos] ADD CONSTRAINT [FK_StudentProgressPhotos_MediaFiles_MediaAssetId] FOREIGN KEY ([MediaAssetId]) REFERENCES [MediaFiles] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260507013920_V8_AddTrainerPublicBannerMediaId'
)
BEGIN
    ALTER TABLE [StudentTransformations] ADD CONSTRAINT [FK_StudentTransformations_MediaFiles_AfterMediaId] FOREIGN KEY ([AfterMediaId]) REFERENCES [MediaFiles] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260507013920_V8_AddTrainerPublicBannerMediaId'
)
BEGIN
    ALTER TABLE [StudentTransformations] ADD CONSTRAINT [FK_StudentTransformations_MediaFiles_BeforeMediaId] FOREIGN KEY ([BeforeMediaId]) REFERENCES [MediaFiles] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260507013920_V8_AddTrainerPublicBannerMediaId'
)
BEGIN
    ALTER TABLE [Trainers] ADD CONSTRAINT [FK_Trainers_MediaFiles_PublicBannerMediaId] FOREIGN KEY ([PublicBannerMediaId]) REFERENCES [MediaFiles] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260507013920_V8_AddTrainerPublicBannerMediaId'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260507013920_V8_AddTrainerPublicBannerMediaId', N'8.0.0');
END;
GO

COMMIT;
GO

