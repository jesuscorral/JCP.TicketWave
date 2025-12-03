using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JCP.TicketWave.BookingService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateSqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "booking");

            migrationBuilder.CreateTable(
                name: "bookings",
                schema: "booking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerEmail = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bookings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tickets",
                schema: "booking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TicketType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SeatNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ReservedUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tickets_bookings_BookingId",
                        column: x => x.BookingId,
                        principalSchema: "booking",
                        principalTable: "bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bookings_created_at",
                schema: "booking",
                table: "bookings",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_event_id",
                schema: "booking",
                table: "bookings",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_event_status",
                schema: "booking",
                table: "bookings",
                columns: new[] { "EventId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_bookings_expires_at",
                schema: "booking",
                table: "bookings",
                column: "ExpiresAt",
                filter: "expires_at IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_status",
                schema: "booking",
                table: "bookings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_user_id",
                schema: "booking",
                table: "bookings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_user_status",
                schema: "booking",
                table: "bookings",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_tickets_booking_id",
                schema: "booking",
                table: "tickets",
                column: "BookingId",
                filter: "booking_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_event_id",
                schema: "booking",
                table: "tickets",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_event_seat_unique",
                schema: "booking",
                table: "tickets",
                columns: new[] { "EventId", "SeatNumber" },
                unique: true,
                filter: "seat_number IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_event_status",
                schema: "booking",
                table: "tickets",
                columns: new[] { "EventId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_tickets_event_type_status",
                schema: "booking",
                table: "tickets",
                columns: new[] { "EventId", "TicketType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_tickets_reserved_until",
                schema: "booking",
                table: "tickets",
                column: "ReservedUntil",
                filter: "reserved_until IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_status",
                schema: "booking",
                table: "tickets",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tickets",
                schema: "booking");

            migrationBuilder.DropTable(
                name: "bookings",
                schema: "booking");
        }
    }
}
