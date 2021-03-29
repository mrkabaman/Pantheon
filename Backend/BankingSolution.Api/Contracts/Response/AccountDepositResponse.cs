namespace BankingSolution.Api.Contracts.Response
{
    public class AccountDepositResponse: ResponseBase
    {
        public string Currency { get; }
        
        public decimal OldBalance { get; }
        public decimal NewBalance { get; }
        public AccountDepositResponse(string accountId, decimal amount, string currency, decimal oldBalance, decimal newBalance) : base(accountId, amount)
        {
            Currency = currency;
            OldBalance = oldBalance;
            NewBalance = newBalance;
        }
    }
}