using System;
using System.IO;
using System.Threading.Tasks;
using HexMaster.Functions.JwtBinding;
using HexMaster.Functions.JwtBinding.Model;
using HexMaster.ShortLink.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HexMaster.ShortLink.Maintenance.Functions.ShortLinks
{
    public  class ShortLinkDeleteFunction
    {
        private readonly IShortLinksService _service;
        private readonly ILogger<ShortLinkCreateFunction> _logger;

        [FunctionName("ShortLinkDeleteFunction")]
        public  async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "shortlink/{id:guid}")] HttpRequest req,
            Guid id,
            [JwtBinding] AuthorizedModel auth)
        {
            _logger.LogInformation("Incoming request for deletion");
            try
            {
                await _service.DeleteAsync(auth.Subject, id);
                return new OkResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove shortlink from system");
            }
            return new BadRequestResult();
        }

        public ShortLinkDeleteFunction(IShortLinksService service,
            ILogger<ShortLinkCreateFunction> logger)
        {
            _service = service;
            _logger = logger;
        }

    }
}
