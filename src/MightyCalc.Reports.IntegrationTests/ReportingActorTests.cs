using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster;
using Akka.Configuration;
using Akka.Remote;
using Akka.TestKit.Xunit2;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MightyCalc.IntegrationTests.Tools;
using MightyCalc.Node;
using MightyCalc.Node.Akka;
using MightyCalc.Reports.DatabaseProjections;
using MightyCalc.Reports.ReportingExtension;
using MightyCalc.Reports.Streams;
using Newtonsoft.Json;
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
akka.actor.provider = ""Akka.Cluster.ClusterActorRefProvider, Akka.Cluster""
akka.cluster.roles=[api, projection, calculation]
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
        plugin = ""akka.persistence.journal.postgresql""
		postgresql {
			# qualified type name of the PostgreSql persistence journal actor
			class = ""Akka.Persistence.PostgreSql.Journal.PostgreSqlJournal, Akka.Persistence.PostgreSql""

		    event-adapters {
                 tagging = """ + typeof(DomainEventAdapter).AssemblyQualifiedName + @"""
              }

            event-adapter-bindings {
                """ + typeof(IDomainEvent).AssemblyQualifiedName + @""" = tagging
            }
			# connection string used for database access
			connection-string = """ + KnownConnectionStrings.Journal + @"""

			
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
			stored-as = bytea

			# Setting used to toggle sequential read access when loading large objects
			# from journals and snapshot stores.
			sequential-access = off
		}
	}

	snapshot-store {
		plugin = ""akka.persistence.snapshot-store.postgresql""
		postgresql {
			# qualified type name of the PostgreSql persistence journal actor
			class = ""Akka.Persistence.PostgreSql.Snapshot.PostgreSqlSnapshotStore, Akka.Persistence.PostgreSql""

			# connection string used for database access
			connection-string = """ + KnownConnectionStrings.SnapshotStore + @"""

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
            await DbTools.ResetDatabases();

            var cluster = Cluster.Get(Sys);
            cluster.Join(((ExtendedActorSystem) Sys).Provider.DefaultAddress);
            var ext = Sys.InitReportingExtension(new ReportingDependencies(options));
            ext.Start();
            return Sys.GetReportingExtension().GetDependencies();
        }


        [Fact]
        public async Task DbRest_should_work()
        {
            await Init();
        }

        [Fact]
        public async Task
            Given_ReportActor_When_sending_start_message_Then_total_usage_projection_launched_producing_data()
        {
            var dep = await Init();

            _output.WriteLine(Sys.Settings.ToString());
            //generate some data

            var calculationActor = Sys.ActorOf(Props.Create<CalculatorActor>(), "CalculatorOne");
            calculationActor.Tell(new CalculatorActorProtocol.CalculateExpression("1+2-3"));
            calculationActor.Tell(new CalculatorActorProtocol.CalculateExpression("1-2*3"));
            calculationActor.Tell(new CalculatorActorProtocol.CalculateExpression("1/2+3"));

            await Task.Delay(10000);

            var projected = new FindProjectionQuery(dep.CreateFunctionUsageContext()).ExecuteForFunctionsTotalUsage();
            Assert.Equal(3, projected.Sequence);

            var usage = await new FunctionsTotalUsageQuery(dep.CreateFunctionUsageContext()).Execute();

            usage.Should().BeEquivalentTo(
                new FunctionTotalUsage {FunctionName = "AddChecked", InvocationsCount = 2},
                new FunctionTotalUsage {FunctionName = "SubtractChecked", InvocationsCount = 2},
                new FunctionTotalUsage {FunctionName = "MultiplyChecked", InvocationsCount = 1},
                new FunctionTotalUsage {FunctionName = "Divide", InvocationsCount = 1});
        }


        [Fact]
        public async Task Given_ReportActor_When_sending_start_message_Then_usage_projection_launched_producing_data()
        {
            var dep = await Init();

            _output.WriteLine(Sys.Settings.ToString());
            //generate some data
            var now = DateTimeOffset.Now;
            var periodStart = now.ToMinutePeriodBegin();
            var periodEnd = now.ToMinutePeriodEnd();
            var oneMinute = TimeSpan.FromMinutes(1);

            var calculatorName = "CalculatorOne";
            var calculationActor = Sys.ActorOf(Props.Create<CalculatorActor>(), calculatorName);
            calculationActor.Tell(new CalculatorActorProtocol.CalculateExpression("1+2-3"));
            calculationActor.Tell(new CalculatorActorProtocol.CalculateExpression("1-2*3"));
            calculationActor.Tell(new CalculatorActorProtocol.CalculateExpression("1/2+3"));

            await Task.Delay(10000);

            var projected = new FindProjectionQuery(dep.CreateFunctionUsageContext()).ExecuteForFunctionsUsage();
            Assert.Equal(3, projected.Sequence);

            var usage = await new FunctionsUsageQuery(dep.CreateFunctionUsageContext()).Execute(calculatorName);

            usage.Should().BeEquivalentTo(
                new FunctionUsage
                {
                    FunctionName = "AddChecked", InvocationsCount = 2, CalculatorName = calculatorName,
                    Period = oneMinute, PeriodEnd = periodEnd, PeriodStart = periodStart
                },
                new FunctionUsage
                {
                    FunctionName = "SubtractChecked", CalculatorName = calculatorName, InvocationsCount = 2,
                    Period = oneMinute, PeriodEnd = periodEnd, PeriodStart = periodStart
                },
                new FunctionUsage
                {
                    FunctionName = "MultiplyChecked", CalculatorName = calculatorName, InvocationsCount = 1,
                    Period = oneMinute, PeriodEnd = periodEnd, PeriodStart = periodStart
                },
                new FunctionUsage
                {
                    FunctionName = "Divide", CalculatorName = calculatorName, InvocationsCount = 1, Period = oneMinute,
                    PeriodEnd = periodEnd, PeriodStart = periodStart
                });
        }

          [Fact]
        public async Task Given_existing_usage_projection_When_starting_it_Then_projection_is_resumed()
        {
            var dep = await Init();

            //generate some data

            var calculatorName = "CalculatorOne";
            var calculationActor = Sys.ActorOf(Props.Create<CalculatorActor>(), calculatorName);
            
            var now = DateTimeOffset.Now;
            var periodStart = now.ToMinutePeriodBegin();
            var periodEnd = now.ToMinutePeriodEnd();
            var oneMinute = TimeSpan.FromMinutes(1);

            calculationActor.Tell(new CalculatorActorProtocol.CalculateExpression("1+2-3"));
            calculationActor.Tell(new CalculatorActorProtocol.CalculateExpression("1-2*3"));
            calculationActor.Tell(new CalculatorActorProtocol.CalculateExpression("1/2+3"));


            await Task.Delay(5000);

            //ensure it is persisted

            var projected = new FindProjectionQuery(dep.CreateFunctionUsageContext()).ExecuteForFunctionsUsage();
            Assert.Equal(3, projected.Sequence);

            var reportActor = Sys.GetReportingExtension().ReportingActor;
            //restart report actor to allow it to get latest projected sequence from DB
            Watch(reportActor);
            Sys.Stop(reportActor);
            FishForMessage<Terminated>(t => t.ActorRef == reportActor);
            await Task.Delay(1000);

            reportActor = Sys.ActorOf(Props.Create<ReportingActor>(), "reportingActor");
            reportActor.Tell(ReportingActor.Start.Instance);

            var usage = await new FunctionsUsageQuery(dep.CreateFunctionUsageContext()).Execute(calculatorName);

            usage.Should().BeEquivalentTo(
	            new FunctionUsage
	            {
		            FunctionName = "AddChecked", InvocationsCount = 2, CalculatorName = calculatorName,
		            Period = oneMinute, PeriodEnd = periodEnd, PeriodStart = periodStart
	            },
	            new FunctionUsage
	            {
		            FunctionName = "SubtractChecked", CalculatorName = calculatorName, InvocationsCount = 2,
		            Period = oneMinute, PeriodEnd = periodEnd, PeriodStart = periodStart
	            },
	            new FunctionUsage
	            {
		            FunctionName = "MultiplyChecked", CalculatorName = calculatorName, InvocationsCount = 1,
		            Period = oneMinute, PeriodEnd = periodEnd, PeriodStart = periodStart
	            },
	            new FunctionUsage
	            {
		            FunctionName = "Divide", CalculatorName = calculatorName, InvocationsCount = 1, Period = oneMinute,
		            PeriodEnd = periodEnd, PeriodStart = periodStart
	            });
            
            now = DateTimeOffset.Now;
            periodStart = now.ToMinutePeriodBegin();
            periodEnd = now.ToMinutePeriodEnd();
            //add new events
            calculationActor.Tell(new CalculatorActorProtocol.CalculateExpression("1+2-3*4/5"));

            await Task.Delay(5000);

            //check the results
            projected = new FindProjectionQuery(dep.CreateFunctionUsageContext()).ExecuteForFunctionsTotalUsage();
            Assert.Equal(4, projected.Sequence);

            usage = await new FunctionsUsageQuery(dep.CreateFunctionUsageContext()).Execute(calculatorName);

            usage.Should().BeEquivalentTo(
	            new FunctionUsage
	            {
		            FunctionName = "AddChecked", InvocationsCount = 3, CalculatorName = calculatorName,
		            Period = oneMinute, PeriodEnd = periodEnd, PeriodStart = periodStart
	            },
	            new FunctionUsage
	            {
		            FunctionName = "SubtractChecked", CalculatorName = calculatorName, InvocationsCount = 3,
		            Period = oneMinute, PeriodEnd = periodEnd, PeriodStart = periodStart
	            },
	            new FunctionUsage
	            {
		            FunctionName = "MultiplyChecked", CalculatorName = calculatorName, InvocationsCount = 2,
		            Period = oneMinute, PeriodEnd = periodEnd, PeriodStart = periodStart
	            },
	            new FunctionUsage
	            {
		            FunctionName = "Divide", CalculatorName = calculatorName, InvocationsCount = 2, Period = oneMinute,
		            PeriodEnd = periodEnd, PeriodStart = periodStart
	            });
        }

        [Fact]
        public async Task Given_existing_total_usage_projection_When_starting_it_Then_projection_is_resumed()
        {
            var dep = await Init();

            //generate some data

            var calculationActor = Sys.ActorOf(Props.Create<CalculatorActor>(), "CalculatorOne");
            calculationActor.Tell(new CalculatorActorProtocol.CalculateExpression("1+2-3"));
            calculationActor.Tell(new CalculatorActorProtocol.CalculateExpression("1-2*3"));
            calculationActor.Tell(new CalculatorActorProtocol.CalculateExpression("1/2+3"));


            await Task.Delay(5000);

            //ensure it is persisted

            var projected = new FindProjectionQuery(dep.CreateFunctionUsageContext()).ExecuteForFunctionsTotalUsage();
            Assert.Equal(3, projected.Sequence);

            var reportActor = Sys.GetReportingExtension().ReportingActor;
            //restart report actor to allow it to get latest projected sequence from DB
            Watch(reportActor);
            Sys.Stop(reportActor);
            FishForMessage<Terminated>(t => t.ActorRef == reportActor);
            await Task.Delay(1000);

            reportActor = Sys.ActorOf(Props.Create<ReportingActor>(), "reportingActor");
            reportActor.Tell(ReportingActor.Start.Instance);

            var usage = await new FunctionsTotalUsageQuery(dep.CreateFunctionUsageContext()).Execute();

            usage.Should().BeEquivalentTo(
                new FunctionTotalUsage {FunctionName = "AddChecked", InvocationsCount = 2},
                new FunctionTotalUsage {FunctionName = "SubtractChecked", InvocationsCount = 2},
                new FunctionTotalUsage {FunctionName = "MultiplyChecked", InvocationsCount = 1},
                new FunctionTotalUsage {FunctionName = "Divide", InvocationsCount = 1});

            //add new events
            calculationActor.Tell(new CalculatorActorProtocol.CalculateExpression("1+2-3*4/5"));

            await Task.Delay(5000);

            //check the results
            projected = new FindProjectionQuery(dep.CreateFunctionUsageContext()).ExecuteForFunctionsTotalUsage();
            Assert.Equal(4, projected.Sequence);

            usage = await new FunctionsTotalUsageQuery(dep.CreateFunctionUsageContext()).Execute();

            usage.Should().BeEquivalentTo(
                new FunctionTotalUsage {FunctionName = "AddChecked", InvocationsCount = 3},
                new FunctionTotalUsage {FunctionName = "SubtractChecked", InvocationsCount = 3},
                new FunctionTotalUsage {FunctionName = "MultiplyChecked", InvocationsCount = 2},
                new FunctionTotalUsage {FunctionName = "Divide", InvocationsCount = 2});
        }
    }
}