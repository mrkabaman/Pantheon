namespace BankingSolution.Api.Contracts.Response
{
    public class AccountBalanceResponse : ResponseBase
    {
        public string Name { get; }

        public AccountBalanceResponse(string accountId, decimal amount, string name) : base(accountId, amount)
        {
            Name = name;
        }
    }
}