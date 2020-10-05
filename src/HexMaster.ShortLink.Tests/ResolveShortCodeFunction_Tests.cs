using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using HexMaster.ShortLink.Core.Contracts;
using HexMaster.ShortLink.Core.Entities;
using HexMaster.ShortLink.Core.Models.Analytics;
using HexMaster.ShortLink.Resolver.Functions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;

namespace HexMaster.ShortLink.Tests
{
    public class ResolveShortCodeFunction_Tests
    {

        private Mock<IShortLinksService> _shortLinksServiceMock;
        private Mock<ILogger<ResolveShortCodeFunction>> _loggerMock;
        private Mock<IAsyncCollector<LinkClickedMessage>> _eventHubMock;

        [SetUp]
        public void Setup()
        {
            _shortLinksServiceMock = new Mock<IShortLinksService>();
            _loggerMock = new Mock<ILogger<ResolveShortCodeFunction>>();
            _eventHubMock = new Mock<IAsyncCollector<LinkClickedMessage>>();
        }

        [Test]
        public async Task ResolveShortCode_Success()
        {
            var code = "shortcd";
            var endpoint = "https://www.google.com";
            WithValidShortCodeToEndpoint(code, endpoint);
            
            var functionClass = new ResolveShortCodeFunction(_shortLinksServiceMock.Object,_loggerMock.Object);
            var result = await functionClass.ResolveShortCode(null, _eventHubMock.Object,  code);

            Assert.IsInstanceOf<RedirectResult>(result);
            var redirectResult = result as RedirectResult;
            Assert.AreEqual(endpoint, redirectResult?.Url);
            _eventHubMock.Verify(evt => 
                    evt.AddAsync(It.IsAny<LinkClickedMessage>(), It.IsAny<CancellationToken>()),
                    Times.Once);

        }

        //[Test]
        //[TestCase(" ")]
        //[TestCase("")]
        //[TestCase(null)]
        //public async Task ResolveShortCode_Fail_InvalidCode(string code)
        //{
        //    // Assemble
        //    var targetEndpoint = "https://google.com";
        //    var context = new DefaultHttpContext();
        //    var request = context.Request;
            
        //    var collectorMock = new Mock<IAsyncCollector<LinkClickedMessage>>();

        //    var entity = new ShortLinkEntity() { EndpointUrl = targetEndpoint };
            
        //    var ctor = typeof(TableQuerySegment<ShortLinkEntity>)
        //        .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
        //        .FirstOrDefault(c => c.GetParameters().Count() == 1);

        //    var segmentMock = ctor.Invoke(new object[] { new List<ShortLinkEntity>() { entity} }) as TableQuerySegment<ShortLinkEntity>;

        //    var tableMock = new Mock<CloudTable>(new Uri("https://hexmaster.com/account"), (TableClientConfiguration)null);
        //    tableMock.Setup(t => t.ExecuteQuerySegmentedAsync(It.IsAny<TableQuery<ShortLinkEntity>>(), It.IsAny<TableContinuationToken>()))
        //             .ReturnsAsync(segmentMock);

        //    var logger = NullLogger.Instance;

        //    // Act
        //    var result = await ResolveShortCodeFunction.ResolveShortCode(request, collectorMock.Object, tableMock.Object, code, logger);

        //    // Assert
        //    Assert.IsInstanceOf<RedirectResult>(result);
        //    var redirectResult = result as RedirectResult;
        //    Assert.AreEqual("https://app.4dn.me/", redirectResult.Url);
        //}

        //[Test]
        //public async Task ResolveShortCode_Fail_CodeNotFound()
        //{
        //    // Assemble
        //    var context = new DefaultHttpContext();
        //    var request = context.Request;
            
        //    var collectorMock = new Mock<IAsyncCollector<LinkClickedMessage>>();

        //    var ctor = typeof(TableQuerySegment<ShortLinkEntity>)
        //        .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
        //        .FirstOrDefault(c => c.GetParameters().Count() == 1);

        //    var segmentMock = ctor.Invoke(new object[] { new List<ShortLinkEntity>() }) as TableQuerySegment<ShortLinkEntity>;

        //    var tableMock = new Mock<CloudTable>(new Uri("https://hexmaster.com/account"), (TableClientConfiguration)null);
        //    tableMock.Setup(t => t.ExecuteQuerySegmentedAsync(It.IsAny<TableQuery<ShortLinkEntity>>(), It.IsAny<TableContinuationToken>()))
        //             .ReturnsAsync(segmentMock);

        //    var logger = NullLogger.Instance;

        //    // Act
        //    var result = await ResolveShortCodeFunction.ResolveShortCode(request, collectorMock.Object, tableMock.Object, "CODE", logger);

        //    // Assert
        //    Assert.IsInstanceOf<RedirectResult>(result);
        //    var redirectResult = result as RedirectResult;
        //    Assert.AreEqual("https://app.4dn.me/", redirectResult.Url);
        //}

        //[Test]
        //[TestCase(" ")]
        //[TestCase("")]
        //[TestCase(null)]
        //public async Task ResolveShortCode_Fail_EmptyEndpoint(string endpoint)
        //{
        //    // Assemble
        //    var context = new DefaultHttpContext();
        //    var request = context.Request;
            
        //    var collectorMock = new Mock<IAsyncCollector<LinkClickedMessage>>();

        //    var ctor = typeof(TableQuerySegment<ShortLinkEntity>)
        //        .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
        //        .FirstOrDefault(c => c.GetParameters().Count() == 1);

        //    var segmentMock = ctor.Invoke(new object[] { new List<ShortLinkEntity>() }) as TableQuerySegment<ShortLinkEntity>;

        //    var tableMock = new Mock<CloudTable>(new Uri("https://hexmaster.com/account"), (TableClientConfiguration)null);
        //    tableMock.Setup(t => t.ExecuteQuerySegmentedAsync(It.IsAny<TableQuery<ShortLinkEntity>>(), It.IsAny<TableContinuationToken>()))
        //             .ReturnsAsync(segmentMock);

        //    var logger = NullLogger.Instance;

        //    // Act
        //    var result = await ResolveShortCodeFunction.ResolveShortCode(request, collectorMock.Object, tableMock.Object, "CODE", logger);

        //    // Assert
        //    Assert.IsInstanceOf<RedirectResult>(result);
        //    var redirectResult = result as RedirectResult;
        //    Assert.AreEqual("https://app.4dn.me/", redirectResult.Url);
        //}


        private void WithValidShortCodeToEndpoint(string code, string endpoint)
        {
            _shortLinksServiceMock.Setup(x => x.ResolveAsync(code)).ReturnsAsync(endpoint);
        }
    }
}