using Microsoft.EntityFrameworkCore;

namespace MightyCalc.Reports
{
    public class FunctionUsageContext : DbContext
    {
        public DbSet<FunctionUsage> FunctionsUsage { get; set; }
        public DbSet<Projections> Projections { get; set; }

        public FunctionUsageContext(DbContextOptions<FunctionUsageContext> options)
            : base(options)
        {
            
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FunctionUsage>().HasKey(p => new {p.CalculatorName, p.FunctionName});
            modelBuilder.Entity<Projections>().HasKey(p => new {p.Name, p.Event});
        }
    }
}