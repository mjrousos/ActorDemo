using Account;
using ActorInterfaces;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;

namespace Actors
{
    internal class Account : Grain, IAccount, IRemindable
    {
        private const string AccountStateName = "AccountState";
        private const string ComputeInterestReminderName = "ComputeInterestReminder";

        private readonly IPersistentState<AccountState> _state;
        private readonly ILogger<Account> _logger;

        /// <summary>
        /// Initializes a new instance of Account
        /// </summary>
        public Account(
            [PersistentState(AccountStateName)]
            IPersistentState<AccountState> state, 
            ILogger<Account> logger)
        {
            _state = state;
            _logger = logger;
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Account actor {Id} activated.", this.GetGrainId().ToString());

            await this.RegisterOrUpdateReminder(ComputeInterestReminderName, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }

        public override async Task OnDeactivateAsync(DeactivationReason readson, CancellationToken cancellationToken)
        {
            // In the context of this demo, the reminder should stay registered even after activation
            // since interest should still be periodically accumulated even if the actor is idle for some time
            // and is deactivated!
            //
            // In order to demonstrate reminder unregistration, though, we'll unregister the reminder here.
            try
            {
                var computerInterestReminder = await this.GetReminder(ComputeInterestReminderName);

                if (computerInterestReminder != null)
                {
                    await this.UnregisterReminder(computerInterestReminder);
                }

            }
            catch (ReminderException) { }
        }

        public Task ReceiveReminder(string reminderName, TickStatus status)
        {
            _logger.LogInformation("Processing reminder {ReminderName}", reminderName);

            return reminderName switch
            {
                ComputeInterestReminderName => ComputeInterestAsync(),
                _ => throw new InvalidOperationException($"Unknown reminder: {reminderName}")
            };
        }

        private async Task ComputeInterestAsync()
        {
            // We could work with _state.State directly, but setting it to a local here allows the rest
            // of the method that previously used state from StateManager.GetStateAsync to remain unchanged.
            var state = _state.State;
            state.Balance *= 1 + state.InterestRate;
            await _state.WriteStateAsync();
        }

        public Task<double> GetBalanceAsync(GrainCancellationToken cancellationToken)
        {
            var state = _state.State;
            return Task.FromResult(state.Balance);
        }

        public async Task<double> WithdrawAsync(double amount, GrainCancellationToken cancellationToken)
        {
            if (amount < 0)
            {
                throw new ArgumentException("Amount must be non-negative", nameof(amount));
            }

            var state = _state.State;

            if (amount > state.Balance)
            {
                throw new InvalidOperationException($"Insufficient balance ({state.Balance}) for withdrawal ({amount})");
            }

            state.Balance -= amount;

            await _state.WriteStateAsync();

            return state.Balance;
        }

        public async Task<double> DepositAsync(double amount, GrainCancellationToken cancellationToken)
        {
            if (amount < 0)
            {
                throw new ArgumentException("Amount must be non-negative", nameof(amount));
            }

            var state = _state.State;

            state.Balance += amount;

            await _state.WriteStateAsync();

            return state.Balance;
        }

        public Task<double> GetInterestRateAsync(GrainCancellationToken cancellationToken)
        {
            var state = _state.State;
            return Task.FromResult(state.InterestRate);
        }

        public async Task SetInterestRateAsync(double rate, GrainCancellationToken cancellationToken)
        {
            if (rate < 0)
            {
                throw new ArgumentException("Rate must be non-negative", nameof(rate));
            }

            var state = _state.State;
            state.InterestRate = rate;
            await _state.WriteStateAsync();
        }

        public async Task DeleteAsync(GrainCancellationToken cancellationToken)
        {
            // Since there's no way to force persistent state deletion in Orleans, just have a method on the
            // grain to clear state or mark it as soft deleted/unused.
            await _state.ClearStateAsync();
        }
    }
}
