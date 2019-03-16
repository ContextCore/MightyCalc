using System.Linq;
using Akka.Persistence;
using Microsoft.EntityFrameworkCore;
using MightyCalc.Node;

namespace MightyCalc.Reports
{
    public class FunctionUsageProjectionActor : ReceivePersistentActor
    {
        public override string PersistenceId { get; }

        public FunctionUsageProjectionActor()
        {
            PersistenceId = Self.Path.Name;


            var optionsBuilder = new DbContextOptionsBuilder<FunctionUsageContext>();
            optionsBuilder.UseNpgsql("Host=localhost:32773;Database=postgres;Username=postgres;");

            Command<Project<CalculatorActor.CalculationPerformed>>(p =>
            {
                using (var context = new FunctionUsageContext(optionsBuilder.Options))
                {
                    foreach (var singleIdGroup in p.Events.GroupBy(e => e.CalculatorId))
                    {
                        var usage = singleIdGroup.SelectMany(i => i.FunctionsUsed).GroupBy(i => i)
                            .Select(g => new {Function = g.Key, Count = g.Count()})
                            .ToDictionary(i => i.Function, i => i.Count);

                    

                        var existingUsage = context.FunctionsUsage.Where(u =>
                            u.CalculatorName == singleIdGroup.Key && usage.Keys.Contains(u.FunctionName)).ToArray();
                       
                        foreach (var function in existingUsage)
                        {
                            function.InvocationsCount += usage[function.FunctionName];
                            
                        }
                        context.UpdateRange(existingUsage);
                    }
                }
            });
            Command<Project<CalculatorActor.FunctionAdded>>(a =>
            {
                using (var context = new FunctionUsageContext(optionsBuilder.Options))
                {
                    context.FunctionsUsage.AddRange(a.Events.Select(u => new FunctionUsage
                    {
                        CalculatorName = u.CalculatorId, FunctionName = u.Definition.Expression, InvocationsCount = 0
                    }));
                }
            });
        }

        public class Project<T>
        {
            public T[] Events { get; }

            public Project(params T[] calculations)
            {
                Events = calculations;
            }
        }
    }
}