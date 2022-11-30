// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Serilog.Filters;
using Serilog.AspNetCore.Tests.Support;
using Serilog.Events;

// Newer frameworks provide IHostBuilder
#pragma warning disable CS0618

namespace Serilog.AspNetCore.Tests;

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
            options.EnrichDiagnosticContext += (diagnosticContext, _) =>
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

    [Fact]
    public async Task RequestLoggingMiddlewareShouldEnrichWithCustomisedProperties()
    {
        var (sink, web) = Setup(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} responded {Status} in {ElapsedMilliseconds:0.0000} ms";
            options.GetMessageTemplateProperties = (ctx, _, elapsedMs, status) =>
                new[]
                {
                    new LogEventProperty("RequestMethod", new ScalarValue(ctx.Request.Method)),
                    new LogEventProperty("Status", new ScalarValue(status)),
                    new LogEventProperty("ElapsedMilliseconds", new ScalarValue(elapsedMs))
                };
        });

        await web.CreateClient().GetAsync("/resource");

        Assert.NotEmpty(sink.Writes);

        var completionEvent = sink.Writes.First(logEvent => Matching.FromSource<RequestLoggingMiddleware>()(logEvent));

        Assert.Equal("string", completionEvent.Properties["SomeString"].LiteralValue());
        Assert.Equal(200, completionEvent.Properties["Status"].LiteralValue());
        Assert.Equal("GET", completionEvent.Properties["RequestMethod"].LiteralValue());
        Assert.True(completionEvent.Properties.ContainsKey("ElapsedMilliseconds"));
        Assert.False(completionEvent.Properties.ContainsKey("Elapsed"));
    }

    [Fact]
    public async Task RequestLoggingMiddlewareShouldEnrichWithCollectedExceptionIfNoUnhandledException()
    {
        var diagnosticContextException = new Exception("Exception set in diagnostic context");
        var (sink, web) = Setup(options =>
        {
            options.EnrichDiagnosticContext += (diagnosticContext, _) =>
            {
                diagnosticContext.SetException(diagnosticContextException);
            };
        });

        await web.CreateClient().GetAsync("/resource");

        var completionEvent = sink.Writes.First(logEvent => Matching.FromSource<RequestLoggingMiddleware>()(logEvent));

        Assert.Same(diagnosticContextException, completionEvent.Exception);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RequestLoggingMiddlewareShouldEnrichWithUnhandledExceptionEvenIfExceptionIsSetInDiagnosticContext(bool setExceptionInDiagnosticContext)
    {
        var diagnosticContextException = new Exception("Exception set in diagnostic context");
        var unhandledException = new Exception("Unhandled exception thrown in API action");
        var (sink, web) = Setup(options =>
        {
            options.EnrichDiagnosticContext += (diagnosticContext, _) =>
            {
                if (setExceptionInDiagnosticContext)
                    diagnosticContext.SetException(diagnosticContextException);
            };
        }, actionCallback: _ => throw unhandledException);

        Func<Task> act = () => web.CreateClient().GetAsync("/resource");

        var thrownException = await Assert.ThrowsAsync<Exception>(act);
        var completionEvent = sink.Writes.First(logEvent => Matching.FromSource<RequestLoggingMiddleware>()(logEvent));
        Assert.Same(unhandledException, completionEvent.Exception);
        Assert.Same(unhandledException, thrownException);
    }

    WebApplicationFactory<TestStartup> Setup(
        ILogger logger,
        bool dispose,
        Action<RequestLoggingOptions>? configureOptions = null,
        Action<HttpContext>? actionCallback = null)
    {
        var web = _web.WithWebHostBuilder(
            builder => builder
                .ConfigureServices(sc => sc.Configure<RequestLoggingOptions>(options =>
                {
                    options.Logger = logger;
                    options.EnrichDiagnosticContext += (diagnosticContext, _) =>
                    {
                        diagnosticContext.Set("SomeString", "string");
                    };
                }))
                .Configure(app =>
                {
                    app.UseSerilogRequestLogging(configureOptions);
                    app.Run(ctx =>
                    {
                        actionCallback?.Invoke(ctx);
                        return Task.CompletedTask;
                    }); // 200 OK
                })
                .UseSerilog(logger, dispose));

        return web;
    }

    (SerilogSink, WebApplicationFactory<TestStartup>) Setup(
        Action<RequestLoggingOptions>? configureOptions = null,
        Action<HttpContext>? actionCallback = null)
    {
        var sink = new SerilogSink();
        var logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Sink(sink)
            .CreateLogger();

        var web = Setup(logger, true, configureOptions, actionCallback);

        return (sink, web);
    }
}