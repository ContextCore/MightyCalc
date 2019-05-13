using System;
using System.IO;
using System.Reflection;
using Akka.Actor;
using Akka.Cluster;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using MightyCalc.Client;
using MightyCalc.Node;
using MightyCalc.Reports;
using MightyCalc.Reports.DatabaseProjections;
using MightyCalc.Reports.ReportingExtension;
using Serilog;
using Serilog.Events;
using Swashbuckle.AspNetCore.Swagger;

namespace MightyCalc.API.Tests
{

    public class CalculationLocalTests : CalculationTests
    {
        protected override IMightyCalcClient CreateClient()
        {
            
            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File($"api_{DateTime.Now:yyyy-MMM-dd-HH-mm-ss}.log")
                .CreateLogger();
            
            var builder = new WebHostBuilder()
                .UseEnvironment("Development")
                .UseStartup<LocalStartup>()
                .UseSerilog(logger); 
            
            var server = new TestServer(builder);
            
            var httpClient = server.CreateClient();
            return new MightyCalcClient("", httpClient);
        }
    }
}