using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TailorManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerDecisionRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RelatedEntityId",
                table: "OrderHistoryEntry",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CustomerDecisionRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProposedWorkChange = table.Column<string>(type: "text", nullable: false),
                    ProposedPriceAdjustment = table.Column<decimal>(type: "numeric", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ResponseCommunicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    RespondedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerDecisionRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerDecisionRequest_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDecisionRequest_OrderId",
                table: "CustomerDecisionRequest",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerDecisionRequest");

            migrationBuilder.DropColumn(
                name: "RelatedEntityId",
                table: "OrderHistoryEntry");
        }
    }
}
