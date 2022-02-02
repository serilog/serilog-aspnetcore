// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Builder;
using Serilog.Filters;
using Serilog.AspNetCore.Tests.Support;

// Newer frameworks provide IHostBuilder
#pragma warning disable CS0618

namespace Serilog.AspNetCore.Tests
{
    public class SerilogWebHostBuilderExtensionsTests : IClassFixture<SerilogWebApplicationFactory>
    {
        readonly SerilogWebApplicationFactory _web;

        public SerilogWebHostBuilderExtensionsTests(SerilogWebApplicationFactory web)
        {
            _web = web;
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DisposeShouldBeHandled(bool dispose)
        {
            var logger = new DisposeTrackingLogger();
            using (var web = Setup(logger, dispose))
            {
                await web.CreateClient().GetAsync("/");
            }

            Assert.Equal(dispose, logger.IsDisposed);
        }

        [Fact]
        public async Task RequestLoggingMiddlewareShouldEnrich()
        {
            var (sink, web) = Setup(options =>
            {
                options.EnrichDiagnosticContext += (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("SomeInteger", 42);
                };
            });

            await web.CreateClient().GetAsync("/resource");

            Assert.NotEmpty(sink.Writes);

            var completionEvent = sink.Writes.First(logEvent => Matching.FromSource<RequestLoggingMiddleware>()(logEvent));

            Assert.Equal(42, completionEvent.Properties["SomeInteger"].LiteralValue());
            Assert.Equal("string", completionEvent.Properties["SomeString"].LiteralValue());
            Assert.Equal("/resource", completionEvent.Properties["RequestPath"].LiteralValue());
            Assert.Equal(200, completionEvent.Properties["StatusCode"].LiteralValue());
            Assert.Equal("GET", completionEvent.Properties["RequestMethod"].LiteralValue());
            Assert.True(completionEvent.Properties.ContainsKey("Elapsed"));
        }

        WebApplicationFactory<TestStartup> Setup(ILogger logger, bool dispose, Action<RequestLoggingOptions> configureOptions = null)
        {
            var web = _web.WithWebHostBuilder(
                builder => builder
                    .ConfigureServices(sc => sc.Configure<RequestLoggingOptions>(options =>
                    {
                        options.Logger = logger;
                        options.EnrichDiagnosticContext += (diagnosticContext, httpContext) =>
                        {
                            diagnosticContext.Set("SomeString", "string");
                        };
                    }))
                    .Configure(app =>
                    {
                        app.UseSerilogRequestLogging(configureOptions);
                        app.Run(_ => Task.CompletedTask); // 200 OK
                    })
                    .UseSerilog(logger, dispose));

            return web;
        }

        (SerilogSink, WebApplicationFactory<TestStartup>) Setup(Action<RequestLoggingOptions> configureOptions = null)
        {
            var sink = new SerilogSink();
            var logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Sink(sink)
                .CreateLogger();

            var web = Setup(logger, true, configureOptions);

            return (sink, web);
        }
    }
}