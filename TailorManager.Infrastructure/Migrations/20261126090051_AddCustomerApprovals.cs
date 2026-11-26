using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TailorManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerApprovals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerApproval",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Scope = table.Column<int>(type: "integer", nullable: false),
                    Method = table.Column<int>(type: "integer", nullable: false),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RelatedDecisionRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    CommunicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    DocumentReference = table.Column<string>(type: "text", nullable: true),
                    ApprovedAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    ApprovedTerms = table.Column<string>(type: "text", nullable: true),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerApproval", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerApproval_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PriceAdjustment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CustomerApprovalId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceAdjustment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceAdjustment_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerApproval_OrderId",
                table: "CustomerApproval",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceAdjustment_OrderId",
                table: "PriceAdjustment",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerApproval");

            migrationBuilder.DropTable(
                name: "PriceAdjustment");
        }
    }
}
