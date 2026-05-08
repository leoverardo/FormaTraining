using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOnboardingProgressAndExtendedProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MustChangePassword",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "BillingCycle",
                table: "TrainerSubscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "PlatformPlanPriceId",
                table: "TrainerSubscriptions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressNumber",
                table: "Trainers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BirthDate",
                table: "Trainers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CPF",
                table: "Trainers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CREF",
                table: "Trainers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Trainers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Complement",
                table: "Trainers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Instagram",
                table: "Trainers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Neighborhood",
                table: "Trainers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePhotoUrl",
                table: "Trainers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondaryColor",
                table: "Trainers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Specialties",
                table: "Trainers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Trainers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "Trainers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "Trainers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BirthDate",
                table: "Students",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PasswordSetupTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordSetupTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordSetupTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlatformPlanPrices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlatformPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BillingCycle = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformPlanPrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformPlanPrices_PlatformPlans_PlatformPlanId",
                        column: x => x.PlatformPlanId,
                        principalTable: "PlatformPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentProgressPhotos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhotoDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByRole = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentProgressPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentProgressPhotos_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentProgressPhotos_Trainers_TrainerId",
                        column: x => x.TrainerId,
                        principalTable: "Trainers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentProgressRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    Height = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Chest = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Waist = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Abdomen = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Hip = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    RightArm = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    LeftArm = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    RightThigh = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    LeftThigh = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    BodyFatPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProgressDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByRole = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentProgressRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentProgressRecords_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentProgressRecords_Trainers_TrainerId",
                        column: x => x.TrainerId,
                        principalTable: "Trainers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrainerOnboardings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CPF = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BrandName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CREF = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Specialties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Instagram = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProfilePhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrimaryColor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecondaryColor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZipCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Street = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Complement = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Neighborhood = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SelectedPlatformPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SelectedPlatformPlanPriceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BillingCycle = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedTrainerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainerOnboardings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainerOnboardings_PlatformPlanPrices_SelectedPlatformPlanPriceId",
                        column: x => x.SelectedPlatformPlanPriceId,
                        principalTable: "PlatformPlanPrices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrainerOnboardings_PlatformPlans_SelectedPlatformPlanId",
                        column: x => x.SelectedPlatformPlanId,
                        principalTable: "PlatformPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrainerSubscriptions_PlatformPlanPriceId",
                table: "TrainerSubscriptions",
                column: "PlatformPlanPriceId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordSetupTokens_TokenHash",
                table: "PasswordSetupTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PasswordSetupTokens_UserId",
                table: "PasswordSetupTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformPlanPrices_PlatformPlanId",
                table: "PlatformPlanPrices",
                column: "PlatformPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProgressPhotos_StudentId",
                table: "StudentProgressPhotos",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProgressPhotos_TrainerId",
                table: "StudentProgressPhotos",
                column: "TrainerId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProgressRecords_StudentId",
                table: "StudentProgressRecords",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProgressRecords_TrainerId",
                table: "StudentProgressRecords",
                column: "TrainerId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainerOnboardings_Email",
                table: "TrainerOnboardings",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainerOnboardings_SelectedPlatformPlanId",
                table: "TrainerOnboardings",
                column: "SelectedPlatformPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainerOnboardings_SelectedPlatformPlanPriceId",
                table: "TrainerOnboardings",
                column: "SelectedPlatformPlanPriceId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrainerSubscriptions_PlatformPlanPrices_PlatformPlanPriceId",
                table: "TrainerSubscriptions",
                column: "PlatformPlanPriceId",
                principalTable: "PlatformPlanPrices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrainerSubscriptions_PlatformPlanPrices_PlatformPlanPriceId",
                table: "TrainerSubscriptions");

            migrationBuilder.DropTable(
                name: "PasswordSetupTokens");

            migrationBuilder.DropTable(
                name: "StudentProgressPhotos");

            migrationBuilder.DropTable(
                name: "StudentProgressRecords");

            migrationBuilder.DropTable(
                name: "TrainerOnboardings");

            migrationBuilder.DropTable(
                name: "PlatformPlanPrices");

            migrationBuilder.DropIndex(
                name: "IX_TrainerSubscriptions_PlatformPlanPriceId",
                table: "TrainerSubscriptions");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MustChangePassword",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BillingCycle",
                table: "TrainerSubscriptions");

            migrationBuilder.DropColumn(
                name: "PlatformPlanPriceId",
                table: "TrainerSubscriptions");

            migrationBuilder.DropColumn(
                name: "AddressNumber",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "CPF",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "CREF",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "Complement",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "Instagram",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "Neighborhood",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "ProfilePhotoUrl",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "SecondaryColor",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "Specialties",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "Street",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "Students");
        }
    }
}
