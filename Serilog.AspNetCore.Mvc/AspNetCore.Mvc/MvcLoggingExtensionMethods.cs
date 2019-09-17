using Microsoft.Extensions.DependencyInjection;
using System;

namespace Serilog.AspNetCore.Mvc
{
    /// <summary>
    /// Extension methods for using MvcRequestLoggingFilter.
    /// </summary>
    public static class MvcLoggingExtensionMethods
    {
        /// <summary>
        /// Adds AspNetCore Mvc logging to serilog hosting logging through the use of MvcRequestLoggingFilter.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMvcBuilder AddSerilogMvcLogging(this IMvcBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.AddTransient<MvcRequestLoggingFilter>();
            builder.AddMvcOptions(options =>
            {
                options.Filters.AddService<MvcRequestLoggingFilter>();
            });

            return builder;
        }
    }
}