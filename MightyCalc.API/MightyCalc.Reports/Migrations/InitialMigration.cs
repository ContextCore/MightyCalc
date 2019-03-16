using Microsoft.EntityFrameworkCore.Migrations;

namespace MightyCalc.Reports.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Blogs",
                columns: table => new
                {
                    CalculatorName = table.Column<string>(nullable: false),
                    FunctionName = table.Column<string>(nullable: false),
                    InvocationsCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blogs", x => new { x.CalculatorName, x.FunctionName });
                });

            migrationBuilder.CreateTable(
                name: "Projections",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    Event = table.Column<string>(nullable: false),
                    Sequence = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projections", x => new { x.Name, x.Event });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Blogs");

            migrationBuilder.DropTable(
                name: "Projections");
        }
    }
}
