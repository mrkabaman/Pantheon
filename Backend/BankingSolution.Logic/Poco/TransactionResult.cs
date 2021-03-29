using System.Collections.Generic;
using BankingSolution.Logic.ValueTypes;

namespace BankingSolution.Logic.Poco
{
    public class TransactionResult: ResultBase
    {
        public string AccountId { get; }
        public IEnumerable<Transaction> Transactions { get; }
        
        public TransactionResult(string accountId, IEnumerable<Transaction> transactions, AccountStatus status, string message, Currency currency, Amount amount) : base(status, message, currency, amount)
        {
            AccountId = accountId;
            Transactions = transactions;
            
        }
    }
}