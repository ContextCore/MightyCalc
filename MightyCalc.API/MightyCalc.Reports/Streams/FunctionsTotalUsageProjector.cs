using System;
using System.Linq;
using Akka.Actor;
using MightyCalc.Reports.DatabaseProjections;
using MightyCalc.Reports.ReportingExtension;

namespace MightyCalc.Reports.Streams
{
    /// <summary>
    /// This projection relies on ordering on 
    /// </summary>
    public class FunctionsTotalUsageProjector : ReceiveActor
    {
        public FunctionsTotalUsageProjector(string eventName = "unknown")
        {
            var dependencies = Context.System.GetReportingExtension().GetDependencies();
            Func<FunctionUsageContext> contextFactory = () => dependencies.CreateFunctionUsageContext();

            Receive<Start>(s =>
            {
                Sender.Tell(Next.Instance);
            });
            Receive<ProjectionDone>(s =>
            {
            });
            //Event processing intentionally made slow for simplicity
            Receive<SequencedFunctionUsage>(e =>
            {
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
                        existingUsage.InvocationsCount = e.InvocationsCount;
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

                Sender.Tell(Next.Instance);
            });
        }

        public class Start
        {
            private Start()
            {
            }

            public static readonly Start Instance = new Start();
        }

        public class Next
        {
            private Next()
            {
            }

            public static readonly Next Instance = new Next();
        }

        public class ProjectionDone
        {
            private ProjectionDone()
            {
            }

            public static readonly ProjectionDone Instance = new ProjectionDone();
        }
    }
}