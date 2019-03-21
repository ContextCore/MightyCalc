using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MightyCalc.Reports.DatabaseProjections;

namespace MightyCalc.Reports.Tests
{
    
    public class BloggingContextFactory : IDesignTimeDbContextFactory<FunctionUsageContext>
    {
        public FunctionUsageContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<FunctionUsageContext>();
            optionsBuilder.UseNpgsql("Host=localhost:32773;Database=postgres;Username=postgres;");
            return new FunctionUsageContext(optionsBuilder.Options);
        }
    }
}