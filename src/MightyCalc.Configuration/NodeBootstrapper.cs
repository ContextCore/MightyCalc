using Akka.Actor;
using Petabridge.Cmd.Cluster;
using Petabridge.Cmd.Host;

namespace MightyCalc.Configuration
{
    /// <summary>
    /// Used to help inject and standardize all of the different components
    /// needed to run all of the mightycalc services in production.
    /// </summary>
    public static class NodeBootstrapper
    {
        /// <summary>
        /// Start Petabridge.Cmd 
        /// </summary>
        /// <param name="system">The <see cref="ActorSystem"/> that will run Petabridge.Cmd</param>
        /// <returns>The same <see cref="ActorSystem"/></returns>
        public static ActorSystem StartPbm(this ActorSystem system)
        {
            var pbm = PetabridgeCmd.Get(system);
            pbm.RegisterCommandPalette(ClusterCommands.Instance); // enable cluster management commands
            pbm.Start();
            return system;
        }
    }

    public class ManualRequestReason : CoordinatedShutdown.Reason
    {
        
    }
}