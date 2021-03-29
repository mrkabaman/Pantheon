namespace BankingSolution.Api.Contracts.Response
{
    public abstract class ResponseBase
    {
        public string AccountId { get; }
        public decimal Amount { get; }

        protected ResponseBase(string accountId, decimal amount)
        {
            AccountId = accountId;
            Amount = amount;
        }
    }
}