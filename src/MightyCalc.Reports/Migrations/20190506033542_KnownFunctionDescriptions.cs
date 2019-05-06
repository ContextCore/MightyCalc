using Microsoft.EntityFrameworkCore.Migrations;

namespace MightyCalc.Reports.Migrations
{
    public partial class KnownFunctionDescriptions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FunctionName",
                table: "KnownFunctions",
                newName: "Name");

            migrationBuilder.AddColumn<int>(
                name: "Arity",
                table: "KnownFunctions",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "KnownFunctions",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Expression",
                table: "KnownFunctions",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Parameters",
                table: "KnownFunctions",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Arity",
                table: "KnownFunctions");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "KnownFunctions");

            migrationBuilder.DropColumn(
                name: "Expression",
                table: "KnownFunctions");

            migrationBuilder.DropColumn(
                name: "Parameters",
                table: "KnownFunctions");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "KnownFunctions",
                newName: "FunctionName");
        }
    }
}
