namespace ActorInterfaces
{
    /// <summary>
    /// This interface defines the methods exposed by an actor.
    /// Clients use this interface to interact with the actor that implements it.
    /// </summary>
    public interface ITransactor : IGrainWithIntegerKey
    {
        Task<string> CreateAccountAsync(double initialBalance, GrainCancellationToken cancellationToken);

        Task<bool> DeleteAccountAsync(string accountId, GrainCancellationToken cancellationToken);

        Task<double> GetAccountBalanceAsync(string accountId, GrainCancellationToken cancellationToken);

        Task<bool> TransferAsync(string fromAccountId, string toAccountId, double amount, GrainCancellationToken cancellationToken);
    }
}
