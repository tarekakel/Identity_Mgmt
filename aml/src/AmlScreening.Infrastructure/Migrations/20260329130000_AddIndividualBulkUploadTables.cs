using System;
using AmlScreening.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmlScreening.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260329130000_AddIndividualBulkUploadTables")]
public class AddIndividualBulkUploadTables : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "IndividualBulkUploadBatches",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                OriginalFileName = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                MatchThreshold = table.Column<int>(type: "int", nullable: false),
                CheckPepUkOnly = table.Column<bool>(type: "bit", nullable: false),
                CheckDisqualifiedDirectorUkOnly = table.Column<bool>(type: "bit", nullable: false),
                CheckSanctions = table.Column<bool>(type: "bit", nullable: false),
                CheckProfileOfInterest = table.Column<bool>(type: "bit", nullable: false),
                CheckReputationalRiskExposure = table.Column<bool>(type: "bit", nullable: false),
                CheckRegulatoryEnforcementList = table.Column<bool>(type: "bit", nullable: false),
                CheckInsolvencyUkIreland = table.Column<bool>(type: "bit", nullable: false),
                ScreeningFinished = table.Column<bool>(type: "bit", nullable: false),
                TotalRowCount = table.Column<int>(type: "int", nullable: false),
                FailedRowCount = table.Column<int>(type: "int", nullable: false),
                QueuedRowCount = table.Column<int>(type: "int", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_IndividualBulkUploadBatches", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_IndividualBulkUploadBatches_TenantId",
            table: "IndividualBulkUploadBatches",
            column: "TenantId");

        migrationBuilder.CreateTable(
            name: "IndividualBulkUploadLines",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                LineIndex = table.Column<int>(type: "int", nullable: false),
                CustomerId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                FullName = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                Nationality = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                DateOfBirthRaw = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                DateOfBirthParsed = table.Column<DateTime>(type: "datetime2", nullable: true),
                CompanyReferenceCode = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                IdType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                IdNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                ReferenceId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                PlaceOfBirth = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                NationalityResolvedCode = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                QueuedForScreening = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_IndividualBulkUploadLines", x => x.Id);
                table.ForeignKey(
                    name: "FK_IndividualBulkUploadLines_IndividualBulkUploadBatches_BatchId",
                    column: x => x.BatchId,
                    principalTable: "IndividualBulkUploadBatches",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_IndividualBulkUploadLines_BatchId",
            table: "IndividualBulkUploadLines",
            column: "BatchId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "IndividualBulkUploadLines");
        migrationBuilder.DropTable(name: "IndividualBulkUploadBatches");
    }
}
