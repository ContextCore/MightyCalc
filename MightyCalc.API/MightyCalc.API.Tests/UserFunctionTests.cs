using System;
using System.Linq;
using System.Threading.Tasks;
using MightyCalc.Client;
using Xunit;

namespace MightyCalc.API.Tests
{
    public abstract class UserFunctionTests
    {
        private IMightyCalcClient Client => _lazyClient.Value;
        private readonly Lazy<IMightyCalcClient> _lazyClient;
        protected abstract IMightyCalcClient CreateClient();
    
        protected UserFunctionTests()
        {
            _lazyClient = new Lazy<IMightyCalcClient>(CreateClient);
        }
        

        [Fact]
        public async Task Given_only_builtin_functions_When_getting_function_list_Then_it_contains_all_functions()
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
            
            
            await Client.CreateFunctionAsync(initialFunc);
            await Assert.ThrowsAsync<MightyCalcException>(() => Client.CreateFunctionAsync(initialFunc));
        }
    }
}