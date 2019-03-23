using System;
using System.Net.Http;
using MightyCalc.API.Tests;
using MightyCalc.Client;

namespace MightyCalc.API.IntegrationTests
{
    public abstract class UserFunctionRemoteTests : UserFunctionTests
    {
        protected override IMightyCalcClient CreateClient()
        {
            var url = Environment.GetEnvironmentVariable("MightyCalc_ApiUrl") ?? "http://localhost:80";
            return new MightyCalcClient(url, new HttpClient());
        }
    }
}