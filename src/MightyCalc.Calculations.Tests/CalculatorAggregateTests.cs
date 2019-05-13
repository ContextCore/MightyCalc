using System;
using System.Linq;
using System.Threading.Tasks;
using GridDomain.Scenarios;
using GridDomain.Scenarios.Runners;
using MightyCalc.Calculations.Aggregate;
using MightyCalc.Calculations.Aggregate.Commands;
using MightyCalc.Calculations.Aggregate.Events;
using Serilog;
using Serilog.Extensions.Logging;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace MightyCalc.Calculations.Tests
{
    public class CalculatorAggregateTests
    {
        private readonly SerilogLoggerProvider _log;

        public CalculatorAggregateTests(ITestOutputHelper output)
        {
            var cfg = new LoggerConfiguration().WriteTo.XunitTestOutput(output).CreateLogger();
            _log = new SerilogLoggerProvider(cfg);
        }

        [Theory]
        [InlineData("1+1.5", 2.5D, "AddChecked")] //add basic
        [InlineData("add(1,1.5)", 2.5D, "add")] //add custom
        [InlineData("1-1.5", -0.5D, "SubtractChecked")] //substract basic 
        [InlineData("sub(1,1.5)", -0.5D, "sub")] //substract custom 
        [InlineData("1*1.5", 1.5, "MultiplyChecked")] //multiple basic 
        [InlineData("3/1.5", 2D, "Divide")] //divide basic 
        [InlineData("sqrt(2.25)", 1.5D, "sqrt")] //square root
        [InlineData("cuberoot(8)", 2D, "cuberoot")] //cube root
        [InlineData("fact(5)", 120D, "fact")] //factorial
        public async Task Given_term_expression_with_build_in_functions_When_calculating_Then_answer_is_provided(
            string expression, double answer, params string[] functionUsed)
        {
            await AggregateScenario.New<Calculator>()
                .When(new CalculateExpression("myCalc", expression, new Parameter[] { }))
                .Then(new CalculationPerformed("myCalc", 0,expression, new Parameter[] { }, answer, functionUsed))
                .Run
                .Local(_log)
                .Check();
        }


        [Fact]
        public void When_add_bad_formed_custom_function_Then_exception_is_raised()
        {
            AggregateScenario.New<Calculator>()
                .When(new AddFunction("myCalc",
                    new FunctionDefinition("test",
                        2,
                        "Test function",
                        "sub(add(a,b),c)",
                        new string[] { })))
                .Run
                .Local(_log)
                .ShouldThrow<Exception>();
        }

     
        [Fact]
        public async Task When_add_custom_function_Then_can_use_it()
        {
            var functionDefinition = new FunctionDefinition("test",
                3,
                "Test function",
                "sub(add(a,b),c)",
                "a", "b", "c");
            await AggregateScenario.New<Calculator>()
                .Given(new CalculatorCreated("myCalc",0))
                .When(new AddFunction("myCalc", functionDefinition))
                .Then(new FunctionAdded("myCalc", functionDefinition,1))
                .Run
                .Local(_log)
                .Check();
        }
        
        
        [Fact]
        public async Task When_add_custom_function_twice_Then_the_last_version_is_used()
        {
            var functionDefinition = new FunctionDefinition("test",
                "Test function",
                "sub(add(a,b),c)",
                "a", "b", "c");
            
            var oldFunctionDefinition = new FunctionDefinition("test",
                "Test function",
                "a+b+c",
                "a", "b", "c");
            
            await AggregateScenario.New<Calculator>()
                .Given(new CalculatorCreated("myCalc",0))
                .When(new AddFunction("myCalc", oldFunctionDefinition),
                      new AddFunction("myCalc", functionDefinition))
                .Then(new FunctionAdded("myCalc", oldFunctionDefinition, 1),
                      new FunctionReplaced("myCalc", functionDefinition, 2))
                .Run
                .Local(_log)
                .Check();
        }
    }
}