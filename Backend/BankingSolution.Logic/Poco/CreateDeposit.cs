using BankingSolution.Logic.ValueTypes;

namespace BankingSolution.Logic.Poco
{
    public record CreateDeposit
    {
        public CreateDeposit(string accountId, Amount amount, Currency currency)
        {
            AccountId = accountId;
            Amount = amount;
            Currency = currency;
        }
        public string AccountId { get; }
        public Amount Amount { get; }
        public Currency Currency { get; }

        public override string ToString()
        {
            return $"[{nameof(AccountId)}: {AccountId}, {nameof(Amount)}: {Amount}, {nameof(Currency)}: {Currency}]";
        }
    }
}