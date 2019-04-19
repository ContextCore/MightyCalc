using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MightyCalc.Reports.DatabaseProjections;

namespace MightyCalc.Reports
{
    public interface IFunctionsTotalUsageQuery
    {
        Task<IReadOnlyCollection<FunctionTotalUsage>> Execute(string functionName = null);
    }

    public interface IFunctionsUsageQuery
    {
        Task<IReadOnlyCollection<FunctionTotalUsage>> Execute(string functionName = null, DateTimeOffset? periodStart=null, DateTimeOffset? periodEnd = null);
    }
}