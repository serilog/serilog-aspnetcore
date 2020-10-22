# Serilog.AspNetCore [![Build status](https://ci.appveyor.com/api/projects/status/4rscdto23ik6vm2r/branch/dev?svg=true)](https://ci.appveyor.com/project/serilog/serilog-aspnetcore/branch/dev) [![NuGet Version](http://img.shields.io/nuget/v/Serilog.AspNetCore.svg?style=flat)](https://www.nuget.org/packages/Serilog.AspNetCore/) [![NuGet Prerelease Version](http://img.shields.io/nuget/vpre/Serilog.AspNetCore.svg?style=flat)](https://www.nuget.org/packages/Serilog.AspNetCore/) 

Serilog logging for ASP.NET Core. This package routes ASP.NET Core log messages through Serilog, so you can get information about ASP.NET's internal operations written to the same Serilog sinks as your application events.

With _Serilog.AspNetCore_ installed and configured, you can write log messages directly through Serilog or any `ILogger` interface injected by ASP.NET. All loggers will use the same underlying implementation, levels, and destinations.

### Instructions

**First**, install the _Serilog.AspNetCore_ [NuGet package](https://www.nuget.org/packages/Serilog.AspNetCore) into your app.

```shell
dotnet add package Serilog.AspNetCore
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

 * Remove the `"Logging"` section from _appsettings.json_ files (this can be replaced with [Serilog configuration](https://github.com/serilog/serilog-settings-configuration) as shown in [the _EarlyInitializationSample_ project](https://github.com/serilog/serilog-aspnetcore/blob/dev/samples/EarlyInitializationSample/Program.cs), if required)
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

**Tip:** to see Serilog output in the Visual Studio output window when running under IIS, either select _ASP.NET Core Web Server_ from the _Show output from_ drop-down list, or replace `WriteTo.Console()` in the logger configuration with `WriteTo.Debug()`.

A more complete example, showing `appsettings.json` configuration, can be found in [the sample project here](https://github.com/serilog/serilog-aspnetcore/tree/dev/samples/EarlyInitializationSample).

### Request logging

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

To enable the middleware, first change the minimum level for `Microsoft.AspNetCore` to `Warning` in your logger configuration or _appsettings.json_ file:

```csharp
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
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
            _diagnosticContext = diagnosticContext ??
                throw new ArgumentNullException(nameof(diagnosticContext));
        }

        public IActionResult Index()
        {
            // The request completion event will carry this property
            _diagnosticContext.Set("CatalogLoadTime", 1423);

            return View();
        }
```

This pattern has the advantage of reducing the number of log events that need to be constructed, transmitted, and stored per HTTP request. Having many properties on the same event can also make correlation of request details and other data easier.

The following request information will be added as properties by default:

* `RequestMethod`
* `RequestPath`
* `StatusCode`
* `Elapsed`

You can modify the message template used for request completion events, add additional properties, or change the event level, using the `options` callback on `UseSerilogRequestLogging()`:

```csharp
app.UseSerilogRequestLogging(options =>
{
    // Customize the message template
    options.MessageTemplate = "Handled {RequestPath}";
    
    // Emit debug-level events instead of the defaults
    options.GetLevel = (httpContext, elapsed, ex) => LogEventLevel.Debug;
    
    // Attach additional properties to the request completion event
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
    };
});
```

### Inline initialization

You can alternatively configure Serilog inline, in `BuildWebHost()`, using a delegate as shown below:

```csharp
    .UseSerilog((hostingContext, services, loggerConfiguration) => loggerConfiguration
        .ReadFrom.Configuration(hostingContext.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console())
```

This has the advantage of making a service provider and the `hostingContext`'s `Configuration` object available for [configuration of the logger](https://github.com/serilog/serilog-settings-configuration), but at the expense of losing `Exception`s raised earlier in program startup.

If this method is used, `Log.Logger` is assigned implicitly, and closed when the app is shut down.

A complete example, showing this approach, can be found in [the _InlineIntializationSample_ project](https://github.com/serilog/serilog-aspnetcore/tree/dev/samples/InlineInitializationSample).

### Enabling `Microsoft.Extensions.Logging.ILoggerProvider`s

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

**Using inline initialization:**

If [inline initialization](#inline-initialization) is used, providers can be enabled by adding `writeToProviders: true` to the `UseSerilog()` method call:

```csharp
    .UseSerilog(
        (hostingContext, loggerConfiguration) => /* snip! */,
        writeToProviders: true)
```

### JSON output

The `Console()`, `Debug()`, and `File()` sinks all support JSON-formatted output natively, via the included _Serilog.Formatting.Compact_ package.

To write newline-delimited JSON, pass a `CompactJsonFormatter` or `RenderedCompactJsonFormatter` to the sink configuration method:

```csharp
    .WriteTo.Console(new RenderedCompactJsonFormatter())
```

### Writing to the Azure Diagnostics Log Stream

The Azure Diagnostic Log Stream ships events from any files in the `D:\home\LogFiles\` folder. To enable this for your app, add a file sink to your `LoggerConfiguration`, taking care to set the `shared` and `flushToDiskInterval` parameters:

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

### Pushing properties to the ILogger

If you want to add extra properties to all logevents in a specific part of your code, you can add them to the **ILogger** in **Microsoft.Extensions.Logging** with the following code. For this code to work, make sure you have added the `.Enrich.FromLogContext()` to the `.UseSerilog(...)` statement, as specified in the samples above.

```csharp
// Microsoft.Extensions.Logging ILogger
// Yes, it's required to use a dictionary. See https://nblumhardt.com/2016/11/ilogger-beginscope/
using (logger.BeginScope(new Dictionary<string, object>
{
    ["UserId"] = "xxx",
    ["ExtraProperty"] = "yyy",
}))
{
   // UserId and ExtraProperty are set for all logging events in these brackets
}
```

The code above results in the same outcome as if you would push propeties in the **ILogger** in Serilog.

```csharp
// Serilog ILogger
using (logger.PushProperty("UserId", "xxx"))
using (logger.PushProperty("ExtraProperty", "yyy"))
{
    // UserId and ExtraProperty are set for all logging events in these brackets
}
```
