using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using MightyCalc.Client;
using MightyCalc.IntegrationTests.Tools;
using Xunit;

namespace MightyCalc.API.IntegrationTests
{
    
    
    //inmemory journal do not support read queries 8(

    public class UserFunctionTests
    {
        private IMightyCalcClient Client { get; }
        protected IMightyCalcClient CreateClient()
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
        public UserFunctionTests()
        {
            Client = CreateClient();
        }
        

        [Fact]
        public async Task When_getting_function_list_Then_it_contains_all_basic_functions()
        {
            var functions = await Client.FindFunctionsAsync();
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

            functions.Select(ne => ne.Description).Should().Contain(expectedNames);
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
                Name = "test-function-"+Guid.NewGuid().ToString("N"),
                Description = "cuberoot description"
            };
            
            await Client.CreateFunctionAsync(namedExpression);

            var functionNames = await Client.FindFunctionsAsync();
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
                Name = "testFunction"+Guid.NewGuid().ToString("N"),
                Description = "cuberoot description"
            };
            
            await Client.CreateFunctionAsync(namedExpression);
            var result = await Client.CalculateAsync(new Client.Expression()
            {
                Representation = $"{namedExpression.Name}(a,b) + 1", Parameters = new[]
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
                Name = "testFunction"+Guid.NewGuid().ToString("N"),
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
                Name = "testFunction"+Guid.NewGuid().ToString("N"),
                Description = "cuberoot description"
            };
            
            await Client.CreateFunctionAsync(initialFunc);
            await Assert.ThrowsAsync<MightyCalcException>(() => Client.CreateFunctionAsync(initialFunc));
        }
    }
}