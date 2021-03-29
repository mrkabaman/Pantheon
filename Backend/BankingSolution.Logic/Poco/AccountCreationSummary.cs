using BankingSolution.Logic.ValueTypes;

namespace BankingSolution.Logic.Poco
{
    public record AccountCreationSummary
    {
        public AccountCreationSummary(string name, string accountId, Amount amount)
        {
            Name = name;
            AccountId = accountId;
            Amount = amount;
            
        }
        public string Name { get; }
        public string AccountId { get; }
        public Amount Amount { get; }
    }
}