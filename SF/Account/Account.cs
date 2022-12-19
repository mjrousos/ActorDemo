using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Account.Interfaces;
using System.Fabric;
using System.Threading;

namespace Account
{
    [StatePersistence(StatePersistence.Persisted)]
    internal class Account : Actor, IAccount, IRemindable
    {
        private const string AccountStateName = "AccountState";
        private const string ComputeInterestReminderName = "ComputeInterestReminder";

        /// <summary>
        /// Initializes a new instance of Account
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public Account(ActorService actorService, ActorId actorId) 
            : base(actorService, actorId)
        {
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override async Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Account actor {0} activated.", Id.ToString());

            var state = await StateManager.TryGetStateAsync<AccountState>(AccountStateName);
            if (!state.HasValue)
            {
                await StateManager.SetStateAsync(AccountStateName, new AccountState());
            }

            await RegisterReminderAsync(ComputeInterestReminderName, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }

        protected override async Task OnDeactivateAsync()
        {
            // In the context of this demo, the reminder should stay registered even after activation
            // since interest should still be periodically accumulated even if the actor is idle for some time
            // and is deactivated!
            //
            // In order to demonstrate reminder unregistration, though, we'll unregister the reminder here.
            try
            {
                var computerInterestReminder = GetReminder(ComputeInterestReminderName);

                if (computerInterestReminder != null)
                {
                    await UnregisterReminderAsync(computerInterestReminder);
                }

            }
            catch (FabricException) { }
        }

        public Task ReceiveReminderAsync(string reminderName, byte[] context, TimeSpan dueTime, TimeSpan period)
        {
            ActorEventSource.Current.ActorMessage(this, "Processing reminder {0}", reminderName);

            return reminderName switch
            {
                ComputeInterestReminderName => ComputeInterestAsync(),
                _ => throw new InvalidOperationException($"Unknown reminder: {reminderName}")
            };
        }

        private async Task ComputeInterestAsync()
        {
            var state = await StateManager.GetStateAsync<AccountState>(AccountStateName);
            state.Balance *= 1 + state.InterestRate;
            await StateManager.SetStateAsync<AccountState>(AccountStateName, state);
        }

        public async Task<double> GetBalanceAsync(CancellationToken cancellationToken)
        {
            var state = await StateManager.GetStateAsync<AccountState>(AccountStateName, cancellationToken);
            return state.Balance;
        }

        public async Task<double> WithdrawAsync(double amount, CancellationToken cancellationToken)
        {
            if (amount < 0)
            {
                throw new ArgumentException("Amount must be non-negative", nameof(amount));
            }

            var state = await StateManager.GetStateAsync<AccountState>(AccountStateName, cancellationToken);

            if (amount > state.Balance)
            {
                throw new InvalidOperationException($"Insufficient balance ({state.Balance}) for withdrawal ({amount})");
            }

            state.Balance -= amount;

            await StateManager.SetStateAsync(AccountStateName, state, cancellationToken);

            return state.Balance;
        }

        public async Task<double> DepositAsync(double amount, CancellationToken cancellationToken)
        {
            if (amount < 0)
            {
                throw new ArgumentException("Amount must be non-negative", nameof(amount));
            }

            var state = await StateManager.GetStateAsync<AccountState>(AccountStateName, cancellationToken);

            state.Balance += amount;

            await StateManager.SetStateAsync(AccountStateName, state, cancellationToken);

            return state.Balance;
        }

        public async Task<double> GetInterestRateAsync(CancellationToken cancellationToken)
        {
            var state = await StateManager.GetStateAsync<AccountState>(AccountStateName, cancellationToken);
            return state.InterestRate;
        }

        public async Task SetInterestRateAsync(double rate, CancellationToken cancellationToken)
        {
            if (rate < 0)
            {
                throw new ArgumentException("Rate must be non-negative", nameof(rate));
            }

            var state = await StateManager.GetStateAsync<AccountState>(AccountStateName, cancellationToken);
            state.InterestRate = rate;
            await StateManager.SetStateAsync(AccountStateName, state, cancellationToken);
        }
    }
}
