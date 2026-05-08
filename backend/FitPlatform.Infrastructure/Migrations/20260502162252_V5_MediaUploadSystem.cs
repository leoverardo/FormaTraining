using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class V5_MediaUploadSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BannerMediaId",
                table: "Trainers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LogoMediaId",
                table: "Trainers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProfilePhotoMediaId",
                table: "Trainers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MediaFileId",
                table: "StudentProgressPhotos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CoverMediaId",
                table: "Posts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VideoMediaId",
                table: "Posts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "MediaFiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MediaType",
                table: "MediaFiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Provider",
                table: "MediaFiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ProviderKey",
                table: "MediaFiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailUrl",
                table: "MediaFiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ImageMediaId",
                table: "Exercises",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VideoMediaId",
                table: "Exercises",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trainers_BannerMediaId",
                table: "Trainers",
                column: "BannerMediaId");

            migrationBuilder.CreateIndex(
                name: "IX_Trainers_LogoMediaId",
                table: "Trainers",
                column: "LogoMediaId");

            migrationBuilder.CreateIndex(
                name: "IX_Trainers_ProfilePhotoMediaId",
                table: "Trainers",
                column: "ProfilePhotoMediaId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProgressPhotos_MediaFileId",
                table: "StudentProgressPhotos",
                column: "MediaFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_CoverMediaId",
                table: "Posts",
                column: "CoverMediaId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_VideoMediaId",
                table: "Posts",
                column: "VideoMediaId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_TrainerId",
                table: "MediaFiles",
                column: "TrainerId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_ImageMediaId",
                table: "Exercises",
                column: "ImageMediaId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_VideoMediaId",
                table: "Exercises",
                column: "VideoMediaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_MediaFiles_ImageMediaId",
                table: "Exercises",
                column: "ImageMediaId",
                principalTable: "MediaFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_MediaFiles_VideoMediaId",
                table: "Exercises",
                column: "VideoMediaId",
                principalTable: "MediaFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_MediaFiles_CoverMediaId",
                table: "Posts",
                column: "CoverMediaId",
                principalTable: "MediaFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_MediaFiles_VideoMediaId",
                table: "Posts",
                column: "VideoMediaId",
                principalTable: "MediaFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentProgressPhotos_MediaFiles_MediaFileId",
                table: "StudentProgressPhotos",
                column: "MediaFileId",
                principalTable: "MediaFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Trainers_MediaFiles_BannerMediaId",
                table: "Trainers",
                column: "BannerMediaId",
                principalTable: "MediaFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Trainers_MediaFiles_LogoMediaId",
                table: "Trainers",
                column: "LogoMediaId",
                principalTable: "MediaFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Trainers_MediaFiles_ProfilePhotoMediaId",
                table: "Trainers",
                column: "ProfilePhotoMediaId",
                principalTable: "MediaFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_MediaFiles_ImageMediaId",
                table: "Exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_MediaFiles_VideoMediaId",
                table: "Exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_MediaFiles_CoverMediaId",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_MediaFiles_VideoMediaId",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentProgressPhotos_MediaFiles_MediaFileId",
                table: "StudentProgressPhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_Trainers_MediaFiles_BannerMediaId",
                table: "Trainers");

            migrationBuilder.DropForeignKey(
                name: "FK_Trainers_MediaFiles_LogoMediaId",
                table: "Trainers");

            migrationBuilder.DropForeignKey(
                name: "FK_Trainers_MediaFiles_ProfilePhotoMediaId",
                table: "Trainers");

            migrationBuilder.DropIndex(
                name: "IX_Trainers_BannerMediaId",
                table: "Trainers");

            migrationBuilder.DropIndex(
                name: "IX_Trainers_LogoMediaId",
                table: "Trainers");

            migrationBuilder.DropIndex(
                name: "IX_Trainers_ProfilePhotoMediaId",
                table: "Trainers");

            migrationBuilder.DropIndex(
                name: "IX_StudentProgressPhotos_MediaFileId",
                table: "StudentProgressPhotos");

            migrationBuilder.DropIndex(
                name: "IX_Posts_CoverMediaId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_VideoMediaId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_MediaFiles_TrainerId",
                table: "MediaFiles");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_ImageMediaId",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_VideoMediaId",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "BannerMediaId",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "LogoMediaId",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "ProfilePhotoMediaId",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "MediaFileId",
                table: "StudentProgressPhotos");

            migrationBuilder.DropColumn(
                name: "CoverMediaId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "VideoMediaId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "MediaFiles");

            migrationBuilder.DropColumn(
                name: "MediaType",
                table: "MediaFiles");

            migrationBuilder.DropColumn(
                name: "Provider",
                table: "MediaFiles");

            migrationBuilder.DropColumn(
                name: "ProviderKey",
                table: "MediaFiles");

            migrationBuilder.DropColumn(
                name: "ThumbnailUrl",
                table: "MediaFiles");

            migrationBuilder.DropColumn(
                name: "ImageMediaId",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "VideoMediaId",
                table: "Exercises");
        }
    }
}
