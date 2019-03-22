using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Akka.Persistence.Query;
using Akka.Persistence.Query.Sql;
using Akka.Streams;
using Akka.Streams.Dsl;
using Akka.TestKit.Xunit2;
using Autofac;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MightyCalc.Node;
using MightyCalc.Reports.DatabaseProjections;
using MightyCalc.Reports.ReportingExtension;
using MightyCalc.Reports.Streams;
using Xunit;
using Xunit.Abstractions;


namespace MightyCalc.Reports.IntegrationTests
{
    public class JournalTests: TestKit
    {
        private readonly ITestOutputHelper _output;

        public JournalTests(ITestOutputHelper output) : base(GetAkkaConfig(), "Test", output)
        {
            _output = output;
        }

        private async Task<IReportingDependencies> Init()
        {
            var options = new DbContextOptionsBuilder<FunctionUsageContext>()
                .UseNpgsql(KnownConnectionStrings.ReadModel)
                .EnableSensitiveDataLogging()
                .Options;
            await ResetDB();

            var container = new ContainerBuilder();
            container.RegisterInstance<IReportingDependencies>(new ReportingDependencies(options));
            Sys.InitReportingExtension(container.Build());
            return Sys.GetReportingExtension().GetDependencies();
        }
		 
        private static Config GetAkkaConfig()
        {
            return akkaConfig.WithFallback(FullDebugConfig);
        }

        private static Config akkaConfig = @"
akka.persistence.journal.plugin = ""akka.persistence.journal.postgresql""
akka.persistence.snapshot-store.plugin = ""akka.persistence.snapshot-store.postgresql""

akka.persistence.query.journal.sql {
		# Implementation class of the SQL ReadJournalProvider
			 class = ""Akka.Persistence.Query.Sql.SqlReadJournalProvider, Akka.Persistence.Query.Sql""
  
		# Absolute path to the write journal plugin configuration entry that this 
		# query journal will connect to. 
		# If undefined (or """") it will connect to the default journal as specified by the
		# akka.persistence.journal.plugin property.
        #write-plugin = """"
  
		# The SQL write journal is notifying the query side as soon as things
		# are persisted, but for efficiency reasons the query side retrieves the events 
		# in batches that sometimes can be delayed up to the configured `refresh-interval`.

        refresh-interval = 1s
  
		# How many events to fetch in one query (replay) and keep buffered until they
		# are delivered downstreams.
	        max-buffer-size = 1
    }

akka.persistence{
	journal {
		postgresql {
			# qualified type name of the PostgreSql persistence journal actor
			class = ""Akka.Persistence.PostgreSql.Journal.PostgreSqlJournal, Akka.Persistence.PostgreSql""

		    event-adapters {
                 tagging = """+typeof(DomainEventAdapter).AssemblyQualifiedName+@"""
              }

            event-adapter-bindings {
                """+typeof(IDomainEvent).AssemblyQualifiedName+@""" = tagging
            }
			# connection string used for database access
			connection-string = """+KnownConnectionStrings.Journal+@"""

			
			# default SQL commands timeout
			connection-timeout = 30s

			# PostgreSql schema name to table corresponding with persistent journal
			schema-name = public

			# PostgreSql table corresponding with persistent journal
			table-name = event_journal

			# should corresponding journal table be initialized automatically
			auto-initialize = on
			
			# metadata table
			metadata-table-name = metadata

			# Postgres data type for payload column. Allowed options: bytea, json, jsonb
			stored-as = json

			# Setting used to toggle sequential read access when loading large objects
			# from journals and snapshot stores.
			sequential-access = off
		}
	}

	snapshot-store {
		postgresql {
			# qualified type name of the PostgreSql persistence journal actor
			class = ""Akka.Persistence.PostgreSql.Snapshot.PostgreSqlSnapshotStore, Akka.Persistence.PostgreSql""

			# connection string used for database access
			connection-string = """+KnownConnectionStrings.SnapshotStore+@"""

			# PostgreSql schema name to table corresponding with persistent journal
			schema-name = public

			# PostgreSql table corresponding with persistent journal
			table-name = snapshot_store

			# should corresponding journal table be initialized automatically
			auto-initialize = on
		}
	}
}
";
        [Fact]
        public async Task Given_journal_When_starting_projection_stream_Then_projection_launched_producing_data()
        {
            var dep = await Init();

            _output.WriteLine(Sys.Settings.ToString());
            //generate some data
            var calculationActor = Sys.ActorOf(Props.Create<CalculatorActor>(), "CalculatorOne");
            calculationActor.Tell(new CalculatorActorProtocol.CalculateExpression("1+2-3"));
            calculationActor.Tell(new CalculatorActorProtocol.CalculateExpression("1-2*3"));
            calculationActor.Tell(new CalculatorActorProtocol.CalculateExpression("1/2+3"));

            var eventName = nameof(CalculatorActor.CalculationPerformed);
            
            var readJournal = PersistenceQuery.Get(Sys).ReadJournalFor<SqlReadJournal>(SqlReadJournal.Identifier);
            var sharedKillSwitch = KillSwitches.Shared("test");
            var source = readJournal.EventsByTag(eventName, Offset.NoOffset())
	            .Via(sharedKillSwitch.Flow<EventEnvelope>());
            
            var flow = FunctionTotalUsageFlow.Instance;
            var sink = FunctionTotalUsageSink.Create(Sys, eventName);
	            
            source.Via(flow).To(sink).Run(Sys.Materializer());
	        
            await Task.Delay(5000);
            
            var projected = new FindProjectionQuery(dep.CreateFunctionUsageContext()).ExecuteForFunctionsTotalUsage();
            Assert.Equal(3,projected.Sequence);

            var usage = await new FunctionsTotalUsageQuery(dep.CreateFunctionUsageContext()).Execute();

            usage.Should().BeEquivalentTo(
                new FunctionTotalUsage {FunctionName = "AddChecked", InvocationsCount = 2},
                new FunctionTotalUsage {FunctionName = "SubtractChecked", InvocationsCount = 2},
                new FunctionTotalUsage {FunctionName = "MultiplyChecked", InvocationsCount = 1},
                new FunctionTotalUsage {FunctionName = "Divide", InvocationsCount = 1});
        }  
        
