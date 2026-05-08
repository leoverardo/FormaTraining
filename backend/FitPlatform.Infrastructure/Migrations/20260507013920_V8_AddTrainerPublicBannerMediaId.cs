using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class V8_AddTrainerPublicBannerMediaId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentProgressPhotos_MediaFiles_MediaFileId",
                table: "StudentProgressPhotos");

            migrationBuilder.RenameColumn(
                name: "MediaFileId",
                table: "StudentProgressPhotos",
                newName: "MediaAssetId");

            migrationBuilder.RenameIndex(
                name: "IX_StudentProgressPhotos_MediaFileId",
                table: "StudentProgressPhotos",
                newName: "IX_StudentProgressPhotos_MediaAssetId");

            migrationBuilder.RenameColumn(
                name: "Size",
                table: "MediaFiles",
                newName: "SizeInBytes");

            migrationBuilder.AddColumn<Guid>(
                name: "PublicBannerMediaId",
                table: "Trainers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AfterMediaId",
                table: "StudentTransformations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BeforeMediaId",
                table: "StudentTransformations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Folder",
                table: "MediaFiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicId",
                table: "MediaFiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecureUrl",
                table: "MediaFiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trainers_PublicBannerMediaId",
                table: "Trainers",
                column: "PublicBannerMediaId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentTransformations_AfterMediaId",
                table: "StudentTransformations",
                column: "AfterMediaId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentTransformations_BeforeMediaId",
                table: "StudentTransformations",
                column: "BeforeMediaId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentProgressPhotos_MediaFiles_MediaAssetId",
                table: "StudentProgressPhotos",
                column: "MediaAssetId",
                principalTable: "MediaFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentTransformations_MediaFiles_AfterMediaId",
                table: "StudentTransformations",
                column: "AfterMediaId",
                principalTable: "MediaFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentTransformations_MediaFiles_BeforeMediaId",
                table: "StudentTransformations",
                column: "BeforeMediaId",
                principalTable: "MediaFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Trainers_MediaFiles_PublicBannerMediaId",
                table: "Trainers",
                column: "PublicBannerMediaId",
                principalTable: "MediaFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentProgressPhotos_MediaFiles_MediaAssetId",
                table: "StudentProgressPhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentTransformations_MediaFiles_AfterMediaId",
                table: "StudentTransformations");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentTransformations_MediaFiles_BeforeMediaId",
                table: "StudentTransformations");

            migrationBuilder.DropForeignKey(
                name: "FK_Trainers_MediaFiles_PublicBannerMediaId",
                table: "Trainers");

            migrationBuilder.DropIndex(
                name: "IX_Trainers_PublicBannerMediaId",
                table: "Trainers");

            migrationBuilder.DropIndex(
                name: "IX_StudentTransformations_AfterMediaId",
                table: "StudentTransformations");

            migrationBuilder.DropIndex(
                name: "IX_StudentTransformations_BeforeMediaId",
                table: "StudentTransformations");

            migrationBuilder.DropColumn(
                name: "PublicBannerMediaId",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "AfterMediaId",
                table: "StudentTransformations");

            migrationBuilder.DropColumn(
                name: "BeforeMediaId",
                table: "StudentTransformations");

            migrationBuilder.DropColumn(
                name: "Folder",
                table: "MediaFiles");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "MediaFiles");

            migrationBuilder.DropColumn(
                name: "SecureUrl",
                table: "MediaFiles");

            migrationBuilder.RenameColumn(
                name: "MediaAssetId",
                table: "StudentProgressPhotos",
                newName: "MediaFileId");

            migrationBuilder.RenameIndex(
                name: "IX_StudentProgressPhotos_MediaAssetId",
                table: "StudentProgressPhotos",
                newName: "IX_StudentProgressPhotos_MediaFileId");

            migrationBuilder.RenameColumn(
                name: "SizeInBytes",
                table: "MediaFiles",
                newName: "Size");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentProgressPhotos_MediaFiles_MediaFileId",
                table: "StudentProgressPhotos",
                column: "MediaFileId",
                principalTable: "MediaFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
