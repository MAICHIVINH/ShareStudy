using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareStudy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEducationLevelToSubject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "education_level_id",
                table: "subjects",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_subjects_education_level_id",
                table: "subjects",
                column: "education_level_id");

            migrationBuilder.AddForeignKey(
                name: "FK_subjects_education_levels_education_level_id",
                table: "subjects",
                column: "education_level_id",
                principalTable: "education_levels",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_subjects_education_levels_education_level_id",
                table: "subjects");

            migrationBuilder.DropIndex(
                name: "IX_subjects_education_level_id",
                table: "subjects");

            migrationBuilder.DropColumn(
                name: "education_level_id",
                table: "subjects");
        }
    }
}
