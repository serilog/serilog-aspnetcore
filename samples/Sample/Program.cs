using Serilog;
using Serilog.Templates;

namespace Sample;

public static class Program
{
    public static int Main(string[] args)
    {
        // The initial "bootstrap" logger is able to log errors during start-up. It's completely replaced by the
        // logger configured in `UseSerilog()` below, once configuration and dependency-injection have both been
        // set up successfully.
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        Log.Information("Starting up!");

        try
        {
            CreateHostBuilder(args).Build().Run();

            Log.Information("Stopped cleanly");
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "An unhandled exception occurred during bootstrapping");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.Console(new ExpressionTemplate(
                    // Include trace and span ids when present.
                    "[{@t:HH:mm:ss} {@l:u3}{#if @tr is not null} ({@tr}:{@sp}){#end}] {@m}\n{@x}")))
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
}
