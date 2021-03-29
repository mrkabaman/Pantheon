namespace BankingSolution.Api.Contracts.Response
{
    public class AccountWithdrawalResponse : ResponseBase
    {
        public decimal OldBalance { get; }
        public decimal NewBalance { get; }

        public AccountWithdrawalResponse(string accountId, decimal amount, decimal oldBalance, decimal newBalance) :
            base(accountId, amount)
        {
            OldBalance = oldBalance;
            NewBalance = newBalance;
        }
    }
}