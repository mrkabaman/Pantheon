namespace BankingSolution.Api.Contracts.Response
{
    public class AccountCreateResponse : ResponseBase
    {
        public string Name { get; }

        public AccountCreateResponse(string accountId, decimal amount, string name) : base(accountId, amount)
        {
            Name = name;
        }
    }
}