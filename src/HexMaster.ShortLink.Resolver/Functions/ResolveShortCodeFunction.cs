using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HexMaster.ShortLink.Core;
using HexMaster.ShortLink.Core.Contracts;
using HexMaster.ShortLink.Core.Models.Analytics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using HexMaster.ShortLink.Core.Entities;

namespace HexMaster.ShortLink.Resolver.Functions
{
    public class ResolveShortCodeFunction
    {
        private readonly IShortLinksService _shortLinksService;
        private readonly ILogger<ResolveShortCodeFunction> _logger;

        [FunctionName("ResolveShortCodeFunction")]
        public async Task<IActionResult> ResolveShortCode(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{path}")]
            HttpRequest req,
            [EventHub(HubNames.ClickEventsHub, Connection = "CloudSettings:EventHubSenderConnectionString")]
            IAsyncCollector<LinkClickedMessage> outputEvents,
            string path)
        {
            var targetUrl = "https://app.4dn.me/";
            var now = DateTimeOffset.UtcNow;
            if (!string.IsNullOrWhiteSpace(path) && Regex.IsMatch(path, Constants.ShortCodeRegularExpression))
            {
                var targetEndpoint = await _shortLinksService.ResolveAsync(path);
                if (!string.IsNullOrWhiteSpace(targetEndpoint))
                {
                    _logger.LogInformation($"Path '{path}' was resolved to endpoint {targetEndpoint}, now redirecting.");
                    targetUrl = targetEndpoint;
                    var message = new LinkClickedMessage
                    {
                        Key = path,
                        ClickedAt = now
                    };
                    await outputEvents.AddAsync(message);
                    await outputEvents.FlushAsync();
                }
                else
                {
                    _logger.LogInformation($"The path ({path}) was not configured as a valid shortlink");
                }
            }
            else
            {
                _logger.LogInformation($"No matching path came in ({path}) so redirecting to the app");
            }

            return new RedirectResult(targetUrl, true);
        }

        public ResolveShortCodeFunction(IShortLinksService shortLinksService, 
            ILogger<ResolveShortCodeFunction> logger)
        {
            _shortLinksService = shortLinksService;
            _logger = logger;
        }
    }
}
