using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class V6_FeedSocialAndPostVisibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "Posts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Visibility",
                table: "Posts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "FeedComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FeedItemKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RelatedEntityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RelatedEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeedComments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeedReactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FeedItemKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RelatedEntityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RelatedEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReactionType = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedReactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeedReactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeedSavedItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FeedItemKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RelatedEntityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RelatedEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedSavedItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeedSavedItems_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeedComments_FeedItemKey",
                table: "FeedComments",
                column: "FeedItemKey");

            migrationBuilder.CreateIndex(
                name: "IX_FeedComments_UserId",
                table: "FeedComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedReactions_FeedItemKey_UserId_ReactionType",
                table: "FeedReactions",
                columns: new[] { "FeedItemKey", "UserId", "ReactionType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeedReactions_UserId",
                table: "FeedReactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedSavedItems_FeedItemKey_UserId",
                table: "FeedSavedItems",
                columns: new[] { "FeedItemKey", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeedSavedItems_UserId",
                table: "FeedSavedItems",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeedComments");

            migrationBuilder.DropTable(
                name: "FeedReactions");

            migrationBuilder.DropTable(
                name: "FeedSavedItems");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Visibility",
                table: "Posts");
        }
    }
}
