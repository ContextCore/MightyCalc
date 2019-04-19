using System;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using Akka.Streams.Implementation.Fusing;
using MightyCalc.Reports.DatabaseProjections;
using MightyCalc.Reports.ReportingExtension;

namespace MightyCalc.Reports.Streams
{

    public class FunctionsTotalUsageProjector : ReceiveActor
    {
        public FunctionsTotalUsageProjector(string eventName = "unknown")
        {
            var dependencies = Context.System.GetReportingExtension().GetDependencies();
            Func<FunctionUsageContext> contextFactory = () => dependencies.CreateFunctionUsageContext();
            var log = Context.GetLogger();
            
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
            Receive<SequencedFunctionTotalUsage>(e =>
            {
                log.Debug("Received event to project");

                using (var context = contextFactory.Invoke())
                {
                    var existingUsage =
                        context.FunctionsTotalUsage.SingleOrDefault(u => u.FunctionName == e.FunctionName);
                    if (existingUsage == null)
                        context.FunctionsTotalUsage.Add(new FunctionTotalUsage
                        {
                            FunctionName = e.FunctionName,
                            InvocationsCount = e.InvocationsCount
                        });
                    else
                    {
                        existingUsage.InvocationsCount += e.InvocationsCount;
                    }

                    var projection = dependencies.CreateFindProjectionQuery(context)
                        .Execute(KnownProjectionsNames.TotalFunctionUsage,
                            nameof(FunctionsTotalUsageProjector),
                            eventName);
                    
                    if (projection == null)
                        context.Projections.Add(new Projection
                        {
                            Event = eventName,
                            Name = KnownProjectionsNames.TotalFunctionUsage,
                            Projector = nameof(FunctionsTotalUsageProjector),
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
