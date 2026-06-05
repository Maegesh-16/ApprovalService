using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApprovalService.API.Migrations
{
    /// <inheritdoc />
    public partial class AddApproverUserIdToApprovalRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApproverUserId",
                table: "ApprovalRequests",
                type: "int",
                nullable: false,
                defaultValue: 1); // Default to 1 for existing records (will need manual update)
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApproverUserId",
                table: "ApprovalRequests");
        }
    }
}
