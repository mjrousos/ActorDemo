using System.Runtime.Serialization;

namespace Account
{
    [GenerateSerializer]
    internal class AccountState
    {
        [Id(0)]
        public bool Active { get; set; }

        [Id(1)]
        public double Balance { get; set; }

        [Id(2)]
        public double InterestRate { get; set; }
    }
}
