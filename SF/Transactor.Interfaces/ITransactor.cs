using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting;

[assembly: FabricTransportActorRemotingProvider(RemotingListenerVersion = RemotingListenerVersion.V2_1, RemotingClientVersion = RemotingClientVersion.V2_1)]
namespace Transactor.Interfaces
{
    /// <summary>
    /// This interface defines the methods exposed by an actor.
    /// Clients use this interface to interact with the actor that implements it.
    /// </summary>
    public interface ITransactor : IActor
    {
        Task<bool> CheckAccountExists(string accountId, CancellationToken cancellationToken);

        Task<string> CreateAccountAsync(double initialBalance, CancellationToken cancellationToken);

        Task<bool> DeleteAccountAsync(string accountId, CancellationToken cancellationToken);

        Task<double> GetAccountBalanceAsync(string accountId, CancellationToken cancellationToken);

        Task<bool> TransferAsync(string fromAccountId, string toAccountId, double amount, CancellationToken cancellationToken);
    }
}
