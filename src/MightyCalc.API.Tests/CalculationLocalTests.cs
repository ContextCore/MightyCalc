using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using MightyCalc.Client;

namespace MightyCalc.API.Tests
{
    public class CalculationLocalTests:CalculationTests
    {
        protected override IMightyCalcClient CreateClient()
        {
            var builder = new WebHostBuilder()
                .UseEnvironment("Development")
                .UseStartup<Startup>(); 
            
            var server = new TestServer(builder);
            var httpClient = server.CreateClient();
            return new MightyCalcClient("", httpClient);
        }
    }
}