using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JCP.TicketWave.PaymentService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "payment");

            migrationBuilder.CreateTable(
                name: "PaymentMethods",
                schema: "payment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Last4Digits = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    ExpiryMonth = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    ExpiryYear = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    CardBrand = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExternalMethodId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                schema: "payment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PaymentMethodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalPaymentId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_PaymentMethods_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalSchema: "payment",
                        principalTable: "PaymentMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentEvents",
                schema: "payment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentEvents_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalSchema: "payment",
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Refunds",
                schema: "payment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ExternalRefundId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Refunds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Refunds_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalSchema: "payment",
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentEvents_EventType",
                schema: "payment",
                table: "PaymentEvents",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentEvents_OccurredAt",
                schema: "payment",
                table: "PaymentEvents",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentEvents_PaymentId",
                schema: "payment",
                table: "PaymentEvents",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_ExternalMethodId",
                schema: "payment",
                table: "PaymentMethods",
                column: "ExternalMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_TenantId",
                schema: "payment",
                table: "PaymentMethods",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_UserId",
                schema: "payment",
                table: "PaymentMethods",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_UserId_IsDefault",
                schema: "payment",
                table: "PaymentMethods",
                columns: new[] { "UserId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BookingId",
                schema: "payment",
                table: "Payments",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CreatedAt",
                schema: "payment",
                table: "Payments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ExternalPaymentId",
                schema: "payment",
                table: "Payments",
                column: "ExternalPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentMethodId",
                schema: "payment",
                table: "Payments",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Status",
                schema: "payment",
                table: "Payments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TenantId",
                schema: "payment",
                table: "Payments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TenantId_Status",
                schema: "payment",
                table: "Payments",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Refunds_CreatedAt",
                schema: "payment",
                table: "Refunds",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Refunds_ExternalRefundId",
                schema: "payment",
                table: "Refunds",
                column: "ExternalRefundId");

            migrationBuilder.CreateIndex(
                name: "IX_Refunds_PaymentId",
                schema: "payment",
                table: "Refunds",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Refunds_Status",
                schema: "payment",
                table: "Refunds",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentEvents",
                schema: "payment");

            migrationBuilder.DropTable(
                name: "Refunds",
                schema: "payment");

            migrationBuilder.DropTable(
                name: "Payments",
                schema: "payment");

            migrationBuilder.DropTable(
                name: "PaymentMethods",
                schema: "payment");
        }
    }
}
