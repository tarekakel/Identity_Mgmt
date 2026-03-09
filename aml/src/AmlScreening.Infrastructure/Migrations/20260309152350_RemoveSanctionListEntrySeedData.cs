using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AmlScreening.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSanctionListEntrySeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SanctionListEntries",
                keyColumn: "Id",
                keyValue: new Guid("a1000001-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "SanctionListEntries",
                keyColumn: "Id",
                keyValue: new Guid("a1000002-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "SanctionListEntries",
                keyColumn: "Id",
                keyValue: new Guid("a1000003-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "SanctionListEntries",
                keyColumn: "Id",
                keyValue: new Guid("a1000004-0000-0000-0000-000000000004"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
