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

Console.WriteLine("Transferring $60 from A to B");
var result = await transactor.TransferAsync(accountAId, accountBId, 60, CancellationToken.None);
Console.WriteLine($"Transfer succeeded: {result}");

Console.WriteLine("Transferring $60 from A to B");
result = await transactor.TransferAsync(accountAId, accountBId, 60, CancellationToken.None);
Console.WriteLine($"Transfer succeeded: {result}");

Console.WriteLine("Checking balances");
var aBalance = await transactor.GetAccountBalanceAsync(accountAId, CancellationToken.None);
var bBalance = await transactor.GetAccountBalanceAsync(accountBId, CancellationToken.None);
Console.WriteLine($"Balances - A: {aBalance}, B: {bBalance}");

Console.WriteLine("Waiting 130 seconds to test interest accumulation");
await Task.Delay(130 * 1000);

Console.WriteLine("Checking balances");
aBalance = await transactor.GetAccountBalanceAsync(accountAId, CancellationToken.None);
bBalance = await transactor.GetAccountBalanceAsync(accountBId, CancellationToken.None);
Console.WriteLine($"Balances - A: {aBalance}, B: {bBalance}");