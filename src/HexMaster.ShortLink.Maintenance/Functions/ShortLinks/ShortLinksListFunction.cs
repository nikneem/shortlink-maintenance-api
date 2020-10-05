using System;
using System.Net.Http;
using System.Threading.Tasks;
using HexMaster.Functions.JwtBinding;
using HexMaster.Functions.JwtBinding.Model;
using HexMaster.ShortLink.Core.Contracts;
using HexMaster.ShortLink.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace HexMaster.ShortLink.Maintenance.Functions.ShortLinks
{
    public  class ShortLinksListFunction
    {
        private readonly IShortLinksService _service;
        private readonly ILogger<ShortLinkCreateFunction> _logger;

        [FunctionName("ShortLinksListFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "shortlinks")] HttpRequestMessage req,
            [JwtBinding] AuthorizedModel auth
        )
        {
            try
            {
                var list = await _service.ListAsync(auth.Subject, 0, 100);
                return new OkObjectResult(list);
            }
            catch (ShortLinkException shortLinkException)
            {
                _logger.LogWarning(shortLinkException, shortLinkException.Message);
                return new ConflictObjectResult(shortLinkException.Errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred while trying to add a new short link");
                return new BadRequestObjectResult(ex.Message);
            }
        }

        public ShortLinksListFunction(IShortLinksService service,
            ILogger<ShortLinkCreateFunction> logger)
        {
            _service = service;
            _logger = logger;
        }
    }
}
