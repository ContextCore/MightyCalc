using System;
using System.Net.Http;
using Microsoft.AspNetCore.TestHost;
using MightyCalc.API.Tests;
using MightyCalc.Client;
using Xunit;

namespace MightyCalc.API.IntegrationTests
{
    public class CalculationRemoteTests:CalculationTests
    {
        protected override IMightyCalcClient CreateClient()
        {
            var url = Environment.GetEnvironmentVariable("MightyCalc_ApiUrl") ?? "http://localhost:80";
            return new MightyCalcClient(url, new HttpClient());
        }
    }
    
    public class UserFunctionRemoteTests : UserFunctionTests
    {
        protected override IMightyCalcClient CreateClient()
        {
            var url = Environment.GetEnvironmentVariable("MightyCalc_ApiUrl") ?? "http://localhost:80";
            return new MightyCalcClient(url,new HttpClient());
        }

        public UserFunctionRemoteTests()
        {
            
        }
    }
}