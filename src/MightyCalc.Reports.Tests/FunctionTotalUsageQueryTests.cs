using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MightyCalc.Calculations;
using MightyCalc.Reports.DatabaseProjections;
using Xunit;

namespace MightyCalc.Reports.Tests
{

    public class KnownFunctionQueryTests
    {
        [Fact]
        public async Task Given_empty_context_When_executing_query_Then_it_should_return_nothing()
        {
            var context = BuildContext(nameof(Given_empty_context_When_executing_query_Then_it_should_return_nothing));
            var query = new KnownFunctionsQuery(context);
            
            var res =  await query.Execute("");
            Assert.Empty(res);
        }

        private FunctionUsageContext BuildContext(string testName)
        {
            var options = new DbContextOptionsBuilder<FunctionUsageContext>()
                .UseInMemoryDatabase(nameof(KnownFunctionQueryTests) +
                                     testName).Options;

            var context = new FunctionUsageContext(options);
            return context;
        }

        [Fact]
        public async Task Given_context_with_data_When_executing_query_without_filter_Then_it_returns_everything()
        {
            var context = BuildContext(nameof(Given_context_with_data_When_executing_query_without_filter_Then_it_returns_everything));

            var functions = new[]
            {
                new KnownFunction {CalculatorId = "calcA", Name="testFunc", Arity = 1, Description = "a description",Expression = "a + 1", Parameters = "a"},
                new KnownFunction {CalculatorId = "calcB", Name="testFunc", Arity = 2, Description = "b description",Expression = "a + b", Parameters = "a;b"},
            };
            
            context.KnownFunctions.AddRange(functions);
            await context.SaveChangesAsync();

            var res =  await new KnownFunctionsQuery(context).Execute();

            res.Should().BeEquivalentTo(
                new FunctionDefinition("testFunc", 1, "a description", "a + 1", "a"),
                new FunctionDefinition("testFunc", 2, "b description", "a + b", "a", "b")
            );
        }


        [Fact]
        public async Task Given_context_with_data_When_executing_query_with_filter_Then_it_returns_filtered_data()
        {
            var context = BuildContext(nameof(Given_context_with_data_When_executing_query_with_filter_Then_it_returns_filtered_data));

            var functions = new[]
            {
                new KnownFunction {CalculatorId = "calcA", Name="testFunc", Arity = 1, Description = "a description",Expression = "a + 1", Parameters = "a"},
                new KnownFunction {CalculatorId = "calcB", Name="testFunc", Arity = 2, Description = "b description",Expression = "a + b", Parameters = "a;b"},
            };
            
            context.KnownFunctions.AddRange(functions);
            await context.SaveChangesAsync();

            var res =  await new KnownFunctionsQuery(context).Execute("calcA","testFunc");

            res.Should().BeEquivalentTo(
                new FunctionDefinition("testFunc", 1, "a description", "a + 1", "a")
            );
        }
    }



    public class FunctionTotalUsageQueryTests
    {

        [Fact]
        public async Task Given_empty_context_When_executing_query_Then_it_should_return_nothing()
        {
            var options = new DbContextOptionsBuilder<FunctionUsageContext>()
                .UseInMemoryDatabase(nameof(Given_empty_context_When_executing_query_Then_it_should_return_nothing)).Options;

            var context = new FunctionUsageContext(options);
            var query = new FunctionsTotalUsageQuery(context);
            
            var res =  await query.Execute("");
            Assert.Empty(res);
        }
        
        [Fact]
        public async Task Given_context_with_data_When_executing_query_without_filter_Then_it_returns_everything()
        {
            var options = new DbContextOptionsBuilder<FunctionUsageContext>()
                .UseInMemoryDatabase(nameof(Given_context_with_data_When_executing_query_without_filter_Then_it_returns_everything)).Options;

            var context = new FunctionUsageContext(options);
            var query = new FunctionsTotalUsageQuery(context);

            var functions = new[]
            {
                new FunctionTotalUsage {FunctionName = "testFunc", InvocationsCount = 1},
                new FunctionTotalUsage {FunctionName = "myFunc", InvocationsCount = 2}
            };
            context.FunctionsTotalUsage.AddRange(functions);
            await context.SaveChangesAsync();
            
            var res =  await query.Execute("");
            
            Assert.Equal(functions,res);
        }


        [Fact]
        public async Task Given_context_with_data_When_executing_query_with_filter_Then_it_returns_filtered_data()
        {
            var options = new DbContextOptionsBuilder<FunctionUsageContext>()
                .UseInMemoryDatabase(nameof(Given_context_with_data_When_executing_query_with_filter_Then_it_returns_filtered_data)).Options;

            var context = new FunctionUsageContext(options);
            var query = new FunctionsTotalUsageQuery(context);

            var functions = new[]
            {
                new FunctionTotalUsage {FunctionName = "testFunc", InvocationsCount = 1},
                new FunctionTotalUsage {FunctionName = "myFunc", InvocationsCount = 2}
            };
            context.FunctionsTotalUsage.AddRange(functions);
            await context.SaveChangesAsync();
            
            var res =  await query.Execute("my");
            
            Assert.Equal(new []{functions[1]},res);
        }
    }
}