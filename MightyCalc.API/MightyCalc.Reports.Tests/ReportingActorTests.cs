using System;
using System.Reflection.PortableExecutable;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using Autofac;
using Microsoft.EntityFrameworkCore;
using MightyCalc.Reports.DatabaseProjections;
using MightyCalc.Reports.ReportingExtension;
using MightyCalc.Reports.Streams;
using Xunit;
using Xunit.Abstractions;

namespace MightyCalc.Reports.Tests
{
    public class ReportingActorTests:TestKit
    {
        public ReportingActorTests(ITestOutputHelper output):base("",output)
        {
            var container = new ContainerBuilder();
            var options = new DbContextOptionsBuilder<FunctionUsageContext>()
                .UseInMemoryDatabase(nameof(ReportingActorTests)).Options;

            container.RegisterInstance<IReportingDependencies>(new ReportingDependencies(options));

            Sys.InitReportingExtension(container.Build());
        }
        
        [Fact]
        public void Given_actor_When_sending_start_message_Then_projection_launched_producing_data()
        {
         
        }
        
        [Fact]
        public void Given_existing_projection_When_starting_it_Then_projection_is_resumed()
        {
         
        }
    }
}