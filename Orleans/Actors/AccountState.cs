using System.Runtime.Serialization;

namespace Account
{
    [GenerateSerializer]
    internal class AccountState
    {
        public double Balance { get; set; }

        public double InterestRate { get; set; }
    }
}
