using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MightyCalc.Reports.DatabaseProjections;
using Xunit;

namespace MightyCalc.Reports.Tests
{
    public class ProjectionQueryTests
    {
        [Fact]
        public async Task Given_context_with_data_When_executing_projection_query_Then_it_should_return_data()
        {
            var options = new DbContextOptionsBuilder<FunctionUsageContext>()
                .UseInMemoryDatabase(nameof(Given_context_with_data_When_executing_projection_query_Then_it_should_return_data)).Options;

            var context = new FunctionUsageContext(options);
            var query = new FindProjectionQuery(context);
            
            var projection = new Projection
                {Event = "testEvent", Name = "testName", Projector = "projector", Sequence = 10};
            context.Projections.Add(projection);
            await context.SaveChangesAsync();

            var res =  query.Execute(projection.Name, projection.Projector, projection.Event);
           Assert.Equal(projection,res);
        }

        [Fact]
        public void Given_empty_context_When_executing_projection_query_Then_it_should_return_Null()
        {
            var options = new DbContextOptionsBuilder<FunctionUsageContext>()
                .UseInMemoryDatabase(nameof(Given_empty_context_When_executing_projection_query_Then_it_should_return_Null)).Options;

            var context = new FunctionUsageContext(options);
            var query = new FindProjectionQuery(context);
            
            var result = query.Execute("testName", "projector", "testEvent");
            Assert.Null(result);
        }
    }
}