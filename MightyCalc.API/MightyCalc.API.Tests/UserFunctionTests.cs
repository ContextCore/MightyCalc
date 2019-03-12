using System;
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
        public void Given_only_builtin_functions_When_getting_function_list_Then_it_contains_all_functions()
        {
            //  var functions = await _client.CalculateAsync(new Client.Expression(){Representation = "test(2,1)"}));
            //  Assert.Equal(400, ex.StatusCode);
        }
        

        [Fact]
        public void When_create_new_function_Then_it_shows_in_list()
        {
            throw new NotImplementedException();
        }
        
        [Fact]
        public void Given_created_function_When_create_new_function_with_same_name_Then_error_is_raised()
        {
            throw new NotImplementedException();
        }
        
        [Fact]
        public void Given_created_function_When_calculating_expression_with_it_Then_function_is_calculated()
        {
            throw new NotImplementedException();
        }

        
        [Fact]
        public void When_delete_not_existing_function_Then_receive_an_error()
        {
            throw new NotImplementedException();
        }
        
        [Fact]
        public void When_delete_existing_function_Then_it_cannot_be_called()
        {
            throw new NotImplementedException();
        }
        
        [Fact]
        public void When_delete_existing_function_Then_it_is_not_shown_in_list()
        {
            throw new NotImplementedException();
        }
        
        [Fact]
        public void When_replace_existing_function_Then_it_is_used_instead_of_old_one()
        {
            throw new NotImplementedException();
        }
    }
}