using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MightyCalc.Reports.DatabaseProjections;
using Xunit;

namespace MightyCalc.Reports.Tests
{
    public class FunctionUsageQueryTests
    {
        [Fact]
        public async Task Given_empty_context_When_executing_query_Then_it_should_return_nothing()
        {
            var options = new DbContextOptionsBuilder<FunctionUsageContext>()
                .UseInMemoryDatabase(nameof(FunctionUsageQueryTests) +
                                     nameof(Given_empty_context_When_executing_query_Then_it_should_return_nothing))
                .Options;

            var context = new FunctionUsageContext(options);
            var query = new FunctionsUsageQuery(context);

            var res = await query.Execute("");
            Assert.Empty(res);
        }

        [Fact]
        public async Task Given_context_with_data_When_executing_query_without_filter_Then_it_returns_everything()
        {
            var options = new DbContextOptionsBuilder<FunctionUsageContext>()
                .UseInMemoryDatabase(nameof(FunctionUsageQueryTests) +
                                     nameof(
                                         Given_context_with_data_When_executing_query_without_filter_Then_it_returns_everything
                                     )).Options;

            var context = new FunctionUsageContext(options);
            var query = new FunctionsUsageQuery(context);
            var periodStart = DateTimeOffset.Now;
            var functions = new[]
            {
                new FunctionUsage
                {
                    FunctionName = "testFunc",
                    InvocationsCount = 1,
                    CalculatorName = "calcA",
                    Period = TimeSpan.FromMinutes(1),
                    PeriodStart = periodStart,
                    PeriodEnd = periodStart + TimeSpan.FromMinutes(1)
                },
                new FunctionUsage
                {
                    FunctionName = "myFunc",
                    InvocationsCount = 2,
                    CalculatorName = "calcB",
                    Period = TimeSpan.FromHours(1),
                    PeriodStart = periodStart,
                    PeriodEnd = periodStart + TimeSpan.FromHours(1)
                },
            };
            context.FunctionsUsage.AddRange(functions);
            await context.SaveChangesAsync();

            var res = await query.Execute();

            Assert.Equal(functions, res);
        }


        [Fact]
        public async Task Given_context_with_data_When_executing_query_with_filter_Then_it_returns_filtered_data()
        {
            var options = new DbContextOptionsBuilder<FunctionUsageContext>()
                .UseInMemoryDatabase(nameof(FunctionUsageQueryTests) +
                                     nameof(
                                         Given_context_with_data_When_executing_query_with_filter_Then_it_returns_filtered_data
                                     )).Options;

            var context = new FunctionUsageContext(options);
            var query = new FunctionsUsageQuery(context);
            var periodStart = DateTimeOffset.Now;

            var functions = new[]
            {
                new FunctionUsage
                {
                    FunctionName = "testFunc",
                    InvocationsCount = 1,
                    CalculatorName = "calcA",
                    Period = TimeSpan.FromMinutes(1),
                    PeriodStart = periodStart,
                    PeriodEnd = periodStart + TimeSpan.FromMinutes(1)
                },
                new FunctionUsage
                {
                    FunctionName = "myFunc",
                    InvocationsCount = 2,
                    CalculatorName = "calcB",
                    Period = TimeSpan.FromHours(1),
                    PeriodStart = periodStart,
                    PeriodEnd = periodStart + TimeSpan.FromHours(1)
                },
                new FunctionUsage
                {
                    FunctionName = "myFunc",
                    InvocationsCount = 2,
                    CalculatorName = "calcB",
                    Period = TimeSpan.FromHours(1),
                    PeriodStart = periodStart - TimeSpan.FromHours(1),
                    PeriodEnd = periodStart
                },
            };
            context.FunctionsUsage.AddRange(functions);
            await context.SaveChangesAsync();

            var res = await query.Execute("calcB",DateTimeOffset.Now - TimeSpan.FromHours(2), DateTimeOffset.Now);

            Assert.Equal(functions[2], res.Single());
        }
    }
}