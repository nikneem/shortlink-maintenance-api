using HexMaster.ShortLink.Core.Configuration;
using HexMaster.ShortLink.Resolver;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;

[assembly: FunctionsStartup(typeof(Startup))]
namespace HexMaster.ShortLink.Resolver
{
    public class Startup : FunctionsStartup
    {

        public override void Configure(IFunctionsHostBuilder builder)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            var serviceProvider = builder.Services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();

            var instrumentationKey = configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
            builder.Services.AddApplicationInsightsTelemetry(x => { x.EnableAdaptiveSampling = false; });
            builder.Services.AddLogging(loggingBuilder =>
            {
                
                var logger = new LoggerConfiguration()
                    .WriteTo.ApplicationInsights(instrumentationKey, TelemetryConverter.Traces)
                    .CreateLogger();
                loggingBuilder.AddSerilog(logger);
            });
        }
    }
}