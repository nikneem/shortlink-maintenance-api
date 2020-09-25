using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HexMaster.ShortLink.Core.Entities;
using HexMaster.ShortLink.Core.Models.Analytics;
using HexMaster.ShortLink.Resolver.Functions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;

namespace HexMaster.ShortLink.Tests
{
    public class Tests
    {
        [Test]
        public async Task ResolveShortCode_Success()
        {
            // Assemble
            var targetEndpoint = "https://google.com";
            var context = new DefaultHttpContext();
            var request = context.Request;
            
            var collectorMock = new Mock<IAsyncCollector<LinkClickedMessage>>();

            var entity = new ShortLinkEntity() { EndpointUrl = targetEndpoint };
            
            var ctor = typeof(TableQuerySegment<ShortLinkEntity>)
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(c => c.GetParameters().Count() == 1);

            var segmentMock = ctor.Invoke(new object[] { new List<ShortLinkEntity>() { entity} }) as TableQuerySegment<ShortLinkEntity>;

            var tableMock = new Mock<CloudTable>(new Uri("https://hexmaster.com/account"), (TableClientConfiguration)null);
            tableMock.Setup(t => t.ExecuteQuerySegmentedAsync(It.IsAny<TableQuery<ShortLinkEntity>>(), It.IsAny<TableContinuationToken>()))
                     .ReturnsAsync(segmentMock);

            var logger = NullLogger.Instance;

            // Act
            var result = await ResolveShortCodeFunction.ResolveShortCode(request, collectorMock.Object, tableMock.Object, "CODE", logger);

            // Assert
            Assert.IsInstanceOf<RedirectResult>(result);
            var r = result as RedirectResult;
            Assert.AreEqual(targetEndpoint, r.Url);
        }

        [Test]
        [TestCase(" ")]
        [TestCase("")]
        [TestCase(null)]
        public async Task ResolveShortCode_Fail_InvalidCode(string code)
        {
            // Assemble
            var targetEndpoint = "https://google.com";
            var context = new DefaultHttpContext();
            var request = context.Request;
            
            var collectorMock = new Mock<IAsyncCollector<LinkClickedMessage>>();

            var entity = new ShortLinkEntity() { EndpointUrl = targetEndpoint };
            
            var ctor = typeof(TableQuerySegment<ShortLinkEntity>)
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(c => c.GetParameters().Count() == 1);

            var segmentMock = ctor.Invoke(new object[] { new List<ShortLinkEntity>() { entity} }) as TableQuerySegment<ShortLinkEntity>;

            var tableMock = new Mock<CloudTable>(new Uri("https://hexmaster.com/account"), (TableClientConfiguration)null);
            tableMock.Setup(t => t.ExecuteQuerySegmentedAsync(It.IsAny<TableQuery<ShortLinkEntity>>(), It.IsAny<TableContinuationToken>()))
                     .ReturnsAsync(segmentMock);

            var logger = NullLogger.Instance;

            // Act
            var result = await ResolveShortCodeFunction.ResolveShortCode(request, collectorMock.Object, tableMock.Object, code, logger);

            // Assert
            Assert.IsInstanceOf<RedirectResult>(result);
            var r = result as RedirectResult;
            Assert.AreEqual("https://app.4dn.me/", r.Url);
        }

        [Test]
        public async Task ResolveShortCode_Fail_CodeNotFound()
        {
            // Assemble
            var targetEndpoint = "https://google.com";
            var context = new DefaultHttpContext();
            var request = context.Request;
            
            var collectorMock = new Mock<IAsyncCollector<LinkClickedMessage>>();

            var ctor = typeof(TableQuerySegment<ShortLinkEntity>)
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(c => c.GetParameters().Count() == 1);

            var segmentMock = ctor.Invoke(new object[] { new List<ShortLinkEntity>() }) as TableQuerySegment<ShortLinkEntity>;

            var tableMock = new Mock<CloudTable>(new Uri("https://hexmaster.com/account"), (TableClientConfiguration)null);
            tableMock.Setup(t => t.ExecuteQuerySegmentedAsync(It.IsAny<TableQuery<ShortLinkEntity>>(), It.IsAny<TableContinuationToken>()))
                     .ReturnsAsync(segmentMock);

            var logger = NullLogger.Instance;

            // Act
            var result = await ResolveShortCodeFunction.ResolveShortCode(request, collectorMock.Object, tableMock.Object, "CODE", logger);

            // Assert
            Assert.IsInstanceOf<RedirectResult>(result);
            var r = result as RedirectResult;
            Assert.AreEqual("https://app.4dn.me/", r.Url);
        }

        [Test]
        [TestCase(" ")]
        [TestCase("")]
        [TestCase(null)]
        public async Task ResolveShortCode_Fail_EmptyEndpoint(string endpoint)
        {
            // Assemble
            var targetEndpoint = "https://google.com";
            var context = new DefaultHttpContext();
            var request = context.Request;
            
            var collectorMock = new Mock<IAsyncCollector<LinkClickedMessage>>();

            var entity = new ShortLinkEntity() { EndpointUrl = endpoint };

            var ctor = typeof(TableQuerySegment<ShortLinkEntity>)
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(c => c.GetParameters().Count() == 1);

            var segmentMock = ctor.Invoke(new object[] { new List<ShortLinkEntity>() }) as TableQuerySegment<ShortLinkEntity>;

            var tableMock = new Mock<CloudTable>(new Uri("https://hexmaster.com/account"), (TableClientConfiguration)null);
            tableMock.Setup(t => t.ExecuteQuerySegmentedAsync(It.IsAny<TableQuery<ShortLinkEntity>>(), It.IsAny<TableContinuationToken>()))
                     .ReturnsAsync(segmentMock);

            var logger = NullLogger.Instance;

            // Act
            var result = await ResolveShortCodeFunction.ResolveShortCode(request, collectorMock.Object, tableMock.Object, "CODE", logger);

            // Assert
            Assert.IsInstanceOf<RedirectResult>(result);
            var r = result as RedirectResult;
            Assert.AreEqual("https://app.4dn.me/", r.Url);
        }
    }
}