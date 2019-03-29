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
                foreach (var table in tables)
                {
                    await context.Database.ExecuteSqlCommandAsync(@" 
do $$
begin
    perform 1
    from information_schema.tables 
    where table_name = '"+table+@"';
    if found then
        execute format('truncate %I RESTART IDENTITY cascade', '"+table+@"');
    end if;
end $$;");
                }
            }
        }
        
        public static async Task ResetDatabases()
        {
            await TruncateTables(KnownConnectionStrings.ReadModel,
                "Projections",
                "FunctionsUsage",
                "FunctionsTotalUsage");
            await TruncateTables(KnownConnectionStrings.Journal, "event_journal","metadata");
            await TruncateTables(KnownConnectionStrings.SnapshotStore, "snapshot_store");
        }
    }
}