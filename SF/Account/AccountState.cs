using System.Runtime.Serialization;

namespace Account
{
    [DataContract]
    internal class AccountState
    {
        [DataMember]
        public bool Active { get; set; }

        [DataMember]
        public double Balance { get; set; }

        [DataMember]
        public double InterestRate { get; set; }
    }
}
