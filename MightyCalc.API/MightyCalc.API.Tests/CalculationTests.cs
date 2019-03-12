using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using MightyCalc.Client;
using Xunit;
using Xunit.Abstractions;

namespace MightyCalc.API.Tests
{
    public class CalculationTests
    {
        private readonly IMightyCalcClient _client;

        public CalculationTests()
        {
            var builder = new WebHostBuilder()
                .UseEnvironment("Development")
                .UseStartup<Startup>(); 
            
            var server = new TestServer(builder);
            var httpClient = server.CreateClient();

            _client = new MightyCalcClient("", httpClient);
        }
        
        
        
        [Theory]
        [InlineData("1+1.5",2.5D)] //add
        [InlineData("1-1.5",-0.5D)] //substract
        [InlineData("1*1.5",1.5D)] //multiple
        [InlineData("3/1.5",2D)] //divide
        [InlineData("sqrt(2.25)",1.5D)] //square root
        [InlineData("cuberoot(8)",2D)] //cube root
        [InlineData("fact(5)",120D)] //factorial
        public async Task Given_term_expression_with_build_in_functions_When_calculating_Then_answer_is_provided(string expression, double answer)
        {
            Assert.Equal(answer, await _client.CalculateAsync(new Client.Expression(){Representation = expression}));
        }
        
        [Theory]
        [InlineData(2.5D, "a+b","a",1D,"b",1.5D)] //add
        [InlineData(2.5D,"a-b","a",1D,"b",-1.5D)] //substract
        [InlineData(1.5D,"a*b","a",1D,"b",1.5)] //multiple
        public async Task Given_parametrized_expression_with_build_in_functions_When_calculating_Then_answer_is_provided(double answer,string expression, params object[] parameters)
        {
            var parametersList = new List<Client.Parameter>();
            var enumerator = parameters.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var name = (string) enumerator.Current;
                enumerator.MoveNext();
                var value = (double) enumerator.Current;
                parametersList.Add(new Client.Parameter{Name = name, Value = value});
            }

            Assert.Equal(answer, await _client.CalculateAsync(new Client.Expression(){Representation = expression, Parameters = parametersList}));
        }
        
        [Fact]
        public async Task Given_parametrized_expression_And_defining_not_existing_parameters_calculating_When_calculating_Then_not_existing_parameters_are_ignored()
        {
            Assert.Equal(2, await _client.CalculateAsync(new Client.Expression(){Representation = "a+1", Parameters = new []
            {
                new Client.Parameter(){Name ="a", Value=1},
                new Client.Parameter(){Name = "b", Value=2}, 
            }}));
        }
        
        [Fact]
        public async Task Given_parametrized_expression_And_redefining_parameters_calculating_When_calculating_Then_error_is_thrown()
        {
            Assert.Equal(3, await _client.CalculateAsync(new Client.Expression(){Representation = "add(a,1)", Parameters = new []
            {
                new Client.Parameter(){Name = "a", Value=1},
                new Client.Parameter(){Name = "a", Value=2}, 
            }}));
        }


        
        [Fact]
        public async Task Given_expression_with_non_existing_function_When_calculating_Then_error_is_raised_AND_code_is_400()
        {
            var ex = await Assert.ThrowsAsync<MightyCalcException>( () => _client.CalculateAsync(new Client.Expression(){Representation = "test(2,1)"}));
            Assert.Equal(400, ex.StatusCode);
        }
    }
}