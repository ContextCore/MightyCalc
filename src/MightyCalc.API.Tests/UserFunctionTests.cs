using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using MightyCalc.Client;
using Serilog;
using Serilog.Events;
using Xunit;

namespace MightyCalc.API.Tests
{


    public class UserFunctionTests
    {
        private IMightyCalcClient Client { get; }
    
        public UserFunctionTests()
        {
            Client = CreateClient();
        }
        
        protected IMightyCalcClient CreateClient()
        {
            
            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File($"api_{DateTime.Now:yyyy-MMM-dd-HH-mm-ss}.log")
                .CreateLogger();
            
            var builder = new WebHostBuilder()
                .UseEnvironment("Development")
                .UseStartup<LocalStartup>()
                .UseSerilog(logger);

            var server = new TestServer(builder);
            return new MightyCalcClient("",server.CreateClient());
        }



 
        [Fact]
        public async Task Given_created_function_When_calculating_expression_with_it_Then_function_is_calculated()
        {
            var namedExpression = new Client.NamedExpression()
            {
                Expression = new Client.Expression
                {
                    Representation = "cuberoot(a)*b", 
                    Parameters =
                        new[] {new Client.Parameter {Name = "a"}, new Client.Parameter {Name = "b"}}
                },
                Name = "testFunction",
                Description = "cuberoot description"
            };
            
            await Client.CreateFunctionAsync(namedExpression);
            var result = await Client.CalculateAsync(new Client.Expression()
            {
                Representation = "testFunction(a,b) + 1", Parameters = new[]
                {
                    new Client.Parameter() {Name = "a", Value = 8},
                    new Client.Parameter() {Name = "b", Value = 2},
                }
            });
            
            Assert.Equal(5,result);
        }
        
        [Fact]
        public async Task Given_created_function_When_replacing_it_Then_new_function_is_taken_for_calculations()
        {
            var initialFunc = new Client.NamedExpression()
            {
                Expression = new Client.Expression
                {
                    Representation = "cuberoot(a)*b", 
                    Parameters =
                        new[] {new Client.Parameter {Name = "a"}, new Client.Parameter {Name = "b"}}
                },
                Name = "testFunction",
                Description = "cuberoot description"
            };
            
            var replacedFunc = new Client.NamedExpression()
            {
                Expression = new Client.Expression
                {
                    Representation = "cuberoot(a)*b + 1", 
                    Parameters =
                        new[] {new Client.Parameter {Name = "a"}, new Client.Parameter {Name = "b"}}
                },
                Name = "testFunction",
                Description = "cuberoot description"
            };
            
            
            await Client.CreateFunctionAsync(initialFunc);
            await Client.ReplaceFunctionAsync(replacedFunc);
            
            var result = await Client.CalculateAsync(new Client.Expression()
            {
                Representation = "testFunction(a,b) + 1", Parameters = new[]
                {
                    new Client.Parameter() {Name = "a", Value = 8},
                    new Client.Parameter() {Name = "b", Value = 2},
                }
            });
            
            Assert.Equal(6,result);
        }
        
       
    }
}