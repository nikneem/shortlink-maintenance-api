using System;
using System.Net.Http;
using System.Threading.Tasks;
using HexMaster.Functions.JwtBinding;
using HexMaster.Functions.JwtBinding.Model;
using HexMaster.ShortLink.Core.Contracts;
using HexMaster.ShortLink.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace HexMaster.ShortLink.Maintenance.Functions.ShortLinks
{

    public  class ShortLinkUpdateFunction
    {

        private readonly IShortLinksService _service;
        private readonly ILogger<ShortLinkCreateFunction> _logger;


        [FunctionName("ShortLinkUpdateFunction")]
        public  async Task<IActionResult> Run(
            [HttpTrigger( "put", Route = "shortlink/{id:guid}")] HttpRequestMessage req,
            [JwtBinding("%JwtBinding:Issuer%", "%JwtBinding:Audience%")] AuthorizedModel auth,
            Guid id)
        {
            try
            {
                var model = await req.Content.ReadAsAsync<ShortLinkUpdateDto>();
                 await _service.UpdateAsync(auth.Subject, id, model);
                return new OkResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred while trying to add a new short link");
                return new BadRequestObjectResult(ex.Message);
            }
        }

        public ShortLinkUpdateFunction(IShortLinksService service,
            ILogger<ShortLinkCreateFunction> logger)
        {
            _service = service;
            _logger = logger;
        }

    }
}
