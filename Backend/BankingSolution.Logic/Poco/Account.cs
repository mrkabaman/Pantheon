using BankingSolution.Logic.ValueTypes;

namespace BankingSolution.Logic.Poco
{
    public class Account
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Balance { get; set; }

        public override string ToString()
        {
            return $"[{nameof(Id)}: {Id}, {nameof(Name)}: {Name}, {nameof(Balance)}: {Balance}]";
        }
    }
}