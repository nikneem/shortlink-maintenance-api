using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HexMaster.ShortLink.Core.Configuration;
using HexMaster.ShortLink.Maintenance;

[assembly: FunctionsStartup(typeof(DiStartup))]
namespace HexMaster.ShortLink.Maintenance
{
    public class DiStartup : FunctionsStartup
    {

        public override void Configure(IFunctionsHostBuilder builder)
        {
            //JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            //{
            //    ContractResolver = new CamelCasePropertyNamesContractResolver()
            //};
            var serviceProvider = builder.Services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();


            //var instrumentationKey = configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
            //builder.Services.AddApplicationInsightsTelemetry(x => { x.EnableAdaptiveSampling = false; });
            //builder.Services.AddLogging(loggingBuilder =>
            //{
                
            //    var logger = new LoggerConfiguration()
            //        .WriteTo.ApplicationInsights(instrumentationKey, TelemetryConverter.Traces)
            //        .CreateLogger();
            //    loggingBuilder.AddSerilog(logger);
            //});

            builder.Services.ConfigureCore(configuration);

        }
    }
}


