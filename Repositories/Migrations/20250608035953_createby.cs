using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class createby : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Users_CreatorUserID",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_CreatorUserID",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "CreatorUserID",
                table: "Courses");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_CreatedBy",
                table: "Courses",
                column: "CreatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Users_CreatedBy",
                table: "Courses",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Users_CreatedBy",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_CreatedBy",
                table: "Courses");

            migrationBuilder.AddColumn<int>(
                name: "CreatorUserID",
                table: "Courses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_CreatorUserID",
                table: "Courses",
                column: "CreatorUserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Users_CreatorUserID",
                table: "Courses",
                column: "CreatorUserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
