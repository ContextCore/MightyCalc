using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MightyCalc.Reports.Migrations
{
    public partial class FuntionUsageByPeriod : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_FunctionsUsage",
                table: "FunctionsUsage");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PeriodStart",
                table: "FunctionsUsage",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PeriodEnd",
                table: "FunctionsUsage",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Period",
                table: "FunctionsUsage",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddPrimaryKey(
                name: "PK_FunctionsUsage",
                table: "FunctionsUsage",
                columns: new[] { "CalculatorName", "FunctionName", "PeriodStart", "PeriodEnd" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_FunctionsUsage",
                table: "FunctionsUsage");

            migrationBuilder.DropColumn(
                name: "PeriodStart",
                table: "FunctionsUsage");

            migrationBuilder.DropColumn(
                name: "PeriodEnd",
                table: "FunctionsUsage");

            migrationBuilder.DropColumn(
                name: "Period",
                table: "FunctionsUsage");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FunctionsUsage",
                table: "FunctionsUsage",
                columns: new[] { "CalculatorName", "FunctionName" });
        }
    }
}
