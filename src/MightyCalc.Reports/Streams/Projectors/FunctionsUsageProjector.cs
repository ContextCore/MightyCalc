using System;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using MightyCalc.Node.Akka;
using MightyCalc.Reports.DatabaseProjections;
using MightyCalc.Reports.ReportingExtension;

namespace MightyCalc.Reports.Streams.Projectors
{
    public class FunctionsUsageProjector : ReceiveActor
    {
        public FunctionsUsageProjector(string eventName = "unknown", TimeSpan? periodParam = null)
        {
            var dependencies = Context.System.GetReportingExtension().GetDependencies();
            Func<FunctionUsageContext> contextFactory = () => dependencies.CreateFunctionUsageContext();
            var log = Context.GetLogger();
            var period = periodParam ?? TimeSpan.FromMinutes(1);
            
            Receive<ProjectorActorProtocol.Start>(s =>
            {
                log.Info("Starting projection");
                Sender.Tell(ProjectorActorProtocol.Next.Instance);
            });
            Receive<ProjectorActorProtocol.ProjectionDone>(s =>
            {
                log.Info("Stopping projection");
            });
            //Event processing intentionally made slow for simplicity
            Receive<Sequenced<CalculatorActor.CalculationPerformed>>(e =>
            {
                log.Debug("Received event to project");
                var now = DateTimeOffset.Now;
                var evt = e.Message;
                
                using (var context = contextFactory.Invoke())
                {

                    foreach (var functionUsage in e.Message.FunctionsUsed.GroupBy(f => f))
                    {
                        var existingUsage =
                            context.FunctionsUsage.SingleOrDefault(u => u.FunctionName == functionUsage.Key
                                                                        && u.CalculatorName == evt.CalculatorId
                                                                        && u.PeriodStart <= evt.Occured
                                                                        && u.PeriodEnd >= evt.Occured);
                        if (existingUsage == null)
                            context.FunctionsUsage.Add(new FunctionUsage
                            {
                                FunctionName = functionUsage.Key,
                                InvocationsCount = functionUsage.Count(),
                                CalculatorName = evt.CalculatorId,
                                Period = period,
                                PeriodStart = evt.Occured.ToPeriodBegin(period),
                                PeriodEnd = evt.Occured.ToPeriodEnd(period)
                            });
                        else
                        {
                            existingUsage.InvocationsCount += functionUsage.Count();
                        }
                    }

                    var projection = dependencies.CreateFindProjectionQuery(context)
                            .Execute(KnownProjectionsNames.FunctionUsage,
                                nameof(FunctionsUsageProjector),
                                eventName);

                        if (projection == null)
                            context.Projections.Add(new Projection
                            {
                                Event = eventName,
                                Name = KnownProjectionsNames.FunctionUsage,
                                Projector = nameof(FunctionsUsageProjector),
                                Sequence = e.Sequence
                            });
                        else
                            projection.Sequence = e.Sequence;

                    //important to update projection sequence and total function usage in a single transaction
                    context.SaveChanges();
                }

                Sender.Tell(ProjectorActorProtocol.Next.Instance);
            });
            
            ReceiveAny(o => log.Warning("missing message: " + o.ToString()));
        }

    }
}