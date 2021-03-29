using BankingSolution.Logic.ValueTypes;

namespace BankingSolution.Logic.Poco
{
    public abstract class ResultBase
    {
        public AccountStatus Status { get; }
        public string Message { get; }
        public Currency Currency { get; }
        public Amount Amount { get; }

        protected ResultBase(AccountStatus status, string message, Currency currency, Amount amount)
        {
            Status = status;
            Message = message;
            Currency = currency;
            Amount = amount;
        }

        public override string ToString()
        {
            return $"[{nameof(Status)}: {Status}, {nameof(Message)}: {Message}, {nameof(Currency)}: {Currency}, {nameof(Amount)}: {Amount}]";
        }
    }
}