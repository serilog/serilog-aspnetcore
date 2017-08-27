// Copyright 2017 Serilog Contributors
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
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace Serilog
{
    /// <summary>
    /// Extends <see cref="IWebHostBuilder"/> with Serilog configuration methods.
    /// </summary>
    public static class SerilogWebHostBuilderExtensions
    {
        /// <summary>
        /// Sets Serilog as the logging provider.
        /// </summary>
        /// <param name="builder">The web host builder to configure.</param>
        /// <param name="logger">The Serilog logger; if not supplied, the static <see cref="Serilog.Log"/> will be used.</param>
        /// <param name="dispose">When true, dispose <paramref name="logger"/> when the framework disposes the provider. If the
        /// logger is not specified but <paramref name="dispose"/> is true, the <see cref="Log.CloseAndFlush()"/> method will be
        /// called on the static <see cref="Log"/> class instead.</param>
        /// <returns>The web host builder.</returns>
        public static IWebHostBuilder UseSerilog(this IWebHostBuilder builder, Serilog.ILogger logger = null, bool dispose = false)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            builder.ConfigureServices(collection =>
                collection.AddSingleton<ILoggerFactory>(new SerilogLoggerFactory(logger, dispose)));
            return builder;
        }
    }
}
