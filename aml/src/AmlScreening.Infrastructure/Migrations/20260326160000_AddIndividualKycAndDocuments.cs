using System;
using AmlScreening.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmlScreening.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260326160000_AddIndividualKycAndDocuments")]
    public partial class AddIndividualKycAndDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IndividualKyc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),

                    ApplicantName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ApplicantAliases = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ApplicantMobileNo = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ApplicantNationalityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApplicantDualNationality = table.Column<bool>(type: "bit", nullable: true),
                    ApplicantGenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApplicantDateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApplicantResidenceStatus = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ApplicantEmirate = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ApplicantCountryOfBirth = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ApplicantCity = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ApplicantEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ApplicantResidentialAddress = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ApplicantOfficeNoBuildingNameStreetArea = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ApplicantPOBox = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ApplicantCustomerRelationship = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ApplicantPreferredChannel = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ApplicantProductType = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ApplicantIndustryType = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ApplicantOccupationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),

                    ApplicantSourceOfFundsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApplicantSourceOfFundsComments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ApplicantIsProofOfSourceFundsObtained = table.Column<bool>(type: "bit", nullable: true),
                    ApplicantSourceOfFundsProofComments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),

                    ApplicantSourceOfWealth = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ApplicantSourceOfWealthComments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ApplicantIsProofOfSourceWealthObtained = table.Column<bool>(type: "bit", nullable: true),
                    ApplicantSourceOfWealthProofComments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),

                    ClientIdTypeCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ClientIdNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ClientEmiratesIdNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ClientIdExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClientPassportNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ClientPassportDateOfIssue = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClientCountryIssuanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),

                    SponsorName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    SponsorAliases = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    SponsorIdTypeCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    SponsorIdNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    SponsorDateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SponsorNationalityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SponsorGenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SponsorDualNationality = table.Column<bool>(type: "bit", nullable: true),
                    SponsorOtherDetails = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),

                    BankCountryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BankIbanAccountNo = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    AccountName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    BankSwiftCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    BankAddress = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    BankCurrency = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),

                    EmployerCompanyName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmployerCompanyWebsite = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    EmployerEmailAddress = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmployerTelNo = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    EmployerAddress = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EmployerIndustryAndBusinessDetails = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),

                    PepFATFIncreasedMonitoringAnswer = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    PepSanctionListOrInverseMediaAnswer = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    PepProminentPublicFunctionsAnswer = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    PepAnyPEPsAfterScreeningAnswer = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    PepSpecificPEPsAfterScreeningDetails = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),

                    FollowUpDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FollowUpRemarks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndividualKyc", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndividualKyc_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IndividualKyc_CustomerId",
                table: "IndividualKyc",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_IndividualKyc_CustomerId_IsActive",
                table: "IndividualKyc",
                columns: new[] { "CustomerId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_IndividualKyc_TenantId",
                table: "IndividualKyc",
                column: "TenantId");

            migrationBuilder.CreateTable(
                name: "IndividualKycDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IndividualKycId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),

                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),

                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),

                    DocumentNo = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    IssuedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    FolderPath = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),

                    FileName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),

                    UploadedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndividualKycDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndividualKycDocuments_IndividualKyc_IndividualKycId",
                        column: x => x.IndividualKycId,
                        principalTable: "IndividualKyc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IndividualKycDocuments_CustomerId",
                table: "IndividualKycDocuments",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_IndividualKycDocuments_IndividualKycId",
                table: "IndividualKycDocuments",
                column: "IndividualKycId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IndividualKycDocuments");

            migrationBuilder.DropTable(
                name: "IndividualKyc");
        }
    }
}

