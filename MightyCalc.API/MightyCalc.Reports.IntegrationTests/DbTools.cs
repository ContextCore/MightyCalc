using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MightyCalc.Reports.DatabaseProjections;
using Npgsql;

namespace MightyCalc.Reports.IntegrationTests
{
    public static class DbTools
    {
        public static async Task TruncateTables(string connectionString, params string[] tables)
        {
            var journalOptions =
                new DbContextOptionsBuilder<FunctionUsageContext>().UseNpgsql(connectionString).Options;
            using (var context = new FunctionUsageContext(journalOptions))
            {
                try
                {
                    await context.Database.ExecuteSqlCommandAsync(
                        $"TRUNCATE {string.Join(",", tables.Select(t => $"\"{t}\""))} RESTART IDENTITY CASCADE;");
                }
                catch (PostgresException e)
                {
                    Console.WriteLine(e);
                }
            }

            ;
        }
    }
}