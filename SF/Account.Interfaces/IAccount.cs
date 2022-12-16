using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting;

[assembly: FabricTransportActorRemotingProvider(RemotingListenerVersion = RemotingListenerVersion.V2_1, RemotingClientVersion = RemotingClientVersion.V2_1)]
namespace Account.Interfaces
{
    /// <summary>
    /// This interface defines the methods exposed by an actor.
    /// Clients use this interface to interact with the actor that implements it.
    /// </summary>
    public interface IAccount : IActor
    {
        Task<double> GetBalanceAsync(CancellationToken cancellationToken);

        Task<double> WithdrawAsync(double amount, CancellationToken cancellationToken);

        Task<double> DepositAsync(double amount, CancellationToken cancellationToken);

        Task<double> GetInterestRateAsync(CancellationToken cancellationToken);

        Task SetInterestRateAsync(double rate, CancellationToken cancellationToken);
    }
}
