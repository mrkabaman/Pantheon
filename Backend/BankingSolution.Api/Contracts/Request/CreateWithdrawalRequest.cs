using System.ComponentModel.DataAnnotations;

namespace BankingSolution.Api.Contracts.Request
{
    public class CreateWithdrawalRequest
    {
        [Required]
        public string AccountId { get; set; }
        
        [Required]
        [Range(1, (double)decimal.MaxValue, ErrorMessage = "Please enter a value bigger than {1}")]
        public decimal Amount { get; set; }

        public override string ToString()
        {
            return $"[{nameof(AccountId)}: {AccountId}, {nameof(Amount)}: {Amount}]";
        }
    }
}