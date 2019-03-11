using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sprache;
using Xunit;

namespace MightyCalc.Calculations.Tests
{
    public class CalculatorTests
    {
        private readonly SpracheCalculator _calculator;

        public CalculatorTests()
        {
            _calculator = new SpracheCalculator();
        }

        [Theory]
        [InlineData("1+1.5", 2.5D)] //add basic
        [InlineData("add(1,1.5)", 2.5D)] //add custom
        [InlineData("1-1.5", -0.5D)] //substract basic 
        [InlineData("sub(1,1.5)", -0.5D)] //substract custom 
        [InlineData("1*1.5", 1.5D)] //multiple basic 
        [InlineData("3/1.5", 2D)] //divide basic 
        [InlineData("sqrt(2.25)", 1.5D)] //square root
        [InlineData("cuberoot(8)", 2D)] //cube root
        [InlineData("fact(5)", 120D)] //factorial
        public async Task Given_term_expression_with_build_in_functions_When_calculating_Then_answer_is_provided(
            string expression, double answer)
        {
            Assert.Equal(answer, _calculator.Calculate(expression));
        }

        [Theory]
        [InlineData(2.5D, "a+b", "a", 1D, "b", 1.5D)] //add basic
        [InlineData(2.5D, "a-b", "a", 1D, "b", -1.5D)] //substract basic
        [InlineData(1.5D, "a*b", "a", 1D, "b", 1.5)] //multiple basic 
        [InlineData(1.5D, "mul(a,b)", "a", 1D, "b", 1.5)] //multiply custom 
        [InlineData(2D,"a/b","a",3D,"b",1.5D)] //divide basic 
        [InlineData(2D,"div(a,b)","a",3D,"b",1.5D)] //divide custom 
        [InlineData(1.5D,"sqrt(a)","a",2.25)] //square root
        [InlineData(1.5D,"cuberoot(parameter)","parameter",3.375)] //cube root with long-named parameter
        [InlineData(120,"fact(cuberoot)","cuberoot",5D)] //factorial with parameter name conflicting with another function
        [InlineData(1,"a","a",0.5,"a",1D)] //redefining_parameters_calculating_When_calculating_Then_latest_value_is_used
        public void
            Given_parametrized_expression_with_build_in_functions_When_calculating_Then_answer_is_provided(
                double answer, string expression, params object[] parameters)
        {
            var parametersList = new List<Parameter>();
            var enumerator = parameters.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var name = (string) enumerator.Current;
                enumerator.MoveNext();
                var value = (double) enumerator.Current;
                parametersList.Add(new Parameter(name, value));
            }

            Assert.Equal(answer, _calculator.Calculate(expression, parametersList.ToArray()));
        }

        [Fact]
        public void Given_expression_with_non_existing_function_When_calculating_Then_error_is_raised()
        {
            Assert.Throws<ParseException>(() => _calculator.Calculate("notExistingFunc(a)", new[] {new Parameter("a", 1)}));
        }
        
        
        [Fact]
        public void Given_basic_calculator_When_fetch_functions_Then_all_basic_are_presented_with_descriptions()
        {
           Assert.Equal(
               new []
               {
                   "Addition",
                   "Substraction",
                   "Multiply",
                   "Divide",
                   "Square Root",
                   "Cube Root",
                   "Factorial"
               }, _calculator.GetKnownFunctions().Select(s => s.Description).ToArray());
        }
        
        [Fact]
        public void Given_basic_calculator_When_fetch_functions_Then_all_basic_are_presented_with_names()
        {
            Assert.Equal(
                new []
                {
                    "add",
                    "sub",
                    "mul",
                    "div",
                    "sqrt",
                    "cuberoot",
                    "fact"
                }, _calculator.GetKnownFunctions().Select(s => s.Name).ToArray());
        }
        [Fact]
        public void Given_basic_calculator_When_fetch_functions_Then_all_basic_are_presented_with_arity()
        {
            Assert.Equal(
                new []
                {
                    2,
                    2,
                    2,
                    2,
                    1,
                    1,
                    1
                }, _calculator.GetKnownFunctions().Select(s => s.Arity).ToArray());
        }
        
        
        [Fact]
        public void When_add_custom_function_Then_can_fetch_it()
        {
             _calculator.AddFunction("test","Test function","sub(add(a,b),c)","a","b","c");
             var funcSignature = _calculator.GetKnownFunctions().Where(s => s.Name == "test").First();
             Assert.Equal("Test function", funcSignature.Description);
             Assert.Equal(3, funcSignature.Arity);
        }
        
        [Fact]
        public void When_add_custom_function_Then_can_use_it()
        {
            _calculator.AddFunction("test","Test function","sub(add(a,b),c)","a","b","c");
            Assert.Equal(-7, _calculator.Calculate("test(1 ,2 ,10)"));
        }
    }
}