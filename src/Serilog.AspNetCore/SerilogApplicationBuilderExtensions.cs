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

using System;
using Microsoft.AspNetCore.Builder;
using Serilog.AspNetCore;

namespace Serilog
{
    /// <summary>
    /// Extends <see cref="IApplicationBuilder"/> with methods for configuring Serilog features.
    /// </summary>
    public static class SerilogApplicationBuilderExtensions
    {
        const string DefaultRequestCompletionMessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

        /// <summary>
        /// Adds middleware for streamlined request logging. Instead of writing HTTP request information
        /// like method, path, timing, status code and exception details
        /// in several events, this middleware collects information during the request (including from
        /// <see cref="IDiagnosticContext"/>), and writes a single event at request completion. Add this
        /// in <c>Startup.cs</c> before any handlers whose activities should be logged.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="messageTemplate">The message template to use when logging request completion
        /// events. The default is
        /// <c>"HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms"</c>. The
        /// template can contain any of the placeholders from the default template, names of properties
        /// added by ASP.NET Core, and names of properties added to the <see cref="IDiagnosticContext"/>.
        /// </param>
        /// <returns>The application builder.</returns>
        public static IApplicationBuilder UseSerilogRequestLogging(
            this IApplicationBuilder app,
            string messageTemplate = DefaultRequestCompletionMessageTemplate)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            if (messageTemplate == null) throw new ArgumentNullException(nameof(messageTemplate));
            return app.UseMiddleware<RequestLoggingMiddleware>(new RequestLoggingOptions(messageTemplate));
        }
    }
}
