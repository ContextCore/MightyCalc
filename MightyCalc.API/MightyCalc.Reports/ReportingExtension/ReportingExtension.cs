using System;
using System.Security.Cryptography.X509Certificates;
using Akka.Actor;
using Akka.Persistence;
using Akka.Persistence.Journal;
using Akka.Persistence.Query;
using Akka.Persistence.Query.Sql;
using Akka.Streams;
using Akka.Streams.Dsl;
using Akka.Streams.Implementation.Fusing;
using Autofac;
using Microsoft.EntityFrameworkCore;
using MightyCalc.Node;

namespace MightyCalc.Reports.ReportingExtension {

    public interface IReportingDependencies
    {
        FunctionUsageContext CreateFunctionUsageContext();
    }

    class ReportingDependencies : IReportingDependencies
    {
        public FunctionUsageContext CreateFunctionUsageContext()
        {
            throw new System.NotImplementedException();
        }
    }
    
    public class ReportingExtension : IExtension
    {
        private readonly IContainer _container;
        private ActorSystem _system;
        int lastFunctionUsageProjected = 0;
        int lastFunctionAddProjected = 0;
        
        public ReportingExtension(ActorSystem system, IContainer container)
        {
            _system = system;
            _container = container;
            
          //  var reportingActor = 
        }

        public IReportingDependencies GetDependencies()
        {
            return _container.Resolve<IReportingDependencies>();
        }
    }

    /// <summary>
    /// This projection relies on ordering on 
    /// </summary>
    public class OverallFunctionUsageProjector : ReceiveActor
    {
        public OverallFunctionUsageProjector()
        {
           // PersistenceId = Self.Path.Name;
            int lastFunctionUsageProjected = 0;
            int lastFunctionAddProjected = 0;

            var dependencies = Context.System.GetReportingExtension().GetDependencies();
            Func<DbContext> contextFactory = () => dependencies.CreateFunctionUsageContext();
            
            
            Receive<ProjectEvents<CalculatorActor.FunctionAdded>>(e =>
            {
                   
            });
            
            Receive<ProjectEvents<CalculatorActor.CalculationPerformed>>(e =>
            {
                
            });
            
        }

        public class ProjectionDone
        {
            private ProjectionDone()
            {
                
            }
            public static ProjectionDone Instance = new ProjectionDone();
        }        
        
        public class ProjectEvents<T>
        {
            public T[] Events { get; }

            public ProjectEvents(string calculatorId, params T[] events)
            {
                Events = events;
            }
        }
      
     //   public override string PersistenceId { get; }
    }
    //one per calculatorId
    public class CalculatorFunctionUsageProjector : ReceivePersistentActor
    {
        public CalculatorFunctionUsageProjector()
        {
            PersistenceId = Self.Path.Name;
            int lastFunctionUsageProjected = 0;
            int lastFunctionAddProjected = 0;

            var dependencies = Context.System.GetReportingExtension().GetDependencies();
            Func<DbContext> contextFactory = () => dependencies.CreateFunctionUsageContext();
            
            Command<ProjectEvents<CalculatorActor.FunctionAdded>>(e =>
            {
                   
            });
            Command<ProjectEvents<CalculatorActor.CalculationPerformed>>(e =>
            {
                
            });
            Recover<FunctionUsageProjected>(r => lastFunctionUsageProjected = r.SequenceTo);
            Recover<FunctionAddProjected>(r => lastFunctionAddProjected = r.SequenceTo);
        }

        public class ProjectEvents<T>
        {
            public T[] Events { get; }

            public ProjectEvents(string calculatorId, params T[] events)
            {
                Events = events;
            }
        }
        public class FunctionUsageProjected:Projected
        {
            protected FunctionUsageProjected(string calculatorId, string projectionId, int sequenceFrom, int sequenceTo) : base(calculatorId, projectionId, nameof(CalculatorActor.CalculationPerformed),sequenceFrom, sequenceTo)
            {
            } 
        }

        public class FunctionAddProjected:Projected
        {
            protected FunctionAddProjected(string calculatorId, string projectionId, int sequenceFrom, int sequenceTo) : base(calculatorId, projectionId, nameof(CalculatorActor.FunctionAdded),sequenceFrom, sequenceTo)
            {
            }
        }
        
        public override string PersistenceId { get; }
    }

    
    public class Projected
    {
        public string CalculatorId { get; }
        public string ProjectionId { get; }
        public int SequenceFrom { get; }
        public int SequenceTo { get; }
        public string EventName { get; }

        protected Projected(string calculatorId, string projectionId, string eventName,  int sequenceFrom, int sequenceTo)
        {
            CalculatorId = calculatorId;
            ProjectionId = projectionId;
            SequenceFrom = sequenceFrom;
            SequenceTo = sequenceTo;
            EventName = eventName;
        }
    }
    
    public class ReportingActor : ReceiveActor
    {
        public ReportingActor()
        {
            // obtain read journal by plugin id
            var readJournal = PersistenceQuery.Get(Context.System).ReadJournalFor<SqlReadJournal>(SqlReadJournal.Identifier);
            
            var stream = readJournal.EventsByTag(nameof(CalculatorActor.CalculationPerformed));
           //ar words = Source.Empty<string>();
           var counts = stream
               .SelectMany(e => (e.Event as CalculatorActor.CalculationPerformed)?.FunctionsUsed)
               // split the words into separate streams first
               .GroupBy(1000000, f => f )
               //transform each element to pair with number of words in it
               .Select(f => new TotalFunctionUsage(){ FunctionName = f, InvocationsCount = 1})
               // add counting logic to the streams
               .Sum((l, r) =>  new TotalFunctionUsage(){ FunctionName = l.FunctionName, InvocationsCount = l.InvocationsCount + r.InvocationsCount})
               // get a stream of word counts
               .MergeSubstreams();

           var sink = Sink.ActorRef<TotalFunctionUsage>(Context.ActorOf(Props.Create<OverallFunctionUsageProjector>(),"overallUsageProjector"),
               OverallFunctionUsageProjector.ProjectionDone.Instance);
           
           sink.RunWith(counts,)

           using (var materializer = Context.System.Materializer())
           {
              counts.ViaMaterialized() 
           } 
        }
    }
}