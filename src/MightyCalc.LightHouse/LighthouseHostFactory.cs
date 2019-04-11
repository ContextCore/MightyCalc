using Akka.Actor;
using MightyCalc.Configuration;

namespace MightyCalc.LightHouse
{
    public static class LighthouseHostFactory
    {
        public static ActorSystem LaunchLighthouse()
        {
            var config = new LighthouseConfig();
            return ActorSystem.Create(config.ClusterName, config.Akka).StartPbm();
        }
    }
}