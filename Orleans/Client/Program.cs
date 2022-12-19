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

Console.WriteLine("Transferring $60 from A to B");
var result = await transactor.TransferAsync(accountAId, accountBId, 60, cts.Token);
Console.WriteLine($"Transfer succeeded: {result}");

Console.WriteLine("Transferring $60 from A to B");
result = await transactor.TransferAsync(accountAId, accountBId, 60, cts.Token);
Console.WriteLine($"Transfer succeeded: {result}");

Console.WriteLine("Checking balances");
var aBalance = await transactor.GetAccountBalanceAsync(accountAId, cts.Token);
var bBalance = await transactor.GetAccountBalanceAsync(accountBId, cts.Token);
Console.WriteLine($"Balances - A: {aBalance}, B: {bBalance}");

Console.WriteLine("Waiting 130 seconds to test interest accumulation");
await Task.Delay(130 * 1000);

Console.WriteLine("Checking balances");
aBalance = await transactor.GetAccountBalanceAsync(accountAId, cts.Token);
bBalance = await transactor.GetAccountBalanceAsync(accountBId, cts.Token);
Console.WriteLine($"Balances - A: {aBalance}, B: {bBalance}");

Console.WriteLine("Deleting account B");
await transactor.DeleteAccountAsync(accountBId, cts.Token);

Console.WriteLine("Checking balances");
aBalance = await transactor.GetAccountBalanceAsync(accountAId, cts.Token);
bBalance = await transactor.GetAccountBalanceAsync(accountBId, cts.Token);
Console.WriteLine($"Balances - A: {aBalance}, B: {bBalance}");