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
    public  class ShortLinkGetFunction
    {

        private readonly IShortLinksService _service;
        private readonly ILogger<ShortLinkCreateFunction> _logger;


        [FunctionName("ShortLinkGetFunction")]
        public  async Task<IActionResult> Run(
            [HttpTrigger( "get", Route = "shortlink/{id:guid}")] HttpRequest req,
            [JwtBinding("%JwtBinding:Issuer%", "%JwtBinding:Audience%")] AuthorizedModel auth,
            Guid id)
        {
            try
            {
                var details = await _service.ReadAsync(auth.Subject, id);
                return new OkObjectResult(details);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred while trying to add a new short link");
                return new BadRequestObjectResult(ex.Message);
            }
        }

        public ShortLinkGetFunction(IShortLinksService service,
            ILogger<ShortLinkCreateFunction> logger)
        {
            _service = service;
            _logger = logger;
        }

    }
}
