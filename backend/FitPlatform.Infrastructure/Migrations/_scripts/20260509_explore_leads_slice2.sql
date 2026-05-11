SET NOCOUNT ON;

IF OBJECT_ID('dbo.TrainerLeads', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TrainerLeads
    (
        Id uniqueidentifier NOT NULL,
        TrainerId uniqueidentifier NOT NULL,
        StudentProfileId uniqueidentifier NULL,
        Name nvarchar(max) NOT NULL,
        Email nvarchar(max) NOT NULL,
        Phone nvarchar(max) NULL,
        Goal nvarchar(max) NULL,
        Message nvarchar(max) NULL,
        Status int NOT NULL CONSTRAINT DF_TrainerLeads_Status DEFAULT (1),
        Source int NOT NULL CONSTRAINT DF_TrainerLeads_Source DEFAULT (1),
        CreatedAt datetime2 NOT NULL,
        UpdatedAt datetime2 NOT NULL,
        CONSTRAINT PK_TrainerLeads PRIMARY KEY (Id),
        CONSTRAINT FK_TrainerLeads_Trainers_TrainerId FOREIGN KEY (TrainerId) REFERENCES dbo.Trainers(Id),
        CONSTRAINT FK_TrainerLeads_StudentProfiles_StudentProfileId FOREIGN KEY (StudentProfileId) REFERENCES dbo.StudentProfiles(Id)
    );
    CREATE INDEX IX_TrainerLeads_TrainerId ON dbo.TrainerLeads(TrainerId);
    CREATE INDEX IX_TrainerLeads_Status ON dbo.TrainerLeads(Status);
END;
