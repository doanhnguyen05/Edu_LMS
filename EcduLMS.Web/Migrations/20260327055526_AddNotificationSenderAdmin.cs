using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduLMS.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationSenderAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SenderAdminId",
                table: "Notifications",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_SenderAdminId",
                table: "Notifications",
                column: "SenderAdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_SenderAdminId",
                table: "Notifications",
                column: "SenderAdminId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_SenderAdminId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_SenderAdminId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "SenderAdminId",
                table: "Notifications");
        }
    }
}
