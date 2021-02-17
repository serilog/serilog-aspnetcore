using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Serilog.AspNetCore.Tests.Support
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SerilogWebApplicationFactory : WebApplicationFactory<TestStartup>
    {
        protected override IWebHostBuilder CreateWebHostBuilder() => new WebHostBuilder().UseStartup<TestStartup>();
        protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseContentRoot(".");
    }

    public class TestStartup { }
}
