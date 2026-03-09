using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AmlScreening.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchTypeAndSanctionListEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MatchType",
                table: "SanctionsScreenings",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SanctionListEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ListSource = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Nationality = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanctionListEntries", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "SanctionListEntries",
                columns: new[] { "Id", "DateOfBirth", "FullName", "ListSource", "Nationality" },
                values: new object[,]
                {
                    { new Guid("a1000001-0000-0000-0000-000000000001"), new DateTime(1970, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "John Smith", "United Nations Security Council", "United States" },
                    { new Guid("a1000002-0000-0000-0000-000000000002"), new DateTime(1985, 11, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Jane Doe", "United Nations Security Council", "Syrian Arab Republic" },
                    { new Guid("a1000003-0000-0000-0000-000000000003"), new DateTime(1960, 3, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ahmed Hassan", "Office of Foreign Assets Control", "Iran" },
                    { new Guid("a1000004-0000-0000-0000-000000000004"), new DateTime(1975, 7, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Maria Garcia", "Office of Foreign Assets Control", "Cuba" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SanctionListEntries_ListSource",
                table: "SanctionListEntries",
                column: "ListSource");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SanctionListEntries");

            migrationBuilder.DropColumn(
                name: "MatchType",
                table: "SanctionsScreenings");
        }
    }
}
