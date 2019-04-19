using System.Collections.Generic;
using System.Threading.Tasks;
using MightyCalc.Reports.DatabaseProjections;

namespace MightyCalc.Reports
{
    public interface IFunctionsTotalUsageQuery
    {
        Task<IReadOnlyCollection<FunctionTotalUsage>> Execute(string functionName = null);
    }
}