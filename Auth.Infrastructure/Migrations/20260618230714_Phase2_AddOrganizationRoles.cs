using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase2_AddOrganizationRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d"),
                columns: new[] { "Description", "Name" },
                values: new object[] { "Organization owner with all permissions", "Owner" });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("2b3c4d5e-6f7a-8b9c-0d1e-2f3a4b5c6d7e"),
                columns: new[] { "Description", "Name" },
                values: new object[] { "Administrator with full system access", "Admin" });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("3c4d5e6f-7a8b-9c0d-1e2f-3a4b5c6d7e8f"),
                columns: new[] { "Description", "Name" },
                values: new object[] { "Manager with operational permissions", "Manager" });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedAt", "Description", "IsSystem", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Dispatcher role for assigning tasks", true, "Dispatcher", null },
                    { new Guid("5e6f7a8b-9c0d-1e2f-3a4b-5c6d7e8f9a0b"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Driver role for transportation services", true, "Driver", null },
                    { new Guid("6f7a8b9c-0d1e-2f3a-4b5c-6d7e8f9a0b1c"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Customer role for end users", true, "Customer", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("5e6f7a8b-9c0d-1e2f-3a4b-5c6d7e8f9a0b"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("6f7a8b9c-0d1e-2f3a-4b5c-6d7e8f9a0b1c"));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d"),
                columns: new[] { "Description", "Name" },
                values: new object[] { "Administrator with full system access", "Admin" });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("2b3c4d5e-6f7a-8b9c-0d1e-2f3a4b5c6d7e"),
                columns: new[] { "Description", "Name" },
                values: new object[] { "Driver role for transportation services", "Driver" });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("3c4d5e6f-7a8b-9c0d-1e2f-3a4b5c6d7e8f"),
                columns: new[] { "Description", "Name" },
                values: new object[] { "Customer role for end users", "Customer" });
        }
    }
}
