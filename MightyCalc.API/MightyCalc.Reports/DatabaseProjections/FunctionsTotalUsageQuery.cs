using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MightyCalc.Reports.DatabaseProjections
{
    public class FunctionsTotalUsageQuery: IOverallFunctionUsageQuery
    {
        private readonly FunctionUsageContext _context;

        public FunctionsTotalUsageQuery(FunctionUsageContext context)
        {
            _context = context;
        }
        public async Task<IReadOnlyCollection<TotalFunctionUsage>> Execute(string functionNameWildCard)
        {
            return await _context.TotalFunctionUsage
                .Where(f => f.FunctionName.Contains(functionNameWildCard))
                .ToArrayAsync();
        }
    }
}