using BankingSolution.Logic.ValueTypes;

namespace BankingSolution.Logic.Poco
{
    public record CreateWithdrawal
    {
        public CreateWithdrawal(string accountId, Amount amount)
        {
            AccountId = accountId;
            Amount = amount;
        }

        public string AccountId { get;  }
        public Amount Amount { get;}

        public override string ToString()
        {
            return $"[{nameof(AccountId)}: {AccountId}, {nameof(Amount)}: {Amount}]";
        }
    }
}