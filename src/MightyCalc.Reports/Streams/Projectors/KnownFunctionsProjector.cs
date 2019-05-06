using System;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using MightyCalc.Calculations.Aggregate.Events;
using MightyCalc.Reports.DatabaseProjections;
using MightyCalc.Reports.ReportingExtension;

namespace MightyCalc.Reports.Streams.Projectors
{
    public class KnownFunctionsProjector : ReceiveActor
    {
        public KnownFunctionsProjector(string eventName = "unknown")
        {
            var dependencies = Context.System.GetReportingExtension().GetDependencies();
            Func<FunctionUsageContext> contextFactory = () => dependencies.CreateFunctionUsageContext();
            var log = Context.GetLogger();

            Receive<ProjectorActorProtocol.Start>(s =>
            {
                log.Info("Starting projection");
                Sender.Tell(ProjectorActorProtocol.Next.Instance);
            });
            Receive<ProjectorActorProtocol.ProjectionDone>(s => { log.Info("Stopping projection"); });
            //Event processing intentionally made slow for simplicity
            Receive<FunctionAdded>(e =>
            {
                log.Debug("Received event to project");

                using (var context = contextFactory.Invoke())
                {
                    var knownFunction =
                        context.KnownFunctions.SingleOrDefault(u =>
                            u.Name == e.CalculatorId && u.Name == e.Definition.Name);

                    if (knownFunction == null)
                        context.KnownFunctions.Add(new KnownFunction()
                        {
                            CalculatorId = e.CalculatorId,
                            Name = e.Definition.Name,
                            Arity = e.Definition.Arity,
                            Description = e.Definition.Description,
                            Expression = e.Definition.Expression,
                            Parameters = String.Join(";", e.Definition.Parameters)
                        });
                    else
                    {
                        knownFunction.Arity = e.Definition.Arity;
                        knownFunction.Description = e.Definition.Description;
                        knownFunction.Expression = e.Definition.Expression;
                        knownFunction.Parameters = String.Join(";", e.Definition.Parameters);
                    }

                    var projection = dependencies.CreateFindProjectionQuery(context)
                        .Execute(KnownProjectionsNames.KnownFunctions,
                            nameof(KnownFunctionsProjector),
                            eventName);

                    if (projection == null)
                        context.Projections.Add(new Projection
                        {
                            Event = eventName,
                            Name = KnownProjectionsNames.KnownFunctions,
                            Projector = nameof(KnownFunctionsProjector),
                            Sequence = e.Version
                        });
                    else
                        projection.Sequence = e.Version;

                    //important to update projection sequence and total function usage in a single transaction
                    context.SaveChanges();
                }

                Sender.Tell(ProjectorActorProtocol.Next.Instance);
            });

            ReceiveAny(o => log.Warning("missing message: " + o.ToString()));
        }
    }
}