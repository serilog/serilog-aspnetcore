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

using Microsoft.Extensions.Logging;
using Serilog.Debugging;
using Serilog.Extensions.Logging;

namespace Serilog.AspNetCore
{
	/// <summary>
	/// Implements Microsoft's ILoggerFactory so that we can inject Serilog Logger.
	/// </summary>
	/// <seealso cref="Microsoft.Extensions.Logging.ILoggerFactory" />
	public class SerilogLoggerFactory : ILoggerFactory
    {
        readonly SerilogLoggerProvider _provider;

		/// <summary>
		/// Initializes a new instance of the <see cref="SerilogLoggerFactory"/> class.
		/// </summary>
		/// <param name="logger">The logger.</param>
		/// <param name="dispose">if set to <c>true</c> [dispose].</param>
		public SerilogLoggerFactory(Serilog.ILogger logger = null, bool dispose = false)
        {
            _provider = new SerilogLoggerProvider(logger, dispose);
        }

		/// <summary>
		/// Disposes the provider.
		/// </summary>
		public void Dispose()
        {
            _provider.Dispose();
        }

		/// <summary>
		/// Creates a new <see cref="T:Microsoft.Extensions.Logging.ILogger" /> instance.
		/// </summary>
		/// <param name="categoryName">The category name for messages produced by the logger.</param>
		/// <returns>
		/// The <see cref="T:Microsoft.Extensions.Logging.ILogger" />.
		/// </returns>
		public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
            return _provider.CreateLogger(categoryName);
        }

		/// <summary>
		/// Adds an <see cref="T:Microsoft.Extensions.Logging.ILoggerProvider" /> to the logging system.
		/// </summary>
		/// <param name="provider">The <see cref="T:Microsoft.Extensions.Logging.ILoggerProvider" />.</param>
		public void AddProvider(ILoggerProvider provider)
        {
            SelfLog.WriteLine("Ignoring added logger provider {0}", provider);
        }
    }
}
