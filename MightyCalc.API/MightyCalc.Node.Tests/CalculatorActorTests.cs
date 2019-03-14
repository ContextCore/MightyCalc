using Akka.Actor;
using Akka.TestKit.Xunit2;
using MightyCalc.Calculations;
using Xunit;
using Xunit.Abstractions;

namespace MightyCalc.Node.Tests
{
    public class CalculatorActorTests:TestKit
    {
        public CalculatorActorTests(ITestOutputHelper output):base("",output)
        {
            
        }
        [Fact]
        public void Given_calculator_actor_When_calculate_Then_result_is_produced()
        {
            var actor = Sys.ActorOf(Props.Create<CalculatorActor>());
            actor.Tell(new CalculatorActorProtocol.CalculateExpression("1+a",new Parameter("a",2)));
            var msg = ExpectMsg<CalculatorActorProtocol.CalculationResult>();
            Assert.Equal(3D,msg.Value);
        }
        
        [Fact]
        public void Given_calculator_actor_When_add_function_Then_it_can_be_used()
        {
            var actor = Sys.ActorOf(Props.Create<CalculatorActor>());
            actor.Tell(new CalculatorActorProtocol.AddFunction(new FunctionDefinition("myFunc",2,"test description",
                "a+b-Pow(a,2)","a","b")));
            
            actor.Tell(new CalculatorActorProtocol.CalculateExpression("1+myFunc(2,3)"));
            var msg = ExpectMsg<CalculatorActorProtocol.CalculationResult>();
            Assert.Equal(2D,msg.Value);
        }
        
        [Fact]
        public void Given_calculator_actor_When_add_bad_function_Then_error_is_returned()
        {
            var actor = Sys.ActorOf(Props.Create<CalculatorActor>());
            actor.Tell(new CalculatorActorProtocol.AddFunction(new FunctionDefinition("myFunc",2,"test description",
                "a+b-Pow(a,2)")));
            
            var msg = ExpectMsg<CalculatorActorProtocol.FunctionAddError>();
            Assert.NotNull(msg.Reason);
        }
        
        
        [Fact]
        public void Given_calculator_actor_When_calculate_bad_epression_Then_error_is_returned()
        {
            var actor = Sys.ActorOf(Props.Create<CalculatorActor>());
            
            actor.Tell(new CalculatorActorProtocol.CalculateExpression("1+myFunc(2,3)"));
            var msg = ExpectMsg<CalculatorActorProtocol.CalculationError>();
            Assert.NotNull(msg.Reason);
        }
    }
}