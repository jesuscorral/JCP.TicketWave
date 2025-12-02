using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JCP.TicketWave.BookingService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.CreateTable(
                name: "bookings",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bookings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tickets",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: true),
                    TicketType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SeatNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ReservedUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tickets_bookings_BookingId",
                        column: x => x.BookingId,
                        principalSchema: "public",
                        principalTable: "bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bookings_created_at",
                schema: "public",
                table: "bookings",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_event_id",
                schema: "public",
                table: "bookings",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_event_status",
                schema: "public",
                table: "bookings",
                columns: new[] { "EventId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_bookings_expires_at",
                schema: "public",
                table: "bookings",
                column: "ExpiresAt",
                filter: "expires_at IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_status",
                schema: "public",
                table: "bookings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_user_id",
                schema: "public",
                table: "bookings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_user_status",
                schema: "public",
                table: "bookings",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_tickets_booking_id",
                schema: "public",
                table: "tickets",
                column: "BookingId",
                filter: "booking_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_event_id",
                schema: "public",
                table: "tickets",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_event_seat_unique",
                schema: "public",
                table: "tickets",
                columns: new[] { "EventId", "SeatNumber" },
                unique: true,
                filter: "seat_number IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_event_status",
                schema: "public",
                table: "tickets",
                columns: new[] { "EventId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_tickets_event_type_status",
                schema: "public",
                table: "tickets",
                columns: new[] { "EventId", "TicketType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_tickets_reserved_until",
                schema: "public",
                table: "tickets",
                column: "ReservedUntil",
                filter: "reserved_until IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_status",
                schema: "public",
                table: "tickets",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tickets",
                schema: "public");

            migrationBuilder.DropTable(
                name: "bookings",
                schema: "public");
        }
    }
}
