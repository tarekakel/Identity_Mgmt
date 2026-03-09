using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmlScreening.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExtendSanctionListEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "SanctionListEntries",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AddColumn<string>(
                name: "DataId",
                table: "SanctionListEntries",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VersionNum",
                table: "SanctionListEntries",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "SanctionListEntries",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondName",
                table: "SanctionListEntries",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnListType",
                table: "SanctionListEntries",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ListType",
                table: "SanctionListEntries",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ListedOn",
                table: "SanctionListEntries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastDayUpdated",
                table: "SanctionListEntries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "SanctionListEntries",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Designation",
                table: "SanctionListEntries",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Comments",
                table: "SanctionListEntries",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Aliases",
                table: "SanctionListEntries",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressCity",
                table: "SanctionListEntries",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressCountry",
                table: "SanctionListEntries",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressNote",
                table: "SanctionListEntries",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlaceOfBirthCountry",
                table: "SanctionListEntries",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SortKey",
                table: "SanctionListEntries",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullNameArabic",
                table: "SanctionListEntries",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FamilyNameArabic",
                table: "SanctionListEntries",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FamilyNameLatin",
                table: "SanctionListEntries",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentNumber",
                table: "SanctionListEntries",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IssuingAuthority",
                table: "SanctionListEntries",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "IssueDate",
                table: "SanctionListEntries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "SanctionListEntries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherInformation",
                table: "SanctionListEntries",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TypeDetail",
                table: "SanctionListEntries",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "SanctionListEntries",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(512)",
                oldMaxLength: 512);

            migrationBuilder.DropColumn(name: "DataId", table: "SanctionListEntries");
            migrationBuilder.DropColumn(name: "VersionNum", table: "SanctionListEntries");
            migrationBuilder.DropColumn(name: "FirstName", table: "SanctionListEntries");
            migrationBuilder.DropColumn(name: "SecondName", table: "SanctionListEntries");
            migrationBuilder.DropColumn(name: "UnListType", table: "SanctionListEntries");
            migrationBuilder.DropColumn(name: "ListType", table: "SanctionListEntries");
            migrationBuilder.DropColumn(name: "ListedOn", table: "SanctionListEntries");
            migrationBuilder.DropColumn(name: "LastDayUpdated", table: "SanctionListEntries");
            migrationBuilder.DropColumn(name: "Gender", table: "SanctionListEntries");
            migrationBuilder.DropColumn(name: "Designation", table: "SanctionListEntries");
            migrationBuilder.DropColumn(name: "Comments", table: "SanctionListEntries");
            migrationBuilder.DropColumn(name: "Aliases", table: "SanctionListEntries");
            migrationBuilder.DropColumn(name: "AddressCity", table: "SanctionListEntries");
            migrationBuilder.DropColumn(name: "AddressCountry", table: "SanctionListEntries");
            migrationBuilder.DropColumn(name: "AddressNote", table: "SanctionListEntries");
            migrationBuilder.DropColumn(name: "PlaceOfBirthCountry", table: "SanctionListEntries");
            migrationBuilder.DropColumn(name: "SortKey", table: "SanctionListEntries");
            migrationBuilder.DropColumn(name: "FullNameArabic", table: "SanctionListEntries");
            migrationBuilder.DropColumn(name: "FamilyNameArabic", table: "SanctionListEntries");
            migrationBuilder.DropColumn(name: "FamilyNameLatin", table: "SanctionListEntries");
            migrationBuilder.DropColumn(name: "DocumentNumber", table: "SanctionListEntries");
            migrationBuilder.DropColumn(name: "IssuingAuthority", table: "SanctionListEntries");
            migrationBuilder.DropColumn(name: "IssueDate", table: "SanctionListEntries");
            migrationBuilder.DropColumn(name: "EndDate", table: "SanctionListEntries");
            migrationBuilder.DropColumn(name: "OtherInformation", table: "SanctionListEntries");
            migrationBuilder.DropColumn(name: "TypeDetail", table: "SanctionListEntries");
        }
    }
}
