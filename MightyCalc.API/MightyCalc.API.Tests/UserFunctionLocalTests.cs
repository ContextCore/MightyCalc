using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using MightyCalc.Client;

namespace MightyCalc.API.Tests
{
    public class UserFunctionLocalTests : UserFunctionTests
    {
        protected override IMightyCalcClient CreateClient()
        {
            var builder = new WebHostBuilder()
                .UseEnvironment("Development")
                .UseStartup<Startup>();

            var server = new TestServer(builder);
            return new MightyCalcClient("",server.CreateClient());
        }
    }
}