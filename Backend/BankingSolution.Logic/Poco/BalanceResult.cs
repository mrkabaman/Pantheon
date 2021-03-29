using BankingSolution.Logic.ValueTypes;

namespace BankingSolution.Logic.Poco
{
    public class BalanceResult: ResultBase
    {
        public string AccountId { get; }
        public string Name { get; }

        public BalanceResult(string accountId, string name, AccountStatus status, string message, Currency currency, Amount amount) : base(status, message, currency, amount)
        {
            AccountId = accountId;
            Name = name;
        }
    }
}