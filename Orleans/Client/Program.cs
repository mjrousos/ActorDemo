using ActorInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = new HostBuilder()
    .UseOrleansClient(client =>
    {
        client.UseLocalhostClustering();
    });

using var host  = builder.Build();
await host.StartAsync();

var client = host.Services.GetRequiredService<IClusterClient>();
var cts = new GrainCancellationTokenSource();

Console.WriteLine("Actor demo client started");

Console.WriteLine("Creating actor client");
var transactor = client.GetGrain<ITransactor>(0);

Console.WriteLine("Creating account A with $100");
var accountAId = await transactor.CreateAccountAsync(100, cts.Token);

Console.WriteLine("Creating account B with $100");
var accountBId = await transactor.CreateAccountAsync(100, cts.Token);

await CheckBalancesAsync(cts.Token);

Console.WriteLine("Transferring $60 from A to B");
var result = await transactor.TransferAsync(accountAId, accountBId, 60, cts.Token);
Console.WriteLine($"Transfer succeeded: {result}");

await CheckBalancesAsync(cts.Token);

Console.WriteLine("Transferring $60 from A to B");
result = await transactor.TransferAsync(accountAId, accountBId, 60, cts.Token);
Console.WriteLine($"Transfer succeeded: {result}");

await CheckBalancesAsync(cts.Token);

Console.WriteLine("Transferring $10 from A to non-existent account");
result = await transactor.TransferAsync(accountAId, "DoesNotExist", 10, cts.Token);
Console.WriteLine($"Transfer succeeded: {result}");

await CheckBalancesAsync(cts.Token);

Console.WriteLine("Waiting 130 seconds to test interest accumulation");
await Task.Delay(130 * 1000);

await CheckBalancesAsync(cts.Token);

Console.WriteLine("Deleting account B");
await transactor.DeleteAccountAsync(accountBId, cts.Token);

await CheckBalancesAsync(cts.Token);

async Task CheckBalancesAsync(GrainCancellationToken cancellationToken)
{
    Console.WriteLine("Checking balances");
    var aBalance = await transactor.GetAccountBalanceAsync(accountAId, cancellationToken);
    var bBalance = await transactor.GetAccountBalanceAsync(accountBId, cancellationToken);
    Console.WriteLine($"Balances - A: {aBalance}, B: {bBalance}");
}