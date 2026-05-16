using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitPlatform.Infrastructure.Migrations
{
    public partial class V17b_TrainerServiceSalesB2C : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
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
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[TrainerServiceOrders]', N'U') IS NOT NULL DROP TABLE [TrainerServiceOrders];
IF OBJECT_ID(N'[TrainerServiceOffers]', N'U') IS NOT NULL DROP TABLE [TrainerServiceOffers];
IF OBJECT_ID(N'[StudentMonthlyGoals]', N'U') IS NOT NULL DROP TABLE [StudentMonthlyGoals];
IF OBJECT_ID(N'[StudentAchievements]', N'U') IS NOT NULL DROP TABLE [StudentAchievements];
");
        }
    }
}
