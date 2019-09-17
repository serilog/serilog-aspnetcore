using Microsoft.Extensions.DependencyInjection;

namespace Serilog.AspNetCore.Mvc
{
    public static class MvcLoggingExtensionMethods
    {
        public static IMvcBuilder AddMvcLogging(this IMvcBuilder builder)
        {
            builder.Services.AddTransient<MvcRequestLoggingFilter>();
            builder.AddMvcOptions(options =>
            {
                options.Filters.AddService<MvcRequestLoggingFilter>();
            });
            return builder;
        }
    }
}

