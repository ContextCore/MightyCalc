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
using Swashbuckle.AspNetCore.Swagger;

namespace MightyCalc.API.Tests
{

    public class CalculationLocalTests : CalculationTests
    {
        protected override IMightyCalcClient CreateClient()
        {
            var builder = new WebHostBuilder()
                .UseEnvironment("Development")
                .UseStartup<LocalStartup>(); 
            
            var server = new TestServer(builder);
            
            var httpClient = server.CreateClient();
            return new MightyCalcClient("", httpClient);
        }
    }
}