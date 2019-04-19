using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using MightyCalc.API.Tests;
using MightyCalc.Client;
using MightyCalc.IntegrationTests.Tools;
using Xunit;
using Xunit.Abstractions;

namespace MightyCalc.API.IntegrationTests
{

    public class CalculationRemoteTests:CalculationTests
    {
        private readonly ITestOutputHelper _output;

        public CalculationRemoteTests(ITestOutputHelper output)
        {
            _output = output;
        }
        protected override IMightyCalcClient CreateClient()
        {
            DbTools.ResetDatabases(false).Wait();
            
            var url = Environment.GetEnvironmentVariable("MightyCalc_ApiUrl") ?? "http://localhost:30010";

            //disabling https checks
            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            
            var httpClient = new HttpClient(httpClientHandler);
            
            return new MightyCalcClient(url, httpClient);
        }
        
        
        [Theory]
        [InlineData(new[]{"1+1.5"},"AddChecked",1)] //add
        [InlineData(new[]{"1+1.5+1","0+0+0+1"},"AddChecked",5)] //add
        [InlineData(new[]{"Pow(2,1) - 23","23 - 4 - 1"},"Pow",1,"SubtractChecked",3)] //add
        public async Task Given_expressions_calculated_When_getting_stats_Then_it_should_be_presented(string[] expressions, params object[] expectedStats)
        {
            var expectedUsage = new Dictionary<string,int>();
            var enumerator = expectedStats.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var name = (string) enumerator.Current;
                enumerator.MoveNext();
                var count = (int) enumerator.Current;
                expectedUsage.Add(name,count);
            }

            var beforeCalculationReport = await Client.UsageTotalStatsAsync();

            foreach (var expression in expressions)
            {
                await Client.CalculateAsync(new Client.Expression{Representation = expression});
            }

            await Task.Delay(10000); // for projection
            
            var report = await Client.UsageTotalStatsAsync();

            foreach (var expected in expectedUsage)
            {
                var usageBefore = beforeCalculationReport.UsageStatistics.FirstOrDefault(u => u.Name == expected.Key)
                                     ?.UsageCount ?? 0;
                
                var usageAfterCalculation = report.UsageStatistics.FirstOrDefault(u => u.Name == expected.Key)
                                                ?.UsageCount ?? 0;
                
                expected.Value.Should().BeGreaterOrEqualTo(usageAfterCalculation - usageBefore);
            }
        }

    }
}