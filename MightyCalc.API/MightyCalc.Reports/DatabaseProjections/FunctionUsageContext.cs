using Microsoft.EntityFrameworkCore;

namespace MightyCalc.Reports.DatabaseProjections
{
    public class FunctionUsageContext : DbContext
    {
        public DbSet<FunctionUsage> FunctionsUsage { get; set; }
        public DbSet<TotalFunctionUsage> TotalFunctionUsage { get; set; }
        public DbSet<Projection> Projections { get; set; }

        public FunctionUsageContext(DbContextOptions<FunctionUsageContext> options)
            : base(options)
        {
            
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FunctionUsage>().HasKey(p => new {p.CalculatorName, p.FunctionName});
            modelBuilder.Entity<Projection>().HasKey(p => new {p.Name, p.Projector, p.Event});
            modelBuilder.Entity<TotalFunctionUsage>().HasKey(p => p.FunctionName);
        }
    }
}