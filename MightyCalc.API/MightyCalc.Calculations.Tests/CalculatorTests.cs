using System;
using System.Collections.Generic;
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
        [InlineData("1+1.5", 2.5D)] //add
        [InlineData("1-1.5", -0.5D)] //substract
        [InlineData("1*1.5", 1.5D)] //multiple
        [InlineData("3/1.5", 2D)] //divide
        [InlineData("sqrt(2.25)", 1.5D)] //square root
        [InlineData("cuberoot(8)", 2D)] //cube root
        [InlineData("fact(5)", 120D)] //factorial
        public async Task Given_term_expression_with_build_in_functions_When_calculating_Then_answer_is_provided(
            string expression, double answer)
        {
            Assert.Equal(answer, _calculator.Calculate(expression));
        }

        [Theory]
        [InlineData(2.5D, "a+b", "a", 1D, "b", 1.5D)] //add
        [InlineData(2.5D, "a-b", "a", 1D, "b", -1.5D)] //substract
        [InlineData(1.5D, "a*b", "a", 1D, "b", 1.5)] //multiple
        [InlineData(2D,"a/b","a",3D,"b",1.5D)] //divide
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
    }
}