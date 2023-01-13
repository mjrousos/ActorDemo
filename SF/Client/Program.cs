using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Transactor.Interfaces;

const string ServiceUri = "fabric:/ActorDemo/TransactorService";

Console.WriteLine("Actor demo client started");

Console.WriteLine("Creating actor client");
var transactor = ActorProxy.Create<ITransactor>(ActorId.CreateRandom(), new Uri(ServiceUri));

Console.WriteLine("Creating account A with $100");
var accountAId = await transactor.CreateAccountAsync(100, CancellationToken.None);

Console.WriteLine("Creating account B with $100");
var accountBId = await transactor.CreateAccountAsync(100, CancellationToken.None);

await CheckBalancesAsync(CancellationToken.None);

Console.WriteLine("Transferring $60 from A to B");
var result = await transactor.TransferAsync(accountAId, accountBId, 60, CancellationToken.None);
Console.WriteLine($"Transfer succeeded: {result}");

await CheckBalancesAsync(CancellationToken.None);

Console.WriteLine("Transferring $60 from A to B");
result = await transactor.TransferAsync(accountAId, accountBId, 60, CancellationToken.None);
Console.WriteLine($"Transfer succeeded: {result}");

await CheckBalancesAsync(CancellationToken.None);

Console.WriteLine("Transferring $10 from A to non-existent account");
result = await transactor.TransferAsync(accountAId, "DoesNotExist", 10, CancellationToken.None);
Console.WriteLine($"Transfer succeeded: {result}");

await CheckBalancesAsync(CancellationToken.None);

Console.WriteLine("Waiting 130 seconds to test interest accumulation");
await Task.Delay(130 * 1000);

await CheckBalancesAsync(CancellationToken.None);

Console.WriteLine("Deleting account B");
await transactor.DeleteAccountAsync(accountBId, CancellationToken.None);

await CheckBalancesAsync(CancellationToken.None);

async Task CheckBalancesAsync(CancellationToken cancellationToken)
{
    Console.WriteLine("Checking balances");
    var aBalance = await transactor.GetAccountBalanceAsync(accountAId, cancellationToken);
    var bBalance = await transactor.GetAccountBalanceAsync(accountBId, cancellationToken);
    Console.WriteLine($"Balances - A: {aBalance}, B: {bBalance}");
}