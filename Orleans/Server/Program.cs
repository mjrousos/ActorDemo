using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = new HostBuilder()
    .UseOrleans(silo =>
    {
        silo.UseLocalhostClustering()
            .AddMemoryGrainStorageAsDefault()
            .AddMemoryGrainStorage("AccountState")
            .UseInMemoryReminderService()
            .ConfigureLogging(ConfigureLogging);
    });

var host = builder.Build();
await host.StartAsync();

Console.WriteLine("Orleans host started.");
Console.WriteLine("Press enter to exit");
Console.ReadLine();

await host.StopAsync();

void ConfigureLogging(ILoggingBuilder builder)
{
    builder.AddConsole();
}