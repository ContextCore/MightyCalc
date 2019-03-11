using System;
using Xunit;
using Xunit.Abstractions;

namespace MightyCalc.API.Tests
{
    public class MightyCalcApiTests
    {
        private readonly ITestOutputHelper _output;

        public MightyCalcApiTests(ITestOutputHelper output)
        {
            _output = output;
        }
        
        
        
        [Theory]
        [InlineData("1+1.5",2.5D)] //add
        [InlineData("1-1.5",-0.5D)] //substract
        [InlineData("1*1.5",1.5D)] //multiple
        [InlineData("3/1.5",2D)] //divide
        [InlineData("sqrt(2.25)",1.5D)] //square root
        [InlineData("cuberoot(3.375)",1.5D)] //cube root
        [InlineData("5!",120D)] //factorial
        public void Given_expression_with_build_in_functions_When_calculating_Then_answer_is_provided(string expression, double answer)
        {
            throw new NotImplementedException();
        }
        
        
        [Fact]
        public void Given_expression_with_non_existing_function_When_calculating_Then_error_is_raised()
        {
            throw new NotImplementedException();
        }
        
        [Fact]
        public void Given_only_builtin_functions_When_getting_function_list_Then_it_contains_all_functions()
        {
            throw new NotImplementedException();
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