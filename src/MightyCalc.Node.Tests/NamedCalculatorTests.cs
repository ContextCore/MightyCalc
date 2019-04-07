using System;
using System.Security;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster;
using Akka.Configuration;
using Akka.TestKit.Xunit2;
using MightyCalc.Calculations;
using Xunit;
using Xunit.Abstractions;

namespace MightyCalc.Node.Tests
{
    public class NamedCalculatorTests:TestKit
    {
        private readonly INamedCalculatorPool _pool;

        private static readonly Config Config = @"
akka {
      actor{
            provider = ""Akka.Cluster.ClusterActorRefProvider, Akka.Cluster"" 
            serialize-messages = on 
            serialize-creators = on
       
			serializers : {
                hyperion = ""Akka.Serialization.HyperionSerializer, Akka.Serialization.Hyperion""
            }
            serialization-bindings : {
                ""System.Object"" = hyperion
            }
       }    
 
      cluster{
			roles = [calculation]
      }
}    
";

        protected NamedCalculatorTests(ITestOutputHelper output, Config config) : base(config,"Test",output)
        {
           // var clusterType = typeof(Akka.Cluster.ClusterActorRefProvider);
            var cluster = Cluster.Get(Sys);
            cluster.Join((Sys as ExtendedActorSystem).Provider.DefaultAddress);
            _pool = new AkkaCalculatorPool(Sys, TimeSpan.FromSeconds(10));
        }

        public NamedCalculatorTests(ITestOutputHelper output) : this(
            output, Config)
        {
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