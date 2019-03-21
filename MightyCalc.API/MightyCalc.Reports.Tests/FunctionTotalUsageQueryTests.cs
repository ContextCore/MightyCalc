using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MightyCalc.Reports.DatabaseProjections;
using Xunit;

namespace MightyCalc.Reports.Tests
{
    public class FunctionTotalUsageQueryTests{

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
            context.TotalFunctionUsage.AddRange(functions);
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
            context.TotalFunctionUsage.AddRange(functions);
            await context.SaveChangesAsync();
            
            var res =  await query.Execute("my");
            
            Assert.Equal(new []{functions[1]},res);
        }
    }
}