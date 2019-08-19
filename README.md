# Serilog.AspNetCore [![Build status](https://ci.appveyor.com/api/projects/status/4rscdto23ik6vm2r/branch/dev?svg=true)](https://ci.appveyor.com/project/serilog/serilog-aspnetcore/branch/dev) [![NuGet Version](http://img.shields.io/nuget/v/Serilog.AspNetCore.svg?style=flat)](https://www.nuget.org/packages/Serilog.AspNetCore/) [![NuGet Prerelease Version](http://img.shields.io/nuget/vpre/Serilog.AspNetCore.svg?style=flat)](https://www.nuget.org/packages/Serilog.AspNetCore/) 

Serilog logging for ASP.NET Core. This package routes ASP.NET Core log messages through Serilog, so you can get information about ASP.NET's internal operations written to the same Serilog sinks as your application events.

With _Serilog.AspNetCore_ installed and configured, you can write log messages directly through Serilog or any `ILogger` interface injected by ASP.NET. All loggers will use the same underlying implementation, levels, and destinations.

### Instructions

**First**, install the _Serilog.AspNetCore_ [NuGet package](https://www.nuget.org/packages/Serilog.AspNetCore) into your app. You will need a way to view the log messages - _Serilog.Sinks.Console_ writes these to the console; there are [many more sinks available](https://www.nuget.org/packages?q=Tags%3A%22serilog%22) on NuGet.

```shell
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
```

**Next**, in your application's _Program.cs_ file, configure Serilog first.  A `try`/`catch` block will ensure any configuration issues are appropriately logged:

```csharp
using Serilog;

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
            CreateWebHostBuilder(args).Build().Run();
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
    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSerilog(); // <-- Add this line;
}
```

**Finally**, clean up by removing the remaining configuration for the default logger:

 * Remove calls to `AddLogging()`
 * Remove the `"Logging"` section from _appsettings.json_ files (this can be replaced with [Serilog configuration](https://github.com/serilog/serilog-settings-configuration) as shown in [the _EarlyInitializationSample_ project](https://github.com/serilog/serilog-aspnetcore/blob/dev/samples/EarlyInitializationSample/Program.cs), if required)
 * Remove `ILoggerFactory` parameters and any `Add*()` calls on the logger factory in _Startup.cs_
 * Remove `UseApplicationInsights()` (this can be replaced with the [Serilog AI sink](https://github.com/serilog/serilog-sinks-applicationinsights), if required)

That's it! With the level bumped up a little you will see log output resembling:

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

A more complete example, showing _appsettings.json_ configuration, can be found in [the sample project here](https://github.com/serilog/serilog-aspnetcore/tree/dev/samples/EarlyInitializationSample).

### Request logging <sup>`3.0.0-*`</sup>

The package includes middleware for smarter HTTP request logging. The default request logging implemented by ASP.NET Core is noisy, with multiple events emitted per request. The included middleware condenses these into a single event that carries method, path, status code, and timing information.

As text, this has a format like:

```
[16:05:54 INF] HTTP GET / responded 200 in 227.3253 ms
```

Or [as JSON](https://github.com/serilog/serilog-formatting-compact):

```json
{
  "@t": "2019-06-26T06:05:54.6881162Z",
  "@mt": "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms",
  "@r": ["224.5185"],
  "RequestMethod": "GET",
  "RequestPath": "/",
  "StatusCode": 200,
  "Elapsed": 224.5185,
  "RequestId": "0HLNPVG1HI42T:00000001",
  "CorrelationId": null,
  "ConnectionId": "0HLNPVG1HI42T"
}
```

To enable the middleware, first change the minimum level for `Microsoft` to `Warning` in your logger configuration or _appsettings.json_ file:

```csharp
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
```

Then, in your application's _Startup.cs_, add the middleware with `UseSerilogRequestLogging()`:

```csharp
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseSerilogRequestLogging(); // <-- Add this line

            // Other app configuration
```

It's important that the `UseSerilogRequestLogging()` call appears _before_ handlers such as MVC. The middleware will not time or log components that appear before it in the pipeline. (This can be utilized to exclude noisy handlers from logging, such as `UseStaticFiles()`, by placing `UseSerilogRequestLogging()` after them.)

During request processing, additional properties can be attached to the completion event using `IDiagnosticContext.Set()`:

```csharp
    public class HomeController : Controller
    {
        readonly IDiagnosticContext _diagnosticContext;

        public HomeController(IDiagnosticContext diagnosticContext)
        {
            _diagnosticContext = diagnosticContext ?? throw new ArgumentNullException(nameof(diagnosticContext));
        }

        public IActionResult Index()
        {
            // The request completion event will carry this property
            _diagnosticContext.Set("CatalogLoadTime", 1423);

            return View();
        }
```

This pattern has the advantage of reducing the number of log events that need to be constructed, transmitted, and stored per HTTP request. Having many properties on the same event can also make correlation of request details and other data easier.

### Inline initialization

You can alternatively configure Serilog inline, in `BulidWebHost()`, using a delegate as shown below:

```csharp
    // dotnet add package Serilog.Settings.Configuration
    .UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
        .ReadFrom.Configuration(hostingContext.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console())
```

This has the advantage of making the `hostingContext`'s `Configuration` object available for configuration of the logger, but at the expense of recording `Exception`s raised earlier in program startup.

If this method is used, `Log.Logger` is assigned implicitly, and closed when the app is shut down.

A complete example, showing this approach, can be found in [the _InlineIntializationSample_ project](https://github.com/serilog/serilog-aspnetcore/tree/dev/samples/InlineInitializationSample).

### Enabling `Microsoft.Extensions.Logging.ILoggerProvider`s <sup>`3.0.0-*`</sup>

Serilog sends events to outputs called _sinks_, that implement Serilog's `ILogEventSink` interface, and are added to the logging pipeline using `WriteTo`. _Microsoft.Extensions.Logging_ has a similar concept called _providers_, and these implement `ILoggerProvider`. Providers are what the default logging configuration creates under the hood through methods like `AddConsole()`.

By default, Serilog ignores providers, since there are usually equivalent Serilog sinks available, and these work more efficiently with Serilog's pipeline. If provider support is needed, it can be optionally enabled.

**Using the recommended configuration:**

In the recommended configuration (in which startup exceptions are caught and logged), first create a `LoggerProviderCollection` in a static field in _Program.cs_:

```csharp
        // using Serilog.Extensions.Logging;
        static readonly LoggerProviderCollection Providers = new LoggerProviderCollection();
```

Next, add `WriteTo.Providers()` to the logger configuration:

```csharp
                .WriteTo.Providers(Providers)
```

Finally, pass the provider collection into `UseSerilog()`:

```csharp
                   .UseSerilog(providers: Providers)
```

Providers registered in _Startup.cs_ with `AddLogging()` will then receive events from Serilog.

**Using iniline initialization:**

If [inline initialization](#inline-initialization) is used, providers can be enabled by adding `writeToProviders: true` to the `UseSerilog()` method call:

```csharp
    .UseSerilog(
        (hostingContext, loggerConfiguration) => /* snip! */,
        writeToProviders: true)
```

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
