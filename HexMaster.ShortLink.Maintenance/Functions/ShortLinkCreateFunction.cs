using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HexMaster.ShortLink.Core.Models.ShortLinks;
using HexMaster.ShortLink.Core.Validators;
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
            var validator = new ShortLinkCreateValidator();
            var validationResult = await validator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                log.LogWarning("Could not create ShortLink due to validation errors");
                foreach (var err in validationResult.Errors)
                {
                    log.LogInformation($"Validation result in ShortLink Create model: '{err.ErrorMessage}'");
                }
                return new BadRequestObjectResult(validationResult.Errors);
            }
        }
    }
}
