using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitPlatform.Infrastructure.Migrations
{
    public partial class V10_ChatModuleV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
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
");

            migrationBuilder.Sql(@"
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
");

            migrationBuilder.Sql(@"
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
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[ChatMessages]', N'U') IS NOT NULL DROP TABLE [ChatMessages];
IF OBJECT_ID(N'[Conversations]', N'U') IS NOT NULL DROP TABLE [Conversations];
");
        }
    }
}

