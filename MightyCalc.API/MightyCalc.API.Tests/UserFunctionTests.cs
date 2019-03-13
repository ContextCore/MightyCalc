using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using MightyCalc.Client;
using Xunit;

namespace MightyCalc.API.Tests
{
    public class UserFunctionTests
    {
        private readonly IMightyCalcClient _client;

        public UserFunctionTests()
        {
            var builder = new WebHostBuilder()
                .UseEnvironment("Development")
                .UseStartup<Startup>();

            var server = new TestServer(builder);
            var httpClient = server.CreateClient();

            _client = new MightyCalcClient("", httpClient);
        }


        [Fact]
        public async Task Given_only_builtin_functions_When_getting_function_list_Then_it_contains_all_functions()
        {
            var functions = await _client.FindFunctionsAsync();
            var expectedNames =
                new[]
                {
                    "Addition",
                    "Substraction",
                    "Multiply",
                    "Divide",
                    "Square Root",
                    "Cube Root",
                    "Factorial"
                };

            Assert.Equal(expectedNames, functions.Select(ne => ne.Description));
        }


        [Fact]
        public async Task When_create_new_function_Then_it_shows_in_list()
        {
            var namedExpression = new Client.NamedExpression()
            {
                Expression = new Client.Expression
                {
                    Representation = "cuberoot(a)*b", 
                    Parameters =
                        new[] {new Client.Parameter {Name = "a"}, new Client.Parameter {Name = "b"}}
                },
                Name = "test function",
                Description = "cuberoot description"
            };
            
            await _client.CreateFunctionAsync(namedExpression);

            var functionNames = await _client.FindFunctionsAsync();
            Assert.Contains(namedExpression.Name, functionNames.Select(f => f.Name));
            Assert.Contains(namedExpression.Expression.Representation, functionNames.Select(f => f.Expression.Representation));
            Assert.Contains(namedExpression.Description, functionNames.Select(f => f.Description));
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
            
            await _client.CreateFunctionAsync(namedExpression);
            var result = await _client.CalculateAsync(new Client.Expression()
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
            
            
            await _client.CreateFunctionAsync(initialFunc);
            await _client.ReplaceFunctionAsync(replacedFunc);
            
            var result = await _client.CalculateAsync(new Client.Expression()
            {
                Representation = "testFunction(a,b) + 1", Parameters = new[]
                {
                    new Client.Parameter() {Name = "a", Value = 8},
                    new Client.Parameter() {Name = "b", Value = 2},
                }
            });
            
            Assert.Equal(6,result);
        }
        
        [Fact]
        public async Task Given_created_function_When_creating_it_again_Then_error_occures()
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
            
            
            await _client.CreateFunctionAsync(initialFunc);
            await Assert.ThrowsAsync<MightyCalcException>(() => _client.CreateFunctionAsync(initialFunc));
        }
    }
}