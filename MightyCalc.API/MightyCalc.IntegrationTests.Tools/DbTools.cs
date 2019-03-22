using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MightyCalc.Reports.DatabaseProjections;

namespace MightyCalc.IntegrationTests.Tools
{
    public static class DbTools
    {
        public static async Task TruncateTables(string connectionString, params string[] tables)
        {
            var journalOptions =
                new DbContextOptionsBuilder<FunctionUsageContext>().UseNpgsql(connectionString).Options;
            using (var context = new FunctionUsageContext(journalOptions))
            {
                var sql = $"TRUNCATE {string.Join(",", tables.Select(t => $"\"{t}\""))} RESTART IDENTITY CASCADE;";
                await context.Database.ExecuteSqlCommandAsync(sql);
            }
        }
        
        public static async Task ResetDatabases()
        {
            await DbTools.TruncateTables(KnownConnectionStrings.ReadModel,
                "Projections",
                "FunctionsUsage",
                "FunctionsTotalUsage");
            await DbTools.TruncateTables(KnownConnectionStrings.Journal, "event_journal","metadata");
            await DbTools.TruncateTables(KnownConnectionStrings.SnapshotStore, "snapshot_store");
        }
    }
}