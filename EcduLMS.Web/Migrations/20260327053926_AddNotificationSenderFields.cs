using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduLMS.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationSenderFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Notifications",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(2000)",
                oldMaxLength: 2000)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "CourseId",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SenderInstructorId",
                table: "Notifications",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CourseId",
                table: "Notifications",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_SenderInstructorId",
                table: "Notifications",
                column: "SenderInstructorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_SenderInstructorId",
                table: "Notifications",
                column: "SenderInstructorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Courses_CourseId",
                table: "Notifications",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_SenderInstructorId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Courses_CourseId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_CourseId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_SenderInstructorId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "SenderInstructorId",
                table: "Notifications");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Notifications",
                type: "varchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
