using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class V21_PrivacyLgpdFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DataPrivacyRequests_Users_UserId",
                table: "DataPrivacyRequests");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "DataPrivacyRequests",
                newName: "RejectionReason");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "DataPrivacyRequests",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "AdminNotes",
                table: "DataPrivacyRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "DataPrivacyRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                table: "DataPrivacyRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequesterEmail",
                table: "DataPrivacyRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ConsentDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsentDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataProcessorVendors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataCategories = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CountryOrRegion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HasInternationalTransfer = table.Column<bool>(type: "bit", nullable: false),
                    PrivacyPolicyReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContractualBasisNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProcessorVendors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PrivacyPolicyVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentType = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentMarkdown = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivacyPolicyVersions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SecurityIncidents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DetectedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReportedToAuthorityAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReportedToUsersAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityIncidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SecurityIncidents_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UserDataExports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDataExports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDataExports_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserConsentHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConsentDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<int>(type: "int", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserConsentHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserConsentHistories_ConsentDefinitions_ConsentDefinitionId",
                        column: x => x.ConsentDefinitionId,
                        principalTable: "ConsentDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserConsentHistories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserPrivacyConsents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConsentDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsGranted = table.Column<bool>(type: "bit", nullable: false),
                    GrantedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPrivacyConsents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPrivacyConsents_ConsentDefinitions_ConsentDefinitionId",
                        column: x => x.ConsentDefinitionId,
                        principalTable: "ConsentDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserPrivacyConsents_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserLegalAcceptances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrivacyPolicyVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TermsOfUseVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Source = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLegalAcceptances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLegalAcceptances_PrivacyPolicyVersions_PrivacyPolicyVersionId",
                        column: x => x.PrivacyPolicyVersionId,
                        principalTable: "PrivacyPolicyVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserLegalAcceptances_PrivacyPolicyVersions_TermsOfUseVersionId",
                        column: x => x.TermsOfUseVersionId,
                        principalTable: "PrivacyPolicyVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserLegalAcceptances_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsentDefinitions_Code",
                table: "ConsentDefinitions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataProcessorVendors_IsActive_Name",
                table: "DataProcessorVendors",
                columns: new[] { "IsActive", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_PrivacyPolicyVersions_DocumentType_IsActive",
                table: "PrivacyPolicyVersions",
                columns: new[] { "DocumentType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_PrivacyPolicyVersions_DocumentType_Version",
                table: "PrivacyPolicyVersions",
                columns: new[] { "DocumentType", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidents_CreatedByUserId",
                table: "SecurityIncidents",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidents_Status_Severity_DetectedAt",
                table: "SecurityIncidents",
                columns: new[] { "Status", "Severity", "DetectedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserConsentHistories_ConsentDefinitionId",
                table: "UserConsentHistories",
                column: "ConsentDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserConsentHistories_UserId_ConsentDefinitionId_ChangedAt",
                table: "UserConsentHistories",
                columns: new[] { "UserId", "ConsentDefinitionId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserDataExports_UserId_RequestedAt",
                table: "UserDataExports",
                columns: new[] { "UserId", "RequestedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserLegalAcceptances_PrivacyPolicyVersionId",
                table: "UserLegalAcceptances",
                column: "PrivacyPolicyVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLegalAcceptances_TermsOfUseVersionId",
                table: "UserLegalAcceptances",
                column: "TermsOfUseVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLegalAcceptances_UserId",
                table: "UserLegalAcceptances",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPrivacyConsents_ConsentDefinitionId",
                table: "UserPrivacyConsents",
                column: "ConsentDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPrivacyConsents_UserId_ConsentDefinitionId",
                table: "UserPrivacyConsents",
                columns: new[] { "UserId", "ConsentDefinitionId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DataPrivacyRequests_Users_UserId",
                table: "DataPrivacyRequests",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DataPrivacyRequests_Users_UserId",
                table: "DataPrivacyRequests");

            migrationBuilder.DropTable(
                name: "DataProcessorVendors");

            migrationBuilder.DropTable(
                name: "SecurityIncidents");

            migrationBuilder.DropTable(
                name: "UserConsentHistories");

            migrationBuilder.DropTable(
                name: "UserDataExports");

            migrationBuilder.DropTable(
                name: "UserLegalAcceptances");

            migrationBuilder.DropTable(
                name: "UserPrivacyConsents");

            migrationBuilder.DropTable(
                name: "PrivacyPolicyVersions");

            migrationBuilder.DropTable(
                name: "ConsentDefinitions");

            migrationBuilder.DropColumn(
                name: "AdminNotes",
                table: "DataPrivacyRequests");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "DataPrivacyRequests");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "DataPrivacyRequests");

            migrationBuilder.DropColumn(
                name: "RequesterEmail",
                table: "DataPrivacyRequests");

            migrationBuilder.RenameColumn(
                name: "RejectionReason",
                table: "DataPrivacyRequests",
                newName: "Notes");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "DataPrivacyRequests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DataPrivacyRequests_Users_UserId",
                table: "DataPrivacyRequests",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
