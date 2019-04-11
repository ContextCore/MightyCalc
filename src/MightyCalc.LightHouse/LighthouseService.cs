using System.Threading.Tasks;
using Akka.Actor;
using MightyCalc.Configuration;

namespace MightyCalc.LightHouse
{
    public class LighthouseService
    {
        private ActorSystem _lighthouseSystem;

        public void Start()
        {
            _lighthouseSystem = LighthouseHostFactory.LaunchLighthouse();
        }

        /// <summary>
        /// Task completes once the Lighthouse <see cref="ActorSystem"/> has terminated.
        /// </summary>
        /// <remarks>
        /// Doesn't actually invoke termination. Need to call <see cref="StopAsync"/> for that.
        /// </remarks>
        public Task TerminationHandle => _lighthouseSystem.WhenTerminated;

        public async Task StopAsync()
        {
            await CoordinatedShutdown.Get(_lighthouseSystem).Run(new ManualRequestReason());
        }
    }
}