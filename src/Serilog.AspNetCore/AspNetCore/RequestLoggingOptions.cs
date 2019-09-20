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
using Serilog.Events;
using System;

namespace Serilog.AspNetCore
{
    /// <summary>
    /// Contains options for the <see cref="Serilog.AspNetCore.RequestLoggingMiddleware"/>.
    /// </summary>
    public class RequestLoggingOptions
    {
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
        /// Gets or sets the function returning the <see cref="LogEventLevel"/> based on the <see cref="HttpContext"/> and on the <see cref="Exception" /> if something wrong happend 
        /// The default behavior returns LogEventLevel.Error when HttpStatusCode is greater than 499 or if Exception is not null.
        /// </summary>
        /// <value>
        /// The function returning the <see cref="LogEventLevel"/>.
        /// </value>
        public Func<HttpContext, Exception, LogEventLevel> GetLevel { get; set; }

        internal RequestLoggingOptions() { }
    }
}