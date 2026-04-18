using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmlScreening.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSanctionListMultiValuedJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddressesJson",
                table: "SanctionListEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AliasesJson",
                table: "SanctionListEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DatesOfBirthJson",
                table: "SanctionListEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DesignationsJson",
                table: "SanctionListEntries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DocumentsJson",
                table: "SanctionListEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastDayUpdatesJson",
                table: "SanctionListEntries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NationalitiesJson",
                table: "SanctionListEntries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PlacesOfBirthJson",
                table: "SanctionListEntries",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressesJson",
                table: "SanctionListEntries");

            migrationBuilder.DropColumn(
                name: "AliasesJson",
                table: "SanctionListEntries");

            migrationBuilder.DropColumn(
                name: "DatesOfBirthJson",
                table: "SanctionListEntries");

            migrationBuilder.DropColumn(
                name: "DesignationsJson",
                table: "SanctionListEntries");

            migrationBuilder.DropColumn(
                name: "DocumentsJson",
                table: "SanctionListEntries");

            migrationBuilder.DropColumn(
                name: "LastDayUpdatesJson",
                table: "SanctionListEntries");

            migrationBuilder.DropColumn(
                name: "NationalitiesJson",
                table: "SanctionListEntries");

            migrationBuilder.DropColumn(
                name: "PlacesOfBirthJson",
                table: "SanctionListEntries");
        }
    }
}
