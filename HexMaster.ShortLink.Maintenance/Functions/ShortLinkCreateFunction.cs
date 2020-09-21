using System;
using System.Net.Http;
using System.Threading.Tasks;
using HexMaster.Functions.JwtBinding;
using HexMaster.Functions.JwtBinding.Model;
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
        private readonly ILogger<ShortLinkCreateFunction> _logger;

        [FunctionName("ShortLinkCreateFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.User, "post", Route = "shortlinks")] HttpRequestMessage req,
            [JwtBinding("%JwtBinding:Issuer%", "%JwtBinding:Audience%")] AuthorizedModel auth
            )
        {
            try
            {
                var model = await req.Content.ReadAsAsync<ShortLinkCreateDto>();
                var createdModel = await _service.CreateAsync(model, auth.Subject);
                _logger.LogWarning($"Created a new ShotLink to https://4dn.me/{createdModel.ShortCode}");
                return new CreatedResult("https://app.4dn.me", createdModel);
            }
            catch (ModelValidationException validationEx)
            {
                _logger.LogWarning(validationEx, "Validation error occurred while trying to add a new short link");
                return new BadRequestObjectResult(validationEx.Errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred while trying to add a new short link");
                return new BadRequestObjectResult(ex.Message);
            }
        }

        public ShortLinkCreateFunction(IShortLinksService service,
            ILogger<ShortLinkCreateFunction> logger)
        {
            _service = service;
            _logger = logger;
        }

    }
}
