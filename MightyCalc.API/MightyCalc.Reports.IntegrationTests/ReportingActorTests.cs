using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Akka.Remote;
using Akka.TestKit.Xunit2;
using Autofac;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MightyCalc.Node;
using MightyCalc.Reports.DatabaseProjections;
using MightyCalc.Reports.ReportingExtension;
using Xunit;
using Xunit.Abstractions;

namespace MightyCalc.Reports.IntegrationTests
{
	public class ReportingActorTests : TestKit
    {
        private readonly ITestOutputHelper _output;

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
	        max-buffer-size = 10
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

        private async Task ResetDB()
        {
            await DbTools.TruncateTables(KnownConnectionStrings.ReadModel,
                "Projections",
                "FunctionsUsage",
                "FunctionsTotalUsage");
            await DbTools.TruncateTables(KnownConnectionStrings.Journal, "event_journal","metadata");
            await DbTools.TruncateTables(KnownConnectionStrings.SnapshotStore, "snapshot_store");
        }

        public ReportingActorTests(ITestOutputHelper output) : base(GetAkkaConfig(), "Test", output)
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


        [Fact]
        public async Task DbRest_should_work()
        {
            await Init();
        }

        [Fact]
        public async Task Given_actor_When_sending_start_message_Then_projection_launched_producing_data()
        {
            var dep = await Init();

            _output.WriteLine(Sys.Settings.ToString());
            //generate some data
   
            var reportActor = Sys.ActorOf(Props.Create<ReportingActor>(), "reportingActor");

            reportActor.Tell(ReportingActor.Start.Instance);
            
            var calculationActor = Sys.ActorOf(Props.Create<CalculatorActor>(), "CalculatorOne");
            calculationActor.Tell(new CalculatorActorProtocol.CalculateExpression("1+2-3"));
            calculationActor.Tell(new CalculatorActorProtocol.CalculateExpression("1-2*3"));
            calculationActor.Tell(new CalculatorActorProtocol.CalculateExpression("1/2+3"));

            await Task.Delay(20000);
            
            var projected = new FindProjectionQuery(dep.CreateFunctionUsageContext()).ExecuteForFunctionsTotalUsage();
            Assert.Equal(projected.Sequence, 3);

            var usage = await new FunctionsTotalUsageQuery(dep.CreateFunctionUsageContext()).Execute();

            usage.Should().BeEquivalentTo(
                new FunctionTotalUsage {FunctionName = "AdditionSigned", InvocationsCount = 2},
                new FunctionTotalUsage {FunctionName = "SubtractSigned", InvocationsCount = 2},
                new FunctionTotalUsage {FunctionName = "MultiplyChecked", InvocationsCount = 1},
                new FunctionTotalUsage {FunctionName = "Divide", InvocationsCount = 1});
        }

        [Fact]
        public async Task Given_existing_projection_When_starting_it_Then_projection_is_resumed()
        {
            var dep = await Init();

            var reportActor = Sys.ActorOf(Props.Create<ReportingActor>(), "reportingActor");

            reportActor.Tell(ReportingActor.Start.Instance);

            //generate some data

            var calculationActor = Sys.ActorOf(Props.Create<CalculatorActor>(), "CalculatorOne");
            calculationActor.Tell(new CalculatorActorProtocol.CalculateExpression("1+2-3"));
            calculationActor.Tell(new CalculatorActorProtocol.CalculateExpression("1-2*3"));
            calculationActor.Tell(new CalculatorActorProtocol.CalculateExpression("1/2+3"));

            await Task.Delay(3000);

            var sys = ActorSystem.Create("sys");

            reportActor = sys.ActorOf(Props.Create<ReportingActor>(), "reportingActor");

            reportActor.Tell(ReportingActor.Start.Instance);

            //add some data on top of existing projection some data

            calculationActor = sys.ActorOf(Props.Create<CalculatorActor>(), "CalculatorOne");
            calculationActor.Tell(new CalculatorActorProtocol.CalculateExpression("1+2-3*4/5"));

            await Task.Delay(3000);

            var projected = new FindProjectionQuery(dep.CreateFunctionUsageContext()).ExecuteForFunctionsTotalUsage();
            Assert.Equal(projected.Sequence, 4);

            var usage = await new FunctionsTotalUsageQuery(dep.CreateFunctionUsageContext()).Execute();

            usage.Should().BeEquivalentTo(
                new FunctionTotalUsage {FunctionName = "AdditionSigned", InvocationsCount = 3},
                new FunctionTotalUsage {FunctionName = "SubtractSigned", InvocationsCount = 3},
                new FunctionTotalUsage {FunctionName = "MultiplyChecked", InvocationsCount = 2},
                new FunctionTotalUsage {FunctionName = "Divide", InvocationsCount = 2});
        }
    }
}