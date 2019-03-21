using Microsoft.EntityFrameworkCore;
using MightyCalc.Reports.DatabaseProjections;

namespace MightyCalc.Reports.ReportingExtension
{
    class ReportingDependencies : IReportingDependencies
    {
        private readonly DbContextOptions<FunctionUsageContext> _dbContextOptions;

        public ReportingDependencies(DbContextOptions<FunctionUsageContext> dbContextOptions)
        {
            _dbContextOptions = dbContextOptions;
        }
        public FunctionUsageContext CreateFunctionUsageContext()
        {
            return new FunctionUsageContext(_dbContextOptions);
        }

        public IFindProjectionQuery CreateFindProjectionQuery(FunctionUsageContext context = null)
        {
            return new FindProjectionQuery(context ?? CreateFunctionUsageContext());
        }
    }
}