using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MightyCalc.Reports.DatabaseProjections
{
    public class FunctionsTotalUsageQuery: IFunctionsTotalUsageQuery
    {
        private readonly FunctionUsageContext _context;

        public FunctionsTotalUsageQuery(FunctionUsageContext context)
        {
            _context = context;
        }
        public async Task<IReadOnlyCollection<FunctionTotalUsage>> Execute(string functionName = null, DateTimeOffset? @from = null, DateTimeOffset? to = null)
        {
            if (String.IsNullOrEmpty(functionName))
                return await _context.FunctionsTotalUsage.ToArrayAsync();
            
            return await _context.FunctionsTotalUsage
                .Where(f => f.FunctionName.Contains(functionName))
                .ToArrayAsync();
        }
    }
}