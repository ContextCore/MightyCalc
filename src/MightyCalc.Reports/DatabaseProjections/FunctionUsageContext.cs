using Microsoft.EntityFrameworkCore;

namespace MightyCalc.Reports.DatabaseProjections
{
    public class FunctionUsageContext : DbContext
    {
        public DbSet<FunctionUsage> FunctionsUsage { get; set; }
        public DbSet<FunctionTotalUsage> FunctionsTotalUsage { get; set; }
        public DbSet<Projection> Projections { get; set; }
        
        public DbSet<KnownFunction> KnownFunctions { get; set; }

        public FunctionUsageContext(DbContextOptions<FunctionUsageContext> options): base(options)
        {
            
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FunctionUsage>().HasKey(p => new {p.CalculatorName, p.FunctionName, p.PeriodStart, p.PeriodEnd});
            modelBuilder.Entity<Projection>().HasKey(p => new {p.Name, p.Projector, p.Event});
            modelBuilder.Entity<FunctionTotalUsage>().HasKey(p => p.FunctionName);
            modelBuilder.Entity<KnownFunction>().HasKey(p => new {p.CalculatorId, p.Name});
        }
    }
}