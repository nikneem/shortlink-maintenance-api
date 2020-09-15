using HexMaster.Functions.Auth;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(JwtBindingStartup))]
namespace HexMaster.Functions.Auth
{
        public class JwtBindingStartup : IWebJobsStartup
        {
            public void Configure(IWebJobsBuilder builder)
            {
                builder.AddJwtBindingExtension();
            }
        }
}
