using System;
using System.Diagnostics.Tracing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using MightyCalc.Client;
using Serilog;
using Serilog.Events;

namespace MightyCalc.API.Tests
{
    public class UserFunctionLocalTests : UserFunctionTests
    {
        protected override IMightyCalcClient CreateClient()
        {
            
            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File($"api_{DateTime.Now:yyyy-MMM-dd-hh-mm-ss}.log")
                .CreateLogger();
            
            var builder = new WebHostBuilder()
                .UseEnvironment("Development")
                .UseStartup<LocalStartup>()
                .UseSerilog(logger);

            var server = new TestServer(builder);
            return new MightyCalcClient("",server.CreateClient());
        }
    }
}