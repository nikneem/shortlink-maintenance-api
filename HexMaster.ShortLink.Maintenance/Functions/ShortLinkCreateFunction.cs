using System;
using System.Net.Http;
using System.Threading.Tasks;
using HexMaster.ShortLink.Core.Contracts;
using HexMaster.ShortLink.Core.Exceptions;
using HexMaster.ShortLink.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace HexMaster.ShortLink.Maintenance.Functions
{
    public  class ShortLinkCreateFunction
    {
        private readonly IShortLinksService _service;

        [FunctionName("ShortLinkCreateFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "shortlinks")] HttpRequestMessage req,
            ILogger log)
        {
            try
            {
                var model = await req.Content.ReadAsAsync<ShortLinkCreateDto>();
                var createdModel = await _service.CreateAsync(model);
                return new CreatedResult("https://app.4dn.me", createdModel);
            }
            catch (ModelValidationException validationEx)
            {
                log.LogWarning(validationEx, "Validation error occurred while trying to add a new short link");
                return new BadRequestObjectResult(validationEx.Errors);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Unhandled exception occurred while trying to add a new short link");
                return new BadRequestObjectResult(ex.Message);
            }
        }

        public ShortLinkCreateFunction(IShortLinksService service)
        {
            _service = service;
        }

    }
}
