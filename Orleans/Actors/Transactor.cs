using Account;
using ActorInterfaces;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actors
{
    [StatelessWorker]
    internal class Transactor: Grain, ITransactor
    {
        private const double DefaultInterestRate = 0.05;
        private readonly ILogger<Transactor> _logger;

        /// <summary>
        /// Initializes a new instance of Transactor
        /// </summary>
        public Transactor(ILogger<Transactor> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Transactor actor {Id} activated.", this.GetGrainId().ToString());

            return Task.CompletedTask;
        }

        public async Task<string> CreateAccountAsync(double initialBalance, GrainCancellationToken cancellationToken)
        {
            var accountId = Guid.NewGuid().ToString();
            var account = GrainFactory.GetGrain<IAccount>(accountId);

            try
            {
                await account.DepositAsync(initialBalance, cancellationToken);
                await account.SetInterestRateAsync(DefaultInterestRate, cancellationToken);
            }
            catch (Exception)
            {
                await DeleteAccountAsync(accountId, cancellationToken);
                throw;
            }

            return accountId.ToString();
        }

        public async Task<bool> DeleteAccountAsync(string accountId, GrainCancellationToken cancellationToken)
        {
            var account = GrainFactory.GetGrain<IAccount>(accountId);
            await account.DeleteAsync(cancellationToken);

            // TODO - Have a mechanism for checking whether the account exists
            return true;
        }

        public async Task<double> GetAccountBalanceAsync(string accountId, GrainCancellationToken cancellationToken)
        {
            var account = GrainFactory.GetGrain<IAccount>(accountId);

            return await account.GetBalanceAsync(cancellationToken);
        }

        public async Task<bool> TransferAsync(string fromAccountId, string toAccountId, double amount, GrainCancellationToken cancellationToken)
        {
            var fromAccount = GrainFactory.GetGrain<IAccount>(fromAccountId);
            var toAccount = GrainFactory.GetGrain<IAccount>(toAccountId);

            // TODO - Have a way of distringuishing between accounts with zero balance and those that don't exist
            // (since the actors are automatically created). Should probably have some ID or boolean that is set
            // on CreateAccountAsync calls to indicate that the account *actually* exists. Otherwise, users could
            // accidentally transfer money to non-existent accounts. With this check in place, they could still
            // transfer to the wrong account but not a non-existent one.
            try
            {
                await fromAccount.WithdrawAsync(amount, cancellationToken);

            }
            catch
            {
                _logger.LogWarning("Transfer failed withdrawing funds. From: {FromAccoundId}, To: {ToAccountId}, Amount: {Amount}", fromAccountId, toAccountId, amount);

                return false;
            }

            try
            {
                await toAccount.DepositAsync(amount, cancellationToken);
            }
            catch
            {
                // This shouldn't actually fail. But for example purposes, if it did (maybe the
                // toAccount was deleted mid-transfer), we would roll back the withdrawal
                // from the fromAccount.
                await fromAccount.DepositAsync(amount, cancellationToken);

                _logger.LogWarning("Transfer failed depositing funds; rolling back. From: {FromAccountId}, To: {ToAccountId}, Amount: {Amount}", fromAccountId, toAccountId, amount);

                return false;
            }

            _logger.LogInformation("Transfer successful. From: {FromAccountId}, To: {ToAccountId}, Amount: {Amount}", fromAccountId, toAccountId, amount);

            return true;
        }
    }
}
