using System;
using System.IO;
using System.Reflection;
using Akka.Actor;
using Akka.Cluster;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using MightyCalc.Calculations;
using MightyCalc.Node;
using MightyCalc.Reports;
using MightyCalc.Reports.DatabaseProjections;
using MightyCalc.Reports.ReportingExtension;
using Swashbuckle.AspNetCore.Swagger;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting.Internal;

namespace MightyCalc.API
{
    
    
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        protected virtual DbContextOptions<FunctionUsageContext> GetDbOptions(MightyCalcApiConfiguration cfg)
        {
            return new DbContextOptionsBuilder<FunctionUsageContext>()
                .UseNpgsql(cfg.ReadModel).Options;
        }

        protected virtual void ConfigureExtensions(ActorSystem system, MightyCalcApiConfiguration cfg)
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(GetDbOptions(cfg));
            builder.RegisterType<FunctionUsageContext>();
            builder.RegisterType<ReportingDependencies>().As<IReportingDependencies>().SingleInstance();
            var container = builder.Build();
            system.InitReportingExtension(container);
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new Info {Title = "Mighty Calc API", Version = "v1"}); })
                .AddMvc()
                .AddNewtonsoftJson();

            var settings = new MightyCalcApiConfiguration();
            Configuration.GetSection("ApiSettings").Bind(settings);

            var system = CreateActorSystem(settings);
            var cluster = Cluster.Get(system);
            cluster.Join(system.Provider.DefaultAddress);

            var options = GetDbOptions(settings);

            services.AddSingleton<ActorSystem>(system);
            services.AddTransient<FunctionUsageContext>();
            services.AddTransient<IApiController, AkkaApi>();
            services.AddSingleton(options);
            services.AddTransient<IFunctionsTotalUsageQuery, FunctionsTotalUsageQuery>();
            services.AddSingleton<INamedCalculatorPool, AkkaCalculatorPool>();

            ConfigureExtensions(system, settings);
        }

        protected virtual ExtendedActorSystem CreateActorSystem(MightyCalcApiConfiguration cfg)
        {
            return (ExtendedActorSystem)ActorSystem.Create("Calc", cfg.Akka);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //map /swagger folder in the same folder as executing assembly
            var location = Assembly.GetExecutingAssembly().Location;
            var executingAssemblyFolder = Path.GetDirectoryName(location);
            var provider = new PhysicalFileProvider(Path.Combine(executingAssemblyFolder,"swagger"));
            
            app.UseDirectoryBrowser(new DirectoryBrowserOptions
            {
                FileProvider = provider,
                RequestPath = "/swagger"
            });

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = provider,
                RequestPath = "/swagger",
                ServeUnknownFileTypes = true });
            
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/MightyCalcAPI.yaml", "MightyCalc API V1");
                c.RoutePrefix = string.Empty;
            });
            app.UseHttpsRedirection();

            app.UseRouting(routes => { routes.MapApplication(); });

            app.UseAuthentication();
            app.UseAuthorization();
        }
    }
}