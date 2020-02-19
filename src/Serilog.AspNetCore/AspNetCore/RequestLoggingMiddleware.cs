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
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Serilog.AspNetCore
{
    class RequestLoggingMiddleware
    {
        readonly RequestDelegate _next;
        readonly DiagnosticContext _diagnosticContext;
        readonly MessageTemplate _messageTemplate;
        readonly Action<IDiagnosticContext, HttpContext> _enrichDiagnosticContext;
        readonly Func<IDiagnosticContext, HttpContext, Task> _enrichDiagnosticContextAsync;
        readonly Func<HttpContext, double, Exception, LogEventLevel> _getLevel;
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
        }

        // ReSharper disable once UnusedMember.Global
        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            var start = Stopwatch.GetTimestamp();

            var collector = _diagnosticContext.BeginCollection();
            try
            {
                await _next(httpContext);
                var elapsedMs = GetElapsedMilliseconds(start, Stopwatch.GetTimestamp());
                var statusCode = httpContext.Response.StatusCode;
                await LogCompletion(httpContext, collector, statusCode, elapsedMs, null);
            }
            catch (Exception ex)
            {
                await LogCompletion(httpContext, collector, 500, GetElapsedMilliseconds(start, Stopwatch.GetTimestamp()), ex);
                throw;
            }
            finally
            {
                collector.Dispose();
            }
        }

        async Task LogCompletion(HttpContext httpContext, DiagnosticContextCollector collector, int statusCode, double elapsedMs, Exception ex)
        {
            var logger = Log.ForContext<RequestLoggingMiddleware>();
            var level = _getLevel(httpContext, elapsedMs, ex);

            if (!logger.IsEnabled(level)) return;

            // Enrich diagnostic context
            _enrichDiagnosticContext?.Invoke(_diagnosticContext, httpContext);
            var task = _enrichDiagnosticContextAsync?.Invoke(_diagnosticContext, httpContext);
            if (task != null)
                await task;

            if (!collector.TryComplete(out var collectedProperties))
                collectedProperties = NoProperties;

            // Last-in (correctly) wins...
            var properties = collectedProperties.Concat(new[]
            {
                new LogEventProperty("RequestMethod", new ScalarValue(httpContext.Request.Method)),
                new LogEventProperty("RequestPath", new ScalarValue(GetPath(httpContext))),
                new LogEventProperty("StatusCode", new ScalarValue(statusCode)),
                new LogEventProperty("Elapsed", new ScalarValue(elapsedMs))
            });

            var evt = new LogEvent(DateTimeOffset.Now, level, ex, _messageTemplate, properties);
            logger.Write(evt);
        }

        static double GetElapsedMilliseconds(long start, long stop)
        {
            return (stop - start) * 1000 / (double)Stopwatch.Frequency;
        }

        static string GetPath(HttpContext httpContext)
        {
            return httpContext.Features.Get<IHttpRequestFeature>()?.RawTarget ?? httpContext.Request.Path.ToString();
        }
    }
}
