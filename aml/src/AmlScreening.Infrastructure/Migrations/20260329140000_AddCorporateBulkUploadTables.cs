using System;
using AmlScreening.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmlScreening.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260329140000_AddCorporateBulkUploadTables")]
public class AddCorporateBulkUploadTables : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "CorporateBulkUploadBatches",
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
                table.PrimaryKey("PK_CorporateBulkUploadBatches", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_CorporateBulkUploadBatches_TenantId",
            table: "CorporateBulkUploadBatches",
            column: "TenantId");

        migrationBuilder.CreateTable(
            name: "CorporateBulkUploadLines",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                LineIndex = table.Column<int>(type: "int", nullable: false),
                CustomerId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                FullName = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                IncorporatedCountry = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                DateOfIncorporationRaw = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                DateOfIncorporationParsed = table.Column<DateTime>(type: "datetime2", nullable: true),
                CompanyReferenceCode = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                TradeLicense = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                IncorporatedCountryResolvedCode = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
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
                table.PrimaryKey("PK_CorporateBulkUploadLines", x => x.Id);
                table.ForeignKey(
                    name: "FK_CorporateBulkUploadLines_CorporateBulkUploadBatches_BatchId",
                    column: x => x.BatchId,
                    principalTable: "CorporateBulkUploadBatches",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_CorporateBulkUploadLines_BatchId",
            table: "CorporateBulkUploadLines",
            column: "BatchId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "CorporateBulkUploadLines");
        migrationBuilder.DropTable(name: "CorporateBulkUploadBatches");
    }
}
