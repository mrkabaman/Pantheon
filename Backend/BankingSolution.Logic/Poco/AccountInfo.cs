using BankingSolution.Logic.ValueTypes;

namespace BankingSolution.Logic.Poco
{
    public record AccountInfo
    {
        public AccountInfo(string accountId, Amount amount)
        {
            AccountId = accountId;
            Amount = amount;
        }

        public string AccountId { get;  }
        public Amount Amount { get;  }

        public override string ToString()
        {
            return $"[{nameof(AccountId)}: {AccountId}, {nameof(Amount)}: {Amount}]";
        }
    }
}