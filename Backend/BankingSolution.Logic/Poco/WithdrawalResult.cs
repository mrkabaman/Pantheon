using BankingSolution.Logic.ValueTypes;

namespace BankingSolution.Logic.Poco
{
    public class WithdrawalResult: ResultBase
    {
        public string AccountId { get; }
        public Amount OldBalance { get; }
        public Amount NewBalance { get; }
        public WithdrawalResult(string accountId,AccountStatus status, string message, Currency currency, Amount amount, Amount oldBalance, Amount newBalance) : base(status, message, currency, amount)
        {
            AccountId = accountId;
            OldBalance = oldBalance;
            NewBalance = newBalance;
        }
        }
}
    
  
