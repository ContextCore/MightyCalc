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
        public async Task<IReadOnlyCollection<FunctionTotalUsage>> Execute(string functionNameWildCard)
        {
            return await _context.FunctionsTotalUsage
                .Where(f => f.FunctionName.Contains(functionNameWildCard))
                .ToArrayAsync();
        }
    }
}