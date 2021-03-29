using System;

namespace BankingSolution.Logic.Poco
{
    public class Transaction
    {
        public long Id { get; set; }
        public string AccountId { get; set; }
        public string Type { get; set; }
        
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        
        public DateTime Timestamp { get; set; }

        public override string ToString()
        {
            return $"[{nameof(Id)}: {Id}, {nameof(AccountId)}: {AccountId}]";
        }
    }
}