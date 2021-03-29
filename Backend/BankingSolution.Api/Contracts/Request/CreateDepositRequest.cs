using System.ComponentModel.DataAnnotations;

namespace BankingSolution.Api.Contracts.Request
{
    public class CreateDepositRequest
    {
        [Required]
        public string AccountId { get; set; }
        
        [Required]
        [Range(1, (double)decimal.MaxValue, ErrorMessage = "Please enter a value bigger than {1}")]
        public decimal Amount { get; set; }
        
        
        [Required]
        [StringLength(3, ErrorMessage = "Currency must be a valid 3 character ISO code")]
        public string Currency { get; set; }

        public override string ToString()
        {
            return $"[{nameof(AccountId)}: {AccountId}, {nameof(Amount)}: {Amount}, {nameof(Currency)}: {Currency}]";
        }
    }
}