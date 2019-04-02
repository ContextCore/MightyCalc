using System;
using System.Net.Http;
using MightyCalc.API.Tests;
using MightyCalc.Client;
using MightyCalc.IntegrationTests.Tools;

namespace MightyCalc.API.IntegrationTests
{
    public class UserFunctionRemoteTests : UserFunctionTests
    {
        protected override IMightyCalcClient CreateClient()
        {
            DbTools.ResetDatabases(false).Wait();

            var url = Environment.GetEnvironmentVariable("MightyCalc_ApiUrl") ?? "http://localhost:5000";

            //disabling https checks
            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            
            var httpClient = new HttpClient(httpClientHandler);
            
            return new MightyCalcClient(url, httpClient);
        }
    }
}