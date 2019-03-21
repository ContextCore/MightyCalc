using System.Linq;

namespace MightyCalc.Reports.DatabaseProjections
{
    public class FindProjectionQuery : IFindProjectionQuery
    {
        private readonly FunctionUsageContext _context;

        public FindProjectionQuery(FunctionUsageContext context)
        {
            _context = context;
        }

        public Projection Execute(string name, string projector, string eventName)
        {
            return _context.Projections.SingleOrDefault(p => p.Name == name &&
                                                             p.Event == eventName &&
                                                             p.Projector == projector);
        }
    }
}