        [Fact]
        public async Task Given_journal_When_starting_projection_stream_Then_sink_receives_all_events()
        {
            await Init();

            _output.WriteLine(Sys.Settings.ToString());
            //generate some data
            var calculationActor = Sys.ActorOf(Props.Create<CalculatorActor>(), "CalculatorOne");
            calculationActor.Tell(new CalculatorActorProtocol.CalculateExpression("1+2-3"));
            calculationActor.Tell(new CalculatorActorProtocol.CalculateExpression("1-2*3"));
            calculationActor.Tell(new CalculatorActorProtocol.CalculateExpression("1/2+3"));
           
            var eventName = nameof(CalculatorActor.CalculationPerformed);

            var readJournal = PersistenceQuery.Get(Sys).ReadJournalFor<SqlReadJournal>(SqlReadJournal.Identifier);
            
            var sharedKillSwitch = KillSwitches.Shared("test");
            var source = readJournal.EventsByTag(eventName, Offset.NoOffset())
						            .Via(sharedKillSwitch.Flow<EventEnvelope>());
            
            var flow = FunctionTotalUsageFlow.Instance;
            var sink = Sink.Seq<SequencedFunctionUsage>();

            var	materializer = Sys.Materializer();
            var runTask = source.RunWith(flow.ToMaterialized(sink, Keep.Right), materializer);

            await Task.Delay(5000);

            sharedKillSwitch.Shutdown();
            
            var res = await runTask;

            res.Should().HaveCount(6);
        }
        
        [Fact]
        public async Task Given_calculator_executed_commands_When_reading_events_from_journal_Then_it_is_available()
        {
            var dep = await Init();

            _output.WriteLine(Sys.Settings.ToString());
            //generate some data
            var calculationActor = Sys.ActorOf(Props.Create<CalculatorActor>(), "CalculatorOne");
            calculationActor.Tell(new CalculatorActorProtocol.CalculateExpression("1+2-3"));
            calculationActor.Tell(new CalculatorActorProtocol.CalculateExpression("1-2*3"));
            calculationActor.Tell(new CalculatorActorProtocol.CalculateExpression("1/2+3"));
           
            var eventName = nameof(CalculatorActor.CalculationPerformed);

            var readJournal = PersistenceQuery.Get(Sys).ReadJournalFor<SqlReadJournal>(SqlReadJournal.Identifier);

            await Task.Delay(1000);
	        
            var source = readJournal.CurrentEventsByTag(eventName, Offset.NoOffset());

            var sink = Sink.Seq<EventEnvelope>();

            var runTask = source.RunWith(sink, Sys.Materializer());

            var res = await runTask;

            res.Should().NotBeEmpty();
        }

        private async Task ResetDB()
        {
            await DbTools.TruncateTables(KnownConnectionStrings.ReadModel,
                "Projections",
                "FunctionsUsage",
                "FunctionsTotalUsage");
            await DbTools.TruncateTables(KnownConnectionStrings.Journal, "event_journal","metadata");
            await DbTools.TruncateTables(KnownConnectionStrings.SnapshotStore, "snapshot_store");
        }
    }
}