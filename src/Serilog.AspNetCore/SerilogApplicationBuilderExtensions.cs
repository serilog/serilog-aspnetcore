// Copyright 2017-2019 Serilog Contributors
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
        /// <summary>
        /// Adds middleware for streamlined request logging. Instead of writing HTTP request information
        /// like method, path, timing, status code and exception details
        /// in several events, this middleware collects information during the request (including from
        /// <see cref="IDiagnosticContext"/>), and writes a single event at request completion. Add this
        /// in <c>Startup.cs</c> before any handlers whose activities should be logged.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <returns>The application builder.</returns>
        public static IApplicationBuilder UseSerilogRequestLogging(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            return app.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}
