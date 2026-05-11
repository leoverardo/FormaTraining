SET NOCOUNT ON;

IF OBJECT_ID('dbo.StudentProfiles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.StudentProfiles
    (
        Id uniqueidentifier NOT NULL,
        UserId uniqueidentifier NOT NULL,
        FullName nvarchar(max) NOT NULL,
        Phone nvarchar(max) NULL,
        BirthDate datetime2 NULL,
        City nvarchar(max) NULL,
        State nvarchar(max) NULL,
        Neighborhood nvarchar(max) NULL,
        Goal nvarchar(max) NULL,
        Interests nvarchar(max) NULL,
        TrainingLevel nvarchar(max) NULL,
        PreferredTrainingMode nvarchar(max) NULL,
        ProfilePhotoUrl nvarchar(max) NULL,
        AccountStatus int NOT NULL CONSTRAINT DF_StudentProfiles_AccountStatus DEFAULT(1),
        CreatedAt datetime2 NOT NULL,
        UpdatedAt datetime2 NOT NULL,
        CONSTRAINT PK_StudentProfiles PRIMARY KEY (Id),
        CONSTRAINT FK_StudentProfiles_Users_UserId FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
    );
    CREATE UNIQUE INDEX IX_StudentProfiles_UserId ON dbo.StudentProfiles(UserId);
END;

IF OBJECT_ID('dbo.TrainerFollowers', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TrainerFollowers
    (
        Id uniqueidentifier NOT NULL,
        TrainerId uniqueidentifier NOT NULL,
        StudentProfileId uniqueidentifier NOT NULL,
        CreatedAt datetime2 NOT NULL,
        UpdatedAt datetime2 NOT NULL,
        CONSTRAINT PK_TrainerFollowers PRIMARY KEY (Id),
        CONSTRAINT FK_TrainerFollowers_Trainers_TrainerId FOREIGN KEY (TrainerId) REFERENCES dbo.Trainers(Id),
        CONSTRAINT FK_TrainerFollowers_StudentProfiles_StudentProfileId FOREIGN KEY (StudentProfileId) REFERENCES dbo.StudentProfiles(Id)
    );
    CREATE UNIQUE INDEX IX_TrainerFollowers_TrainerId_StudentProfileId ON dbo.TrainerFollowers(TrainerId, StudentProfileId);
END;

IF OBJECT_ID('dbo.SavedTrainers', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SavedTrainers
    (
        Id uniqueidentifier NOT NULL,
        TrainerId uniqueidentifier NOT NULL,
        StudentProfileId uniqueidentifier NOT NULL,
        CreatedAt datetime2 NOT NULL,
        UpdatedAt datetime2 NOT NULL,
        CONSTRAINT PK_SavedTrainers PRIMARY KEY (Id),
        CONSTRAINT FK_SavedTrainers_Trainers_TrainerId FOREIGN KEY (TrainerId) REFERENCES dbo.Trainers(Id),
        CONSTRAINT FK_SavedTrainers_StudentProfiles_StudentProfileId FOREIGN KEY (StudentProfileId) REFERENCES dbo.StudentProfiles(Id)
    );
    CREATE UNIQUE INDEX IX_SavedTrainers_TrainerId_StudentProfileId ON dbo.SavedTrainers(TrainerId, StudentProfileId);
END;
