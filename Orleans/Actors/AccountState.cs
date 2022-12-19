using System.Runtime.Serialization;

namespace Account
{
    [GenerateSerializer]
    internal class AccountState
    {
        [Id(0)]
        public double Balance { get; set; }

        [Id(1)]
        public double InterestRate { get; set; }
    }
}
