using Microsoft.EntityFrameworkCore.Migrations;

namespace MightyCalc.Reports.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FunctionsTotalUsage",
                columns: table => new
                {
                    FunctionName = table.Column<string>(nullable: false),
                    InvocationsCount = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FunctionsTotalUsage", x => x.FunctionName);
                });

            migrationBuilder.CreateTable(
                name: "FunctionsUsage",
                columns: table => new
                {
                    CalculatorName = table.Column<string>(nullable: false),
                    FunctionName = table.Column<string>(nullable: false),
                    InvocationsCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FunctionsUsage", x => new { x.CalculatorName, x.FunctionName });
                });

            migrationBuilder.CreateTable(
                name: "Projections",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    Projector = table.Column<string>(nullable: false),
                    Event = table.Column<string>(nullable: false),
                    Sequence = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projections", x => new { x.Name, x.Projector, x.Event });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FunctionsTotalUsage");

            migrationBuilder.DropTable(
                name: "FunctionsUsage");

            migrationBuilder.DropTable(
                name: "Projections");
        }
    }
}
