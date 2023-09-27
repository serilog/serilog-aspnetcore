// Copyright 2019 Serilog Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Serilog.Events;
using Serilog.Extensions.Hosting;
using Serilog.Parsing;
using System.Diagnostics;

namespace Serilog.AspNetCore;

// ReSharper disable once ClassNeverInstantiated.Global
class RequestLoggingMiddleware
{
    readonly RequestDelegate _next;
    readonly DiagnosticContext _diagnosticContext;
    readonly MessageTemplate _messageTemplate;
    readonly Action<IDiagnosticContext, HttpContext>? _enrichDiagnosticContext;
    readonly Func<IDiagnosticContext, HttpContext, Task>? _enrichDiagnosticContextAsync;
    readonly Func<HttpContext, double, Exception?, LogEventLevel> _getLevel;
    readonly Func<HttpContext, string, double, int, IEnumerable<LogEventProperty>> _getMessageTemplateProperties;
    readonly ILogger? _logger;
    readonly bool _includeQueryInRequestPath;
    static readonly LogEventProperty[] NoProperties = new LogEventProperty[0];

    public RequestLoggingMiddleware(RequestDelegate next, DiagnosticContext diagnosticContext, RequestLoggingOptions options)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _diagnosticContext = diagnosticContext ?? throw new ArgumentNullException(nameof(diagnosticContext));

        _getLevel = options.GetLevel;
        _enrichDiagnosticContext = options.EnrichDiagnosticContext;
        _enrichDiagnosticContextAsync = options.EnrichDiagnosticContextAsync;
        _messageTemplate = new MessageTemplateParser().Parse(options.MessageTemplate);
        _logger = options.Logger?.ForContext<RequestLoggingMiddleware>();
        _includeQueryInRequestPath = options.IncludeQueryInRequestPath;
        _getMessageTemplateProperties = options.GetMessageTemplateProperties;
    }

    // ReSharper disable once UnusedMember.Global
    public async Task Invoke(HttpContext httpContext)
    {
        if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

        var start = Stopwatch.GetTimestamp();
        var collector = _diagnosticContext.BeginCollection();
        ILogger? logger = null;
        var level = LogEventLevel.Information; // only used if _getLevel throws an exception
        double elapsedMs;

        try
        {
            await _next(httpContext);
            elapsedMs = GetElapsedMilliseconds(start, Stopwatch.GetTimestamp());
            var statusCode = httpContext.Response.StatusCode;
            logger = _logger ?? Log.ForContext<RequestLoggingMiddleware>();
            level = _getLevel(httpContext, elapsedMs, null);
            LogCompletion(httpContext, collector, statusCode, elapsedMs, logger, level, null);
        }
        catch (Exception ex)
            // Never caught, because `LogCompletion()` returns false. This ensures e.g. the developer exception page is still
            // shown, although it does also mean we see a duplicate "unhandled exception" event from ASP.NET Core.
            // The elapsedMs is required for calculating the level and for LogCompletion.
            // The level is required in the finally block.
            when ((elapsedMs = GetElapsedMilliseconds(start, Stopwatch.GetTimestamp())) >= 0
                  && (level = _getLevel(httpContext, elapsedMs, ex)) >= 0
                  && LogCompletion(httpContext, collector, 500, elapsedMs, logger, level, ex))
        {
        }
        finally
        {
            await CallEnrichDiagnosticContextAsync(httpContext, logger, level);
            collector.Dispose();
        }
    }

    async Task CallEnrichDiagnosticContextAsync(HttpContext httpContext, ILogger? logger, LogEventLevel level)
    {
        try
        {
            logger ??= Log.ForContext<RequestLoggingMiddleware>();
            if (!logger.IsEnabled(level)) return;

            if (_enrichDiagnosticContextAsync != null)
            {
                await _enrichDiagnosticContextAsync.Invoke(_diagnosticContext, httpContext);
            }
        }
        catch
        {
            // we want to avoid throwing exceptions in the logging pipeline, so we just swallow them here
        }
    }

    bool LogCompletion(HttpContext httpContext, DiagnosticContextCollector collector, int statusCode, double elapsedMs, ILogger? logger, LogEventLevel level, Exception? ex)
    {
        logger ??= Log.ForContext<RequestLoggingMiddleware>();

        if (!logger.IsEnabled(level)) return false;

        // Enrich diagnostic context
        _enrichDiagnosticContext?.Invoke(_diagnosticContext, httpContext);

        if (!collector.TryComplete(out var collectedProperties, out var collectedException))
            collectedProperties = NoProperties;

        // Last-in (correctly) wins...
        var properties = collectedProperties.Concat(_getMessageTemplateProperties(httpContext, GetPath(httpContext, _includeQueryInRequestPath), elapsedMs, statusCode));

        var evt = new LogEvent(DateTimeOffset.Now, level, ex ?? collectedException, _messageTemplate, properties);
        logger.Write(evt);

        return false;
    }

    static double GetElapsedMilliseconds(long start, long stop)
    {
        return (stop - start) * 1000 / (double)Stopwatch.Frequency;
    }

    static string GetPath(HttpContext httpContext, bool includeQueryInRequestPath)
    {
        /*
            In some cases, like when running integration tests with WebApplicationFactory<T>
            the Path returns an empty string instead of null, in that case we can't use
            ?? as fallback.
        */
        var requestPath = includeQueryInRequestPath
            ? httpContext.Features.Get<IHttpRequestFeature>()?.RawTarget
            : httpContext.Features.Get<IHttpRequestFeature>()?.Path;
        if (string.IsNullOrEmpty(requestPath))
        {
            requestPath = httpContext.Request.Path.ToString();
        }

        return requestPath!;
    }
}