# Setup up for older versions of ASP.Net

## ASP.Net <=5.0 Serilog setup instructions

**First**, install the _Serilog.AspNetCore_ [NuGet package](https://www.nuget.org/packages/Serilog.AspNetCore) into your app.

```shell
dotnet add package Serilog.AspNetCore
```

**Next**, in your application's _Program.cs_ file, configure Serilog first.  A `try`/`catch` block will ensure any configuration issues are appropriately logged:


```csharp
using Serilog;
using Serilog.Events;

public class Program
{
    public static int Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            Log.Information("Starting web host");
            CreateHostBuilder(args).Build().Run();
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
```

**Then**, add `UseSerilog()` to the Generic Host in `CreateHostBuilder()`.

```csharp
    public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog() // <-- Add this line
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
}
```

**Finally**, clean up by removing the remaining configuration for the default logger:

1. Remove the `"Logging"` section from _appsettings.*.json_ files (this can be replaced with [Serilog configuration](https://github.com/serilog/serilog-settings-configuration) as shown in [the _Sample_ project](https://github.com/serilog/serilog-aspnetcore/blob/dev/samples/Sample/Program.cs), if required)
2. Remove `UseApplicationInsights()` (this can be replaced with the [Serilog AI sink](https://github.com/serilog/serilog-sinks-applicationinsights), if required)

That's it! With the level bumped up a little you will see log output resembling:

``` bash
[22:14:44.646 DBG] RouteCollection.RouteAsync
    Routes: 
        Microsoft.AspNet.Mvc.Routing.AttributeRoute
        {controller=Home}/{action=Index}/{id?}
    Handled? True
[22:14:44.647 DBG] RouterMiddleware.Invoke
    Handled? True
[22:14:45.706 DBG] /lib/jquery/jquery.js not modified
[22:14:45.706 DBG] /css/site.css not modified
[22:14:45.741 DBG] Handled. Status code: 304 File: /css/site.css
```
