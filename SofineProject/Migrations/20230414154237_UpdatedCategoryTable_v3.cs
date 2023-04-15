using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SofineProject.Migrations
{
    public partial class UpdatedCategoryTable_v3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image",
                table: "Categories");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "Categories",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }
    }
}
