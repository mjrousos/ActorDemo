namespace ActorInterfaces
{
    /// <summary>
    /// This interface defines the methods exposed by an actor.
    /// Clients use this interface to interact with the actor that implements it.
    /// </summary>
    public interface IAccount : IGrainWithStringKey
    {
        Task ActivateAsync(GrainCancellationToken cancellationToken);

        Task<bool> IsActive(GrainCancellationToken cancellationToken);

        Task<double> GetBalanceAsync(GrainCancellationToken cancellationToken);

        Task<double> WithdrawAsync(double amount, GrainCancellationToken cancellationToken);

        Task<double> DepositAsync(double amount, GrainCancellationToken cancellationToken);

        Task<double> GetInterestRateAsync(GrainCancellationToken cancellationToken);

        Task SetInterestRateAsync(double rate, GrainCancellationToken cancellationToken);

        Task<bool> DeleteAsync(GrainCancellationToken cancellationToken);
    }
}
