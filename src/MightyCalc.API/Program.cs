using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MightyCalc.Reports.DatabaseProjections;
using MightyCalc.Reports.ReportingExtension;
using Swashbuckle.AspNetCore.Swagger;

namespace MightyCalc.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
                
            using (var serviceScope = host.Services.CreateScope())
            {
                using(var context = serviceScope.ServiceProvider.GetRequiredService<FunctionUsageContext>() )
                {
                    context.Database.Migrate();
                }

                var system = (ExtendedActorSystem)serviceScope.ServiceProvider.GetRequiredService<ActorSystem>();
                
                var cluster = Cluster.Get(system);
                cluster.Join(system.Provider.DefaultAddress);
                system.GetReportingExtension().Start();
            }
            
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}