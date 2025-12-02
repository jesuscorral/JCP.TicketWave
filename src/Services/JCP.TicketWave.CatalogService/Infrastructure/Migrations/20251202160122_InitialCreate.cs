using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JCP.TicketWave.CatalogService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "catalog");

            migrationBuilder.CreateTable(
                name: "Categories",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IconUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false, defaultValue: "#007ACC"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VenueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    StartDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TicketTypes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Images = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Venues",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    AddressStreet = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AddressCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AddressState = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AddressCountry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AddressPostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AddressLatitude = table.Column<decimal>(type: "decimal(10,8)", nullable: true),
                    AddressLongitude = table.Column<decimal>(type: "decimal(11,8)", nullable: true),
                    CapacityTotal = table.Column<int>(type: "int", nullable: false),
                    CapacitySections = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CapacityHasSeatedSections = table.Column<bool>(type: "bit", nullable: false),
                    CapacityHasStandingAreas = table.Column<bool>(type: "bit", nullable: false),
                    Amenities = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Images = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Venues", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_DisplayOrder",
                schema: "catalog",
                table: "Categories",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_IsActive",
                schema: "catalog",
                table: "Categories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_TenantName",
                schema: "catalog",
                table: "Categories",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_CategoryId",
                schema: "catalog",
                table: "Events",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_CreatedAt",
                schema: "catalog",
                table: "Events",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Events_StartDateTime",
                schema: "catalog",
                table: "Events",
                column: "StartDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_Events_Status",
                schema: "catalog",
                table: "Events",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Events_TenantCategory",
                schema: "catalog",
                table: "Events",
                columns: new[] { "TenantId", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_Events_VenueId",
                schema: "catalog",
                table: "Events",
                column: "VenueId");

            migrationBuilder.CreateIndex(
                name: "IX_Venues_IsActive",
                schema: "catalog",
                table: "Venues",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Venues_Name",
                schema: "catalog",
                table: "Venues",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Venues_TenantCity",
                schema: "catalog",
                table: "Venues",
                columns: new[] { "TenantId", "CityId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Categories",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "Events",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "Venues",
                schema: "catalog");
        }
    }
}
