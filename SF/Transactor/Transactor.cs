using Account.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Runtime;
using Transactor.Interfaces;

namespace Transactor
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.None)]
    internal class Transactor : Actor, ITransactor
    {
        private const double DefaultInterestRate = 0.05;

        /// <summary>
        /// Initializes a new instance of Transactor
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public Transactor(ActorService actorService, ActorId actorId) 
            : base(actorService, actorId)
        {
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Transactor actor {0} activated.", Id.ToString());

            return Task.CompletedTask;
        }

        public async Task<string> CreateAccountAsync(double initialBalance, CancellationToken cancellationToken)
        {
            var accountId = Guid.NewGuid().ToString();
            var account = ActorProxy.Create<IAccount>(new ActorId(accountId));

            try
            {
                await account.ActivateAsync(cancellationToken);
                await account.DepositAsync(initialBalance, cancellationToken);
                await account.SetInterestRateAsync(DefaultInterestRate, cancellationToken);
            }
            catch (Exception) 
            { 
                await DeleteAccountAsync(accountId, cancellationToken);
                throw;
            }
            
            return accountId;
        }

        public async Task<bool> DeleteAccountAsync(string accountId, CancellationToken cancellationToken)
        {
            var id = new ActorId(accountId);
            var account = ActorProxy.Create<IAccount>(id);
            var exists = await account.IsActive(cancellationToken);
            var accountService = ActorServiceProxy.Create(account.GetActorReference().ServiceUri, id);

            await accountService.DeleteActorAsync(id, cancellationToken);

            return exists;
        }

        public async Task<double> GetAccountBalanceAsync(string accountId, CancellationToken cancellationToken)
        {
            var account = ActorProxy.Create<IAccount>(new ActorId(accountId));

            return await account.GetBalanceAsync(cancellationToken);
        }

        public async Task<bool> TransferAsync(string fromAccountId, string toAccountId, double amount, CancellationToken cancellationToken)
        {
            var fromAccount = ActorProxy.Create<IAccount>(new ActorId(fromAccountId));
            var toAccount = ActorProxy.Create<IAccount>(new ActorId(toAccountId));

            try
            {
                await fromAccount.WithdrawAsync(amount, cancellationToken);

            }
            catch
            {
                ActorEventSource.Current.ActorMessage(this, "Transfer failed withdrawing funds. From: {0}, To: {1}, Amount: {2}", fromAccountId, toAccountId, amount);

                return false;
            }

            try
            {
                await toAccount.DepositAsync(amount, cancellationToken);
            }
            catch
            {
                await fromAccount.DepositAsync(amount, cancellationToken);

                ActorEventSource.Current.ActorMessage(this, "Transfer failed depositing funds; rolling back. From: {0}, To: {1}, Amount: {2}", fromAccountId, toAccountId, amount);

                return false;
            }

            ActorEventSource.Current.ActorMessage(this, "Transfer successful. From: {0}, To: {1}, Amount: {2}", fromAccountId, toAccountId, amount);

            return true;
        }

        public async Task<bool> CheckAccountExists(string accountId, CancellationToken cancellationToken)
        {
            var account = ActorProxy.Create<IAccount>(new ActorId(accountId));
            return await account.IsActive(cancellationToken);
        }
    }
}
