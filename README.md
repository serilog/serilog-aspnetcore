# Serilog.AspNetCore [![Build status](https://ci.appveyor.com/api/projects/status/4rscdto23ik6vm2r?svg=true)](https://ci.appveyor.com/project/serilog/serilog-aspnetcore) [![NuGet Version](http://img.shields.io/nuget/v/Serilog.AspNetCore.svg?style=flat)](https://www.nuget.org/packages/Serilog.AspNetCore/) 


Serilog logging for ASP.NET Core. This package routes ASP.NET Core log messages through Serilog, so you can get information about ASP.NET's internal operations logged to the same Serilog sinks as your application events.

### Instructions

**First**, install the _Serilog.AspNetCore_ [NuGet package](https://www.nuget.org/packages/Serilog.AspNetCore) into your app. You will need a way to view the log messages - _Serilog.Sinks.Console_ writes these to the console; there are [many more sinks available](https://www.nuget.org/packages?q=Tags%3A%22serilog%22) on NuGet.

```powershell
Install-Package Serilog.AspNetCore -DependencyVersion Highest
Install-Package Serilog.Sinks.Console
```

**Next**, in your application's _Program.cs_ file, configure Serilog first.  A `try`/`catch` block will ensure any configuration issues are appropriately logged:

```csharp
public class Program
{
    public static int Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            Log.Information("Starting web host");
            BuildWebHost(args).Run();
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

**Then**, add `UseSerilog()` to the web host builder in `BuildWebHost()`.

```csharp    
    public static IWebHost BuildWebHost(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
	    .UseStartup<Startup>()
            .UseSerilog() // <-- Add this line
            .Build();
}
```

**Finally**, clean up by removing the remaining configuration for the default logger:

 * Remove calls to `AddLogging()`
 * Remove the `"Logging"` section from _appsettings.json_ files (this can be replaced with [Serilog configuration](https://github.com/serilog/serilog-settings-configuration) as shown in [this example](https://github.com/serilog/serilog-aspnetcore/blob/dev/samples/SimpleWebSample/Program.cs), if required)
 * Remove `ILoggerFactory` parameters and any `Add*()` calls on the logger factory in _Startup.cs_
 * Remove `UseApplicationInsights()` (this can be replaced with the [Serilog AI sink](https://github.com/serilog/serilog-sinks-applicationinsights), if required)

That's it! With the level bumped up a little you will see log output like:

```
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

Tip: to see Serilog output in the Visual Studio output window when running under IIS, select _ASP.NET Core Web Server_ from the _Show output from_ drop-down list.

A more complete example, showing _appsettings.json_ configuration, can be found in [the sample project here](https://github.com/serilog/serilog-aspnetcore/tree/dev/samples/SimpleWebSample).

### Using the package

With _Serilog.AspNetCore_ installed and configured, you can write log messages directly through Serilog or any `ILogger` interface injected by ASP.NET. All loggers will use the same underlying implementation, levels, and destinations.

**Tip:** change the minimum level for `Microsoft` to `Warning` and plug in this [custom logging middleware](https://github.com/datalust/serilog-middleware-example/blob/master/src/Datalust.SerilogMiddlewareExample/Diagnostics/SerilogMiddleware.cs) to clean up request logging output and record more context around errors and exceptions.

### Inline initialization

You can alternatively configure Serilog using a delegate as shown below:

```csharp
    // dotnet add package Serilog.Settings.Configuration
    .UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
	.ReadFrom.Configuration(hostingContext.Configuration)
	.Enrich.FromLogContext()
	.WriteTo.Console())
```

This has the advantage of making the `hostingContext`'s `Configuration` object available for configuration of the logger, but at the expense of recording `Exception`s raised earlier in program startup.

If this method is used, `Log.Logger` is assigned implicitly, and closed when the app is shut down.

Note: Configuring Serilog via the `hostingContext`'s `Configuration` object requires that you've installed the _Serilog.Settings.Configuration_ [NuGet package](https://www.nuget.org/packages/Serilog.Settings.Configuration).

### Writing to the Azure Diagnostics Log Stream

The Azure Diagnostic Log Stream ships events from any files in the `D:\home\LogFiles\` folder. To enable this for your app, first install the _Serilog.Sinks.File_ package:

```powershell
Install-Package Serilog.Sinks.File
```

Then add a file sink to your `LoggerConfiguration`, taking care to set the `shared` and `flushToDiskInterval` parameters:

```csharp
    public static int Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
	    // Add this line:
	    .WriteTo.File(
	    	@"D:\home\LogFiles\Application\myapp.txt",
		fileSizeLimitBytes: 1_000_000,
		rollOnFileSizeLimit: true,
		shared: true,
		flushToDiskInterval: TimeSpan.FromSeconds(1))
            .CreateLogger();
```
