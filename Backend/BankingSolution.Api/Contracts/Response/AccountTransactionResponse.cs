using System;

namespace BankingSolution.Api.Contracts.Response
{
    public class AccountTransactionResponse : ResponseBase
    {
        public string TransactionType { get; }
        public decimal Balance { get; }

        public DateTime Timestamp { get; }

        public AccountTransactionResponse(string accountId, decimal amount, string transactionType, decimal balance,
            DateTime timestamp) : base(accountId, amount)
        {
            TransactionType = transactionType;
            Balance = balance;
            Timestamp = timestamp;
        }
    }
}