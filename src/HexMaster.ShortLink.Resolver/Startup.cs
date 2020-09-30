using System;
using HexMaster.ShortLink.Core.Configuration;
using HexMaster.ShortLink.Resolver;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

[assembly: FunctionsStartup(typeof(Startup))]
namespace HexMaster.ShortLink.Resolver
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();
            var hostingEnvironment = serviceProvider.GetService<IHostingEnvironment>();


            var instrumentationKey = configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
            builder.Services.AddApplicationInsightsTelemetry(x => { x.EnableAdaptiveSampling = false; });
            builder.Services.AddLogging(loggingBuilder =>
            {
                var loggerBuilder = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .MinimumLevel.Verbose()
                    .MinimumLevel.Override("HexMaster", LogEventLevel.Verbose)
                    .MinimumLevel.Override("Function", LogEventLevel.Verbose)
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("Host", LogEventLevel.Error)
                    .MinimumLevel.Override("System", LogEventLevel.Error)
                    .WriteTo.ApplicationInsights(instrumentationKey, TelemetryConverter.Traces);

                if (hostingEnvironment != null && hostingEnvironment.IsDevelopment())
                {
                    loggerBuilder=loggerBuilder.WriteTo.ColoredConsole();
                }

                var logger = loggerBuilder.CreateLogger();
                loggingBuilder.AddSerilog(logger);
            });

            builder.Services.ConfigureCore(configuration);
        }
    }
}


