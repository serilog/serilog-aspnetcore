// Copyright 2019-2020 Serilog Contributors
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
using Serilog.Events;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Serilog.AspNetCore;

/// <summary>
/// Contains options for the <see cref="RequestLoggingMiddleware"/>.
/// </summary>
public class RequestLoggingOptions
{
    const string DefaultRequestCompletionMessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

    static LogEventLevel DefaultGetLevel(HttpContext ctx, double _, Exception? ex) =>
        ex != null
            ? LogEventLevel.Error
            : ctx.Response.StatusCode > 499
                ? LogEventLevel.Error
                : LogEventLevel.Information;

    static IEnumerable<LogEventProperty> DefaultGetMessageTemplateProperties(HttpContext httpContext, string requestPath, double elapsedMs, int statusCode) =>
        new[]
        {
            new LogEventProperty("RequestMethod", new ScalarValue(httpContext.Request.Method)),
            new LogEventProperty("RequestPath", new ScalarValue(requestPath)),
            new LogEventProperty("StatusCode", new ScalarValue(statusCode)),
            new LogEventProperty("Elapsed", new ScalarValue(elapsedMs))
        };

    /// <summary>
    /// Gets or sets the message template. The default value is
    /// <c>"HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms"</c>. The
    /// template can contain any of the placeholders from the default template, names of properties
    /// added by ASP.NET Core, and names of properties added to the <see cref="IDiagnosticContext"/>.
    /// </summary>
    /// <value>
    /// The message template.
    /// </value>
    public string MessageTemplate { get; set; }

    /// <summary>
    /// A function returning the <see cref="LogEventLevel"/> based on the <see cref="HttpContext"/>, the number of
    /// elapsed milliseconds required for handling the request, and an <see cref="Exception" /> if one was thrown.
    /// The default behavior returns <see cref="LogEventLevel.Error"/> when the response status code is greater than 499 or if the
    /// <see cref="Exception"/> is not null.
    /// </summary>
    /// <value>
    /// A function returning the <see cref="LogEventLevel"/>.
    /// </value>
    public Func<HttpContext, double, Exception?, LogEventLevel> GetLevel { get; set; }

    /// <summary>
    /// A callback that can be used to set additional properties on the request completion event.
    /// </summary>
    public Action<IDiagnosticContext, HttpContext>? EnrichDiagnosticContext { get; set; }

    /// <summary>
    /// The logger through which request completion events will be logged. The default is to use the
    /// static <see cref="Log"/> class.
    /// </summary>
    public ILogger? Logger { get; set; }

    /// <summary>
    /// Include the full URL query string in the <c>RequestPath</c> property
    /// that is attached to request log events. The default is <c>false</c>.
    /// </summary>
    public bool IncludeQueryInRequestPath { get; set; }

    /// <summary>
    /// A function to specify the values of the MessageTemplateProperties.
    /// </summary>
    public Func<HttpContext, string, double, int, IEnumerable<LogEventProperty>> GetMessageTemplateProperties { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public RequestLoggingOptions()
    {
        GetLevel = DefaultGetLevel;
        MessageTemplate = DefaultRequestCompletionMessageTemplate;
        GetMessageTemplateProperties = DefaultGetMessageTemplateProperties;
    }
}