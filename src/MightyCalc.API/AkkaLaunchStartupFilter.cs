using System;
using Akka.Actor;
using Akka.Cluster;
using DotNetty.Common.Concurrency;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MightyCalc.Reports.ReportingExtension;

namespace MightyCalc.API
{
    public class AkkaLaunchStartupFilter: IStartupFilter
    {
        private readonly IServiceProvider _serviceProvider;
        public AkkaLaunchStartupFilter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            using(var scope = _serviceProvider.CreateScope())
            {
                var system = (ExtendedActorSystem) scope.ServiceProvider.GetRequiredService<ActorSystem>();

                var cluster = Cluster.Get(system);
                var source = new TaskCompletionSource();
                cluster.RegisterOnMemberUp(() => source.Complete());
                source.Task.Wait();
            }

            return next;
        }
    }
}