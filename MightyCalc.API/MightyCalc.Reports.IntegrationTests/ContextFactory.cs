using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MightyCalc.IntegrationTests.Tools;
using MightyCalc.Reports.DatabaseProjections;

namespace MightyCalc.Reports.IntegrationTests
{
    
    public class ContextFactory : IDesignTimeDbContextFactory<FunctionUsageContext>
    {
        public FunctionUsageContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<FunctionUsageContext>();
            optionsBuilder.UseNpgsql(KnownConnectionStrings.ReadModel,
            b => b.MigrationsAssembly("MightyCalc.Reports"));
            return new FunctionUsageContext(optionsBuilder.Options);
        }
    }
}