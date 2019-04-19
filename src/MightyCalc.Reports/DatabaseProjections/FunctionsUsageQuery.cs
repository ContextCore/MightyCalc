using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MightyCalc.Reports.DatabaseProjections
{
    public class FunctionsUsageQuery : IFunctionsUsageQuery
    {
        private readonly FunctionUsageContext _context;

        public FunctionsUsageQuery(FunctionUsageContext context)
        {
            _context = context;
        }
        
        public async Task<IReadOnlyCollection<FunctionUsage>> Execute(string calculatorName = null, DateTimeOffset? periodStart = null, DateTimeOffset? periodEnd = null)
        {
            IQueryable<FunctionUsage> functions = _context.FunctionsUsage;

            if (!String.IsNullOrEmpty(calculatorName))
                functions = functions.Where(f => f.CalculatorName == calculatorName);
            
            if (periodStart != null)
                functions = functions.Where(f => f.PeriodStart >= periodStart);
            
            if (periodEnd != null)
                functions = functions.Where(f => f.PeriodEnd <= periodEnd);

            return await functions.ToArrayAsync();
        }
    }
}