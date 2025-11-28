using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace Serilog.AspNetCore.Tests.Support;

// ReSharper disable once ClassNeverInstantiated.Global
public class TestEntryPoint;

// ReSharper disable once ClassNeverInstantiated.Global
public class SerilogWebApplicationFactory : WebApplicationFactory<TestEntryPoint>
{
    protected override IHostBuilder CreateHostBuilder()
    {
        return new HostBuilder();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseContentRoot(".");
    }
}
