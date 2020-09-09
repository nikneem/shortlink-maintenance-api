using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HexMaster.ShortLink.Core.Models.ShortLinks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace HexMaster.ShortLink.Maintenance.Functions
{
    public static class ShortLinkCreateFunction
    {

        public const string UrlRegularExpression = "(?<Protocol>\\w+):\\/\\/(?<Domain>[\\w@][\\w.:@]+)\\/?[\\w\\.?=%&=\\-@/$,]*";

        [FunctionName("ShortLinkCreateFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            var model = await req.Content.ReadAsAsync<ShortLinkCreateDto>();
            if (Regex.IsMatch(model.EndpointUrl, UrlRegularExpression))
            {
                // Do stuff
            }
            return new BadRequestResult();
        }
    }
}
