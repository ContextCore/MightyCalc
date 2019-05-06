using Microsoft.EntityFrameworkCore.Migrations;

namespace MightyCalc.Reports.Migrations
{
    public partial class KnownFunctions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KnownFunctions",
                columns: table => new
                {
                    CalculatorId = table.Column<string>(nullable: false),
                    FunctionName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnownFunctions", x => new { x.CalculatorId, x.FunctionName });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KnownFunctions");
        }
    }
}
