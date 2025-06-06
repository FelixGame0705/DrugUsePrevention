using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class fixCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Programs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "isAccept",
                table: "Courses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "isActive",
                table: "Courses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "CourseContent",
                columns: table => new
                {
                    ContentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseID = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    isActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseContent", x => x.ContentID);
                    table.ForeignKey(
                        name: "FK_CourseContent_Courses_CourseID",
                        column: x => x.CourseID,
                        principalTable: "Courses",
                        principalColumn: "CourseID");
                });

            migrationBuilder.CreateTable(
                name: "CheckCourseContent",
                columns: table => new
                {
                    CheckID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RegistrationID = table.Column<int>(type: "int", nullable: false),
                    ContentID = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckCourseContent", x => x.CheckID);
                    table.ForeignKey(
                        name: "FK_CheckCourseContent_CourseContent_ContentID",
                        column: x => x.ContentID,
                        principalTable: "CourseContent",
                        principalColumn: "ContentID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CheckCourseContent_CourseRegistrations_RegistrationID",
                        column: x => x.RegistrationID,
                        principalTable: "CourseRegistrations",
                        principalColumn: "RegistrationID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CheckCourseContent_ContentID",
                table: "CheckCourseContent",
                column: "ContentID");

            migrationBuilder.CreateIndex(
                name: "IX_CheckCourseContent_RegistrationID",
                table: "CheckCourseContent",
                column: "RegistrationID");

            migrationBuilder.CreateIndex(
                name: "IX_CourseContent_CourseID",
                table: "CourseContent",
                column: "CourseID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CheckCourseContent");

            migrationBuilder.DropTable(
                name: "CourseContent");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "isAccept",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "isActive",
                table: "Courses");
        }
    }
}
