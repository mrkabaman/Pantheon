using BankingSolution.Logic.ValueTypes;

namespace BankingSolution.Logic.Poco
{
    public record CreateAccount
    {
        public CreateAccount(string name, Amount amount)
        {
            Name = name;
            Amount = amount;
        }

        public Amount Amount { get; }
        public string Name { get; }

        public override string ToString()
        {
            return $"[{nameof(Amount)}: {Amount}, {nameof(Name)}: {Name}]";
        }
    }
}