using System;
using System.Threading.Tasks;
using MightyCalc.Calculations;
using Xunit;

namespace MightyCalc.Node.Tests
{
    public class NamedCalculatorTests
    {
        private readonly INamedCalculatorPool _pool;

        public NamedCalculatorTests()
        {
            _pool = null;
        }
        
        [Fact]
        public async Task Given_empty_pool_When_getting_a_calculator_and_calculate_Then_calculation_succeeds()
        {
            var calc = _pool.For("test");
            Assert.Equal(101D, await calc.Calculate("1+b",new Parameter("b",100)));
        }
        
        [Fact]
        public async Task Given_named_calculator_When_add_new_functions_Then_it_can_be_used()
        {
            var calc = _pool.For("test");
            await calc.AddFunction("newFunc", "new test function", "a+b-c", "a", "b", "c");
            
            calc = _pool.For("test");
            Assert.Equal(94D, await calc.Calculate("1+b+newFunc(1,2,c)",new Parameter("b",100), new Parameter("c",10)));
        }
        
        [Fact]
        public async Task Given_different_named_calculator_When_add_new_functions_Then_they_are_independent()
        {
            var calc = _pool.For("test");
            await calc.AddFunction("newFunc", "new test function", "a+b-c", "a", "b", "c");
            
            calc = _pool.For("test2");
            await calc.AddFunction("newFunc", "new test function", "a+b", "a", "b");

            
            Assert.Equal(-1D, await _pool.For("test").Calculate("newFunc(1,2,4)"));
            Assert.Equal(3D, await _pool.For("test2").Calculate("newFunc(1,2)"));
        }
    }
}