using System.ComponentModel.DataAnnotations;

namespace BankingSolution.Api.Contracts.Request
{
    public class CreateAccountRequest
    {
        public string Name { get; set; }
        
        [Required]
        [Range(1, (double)decimal.MaxValue, ErrorMessage = "Please enter a value bigger than {1}")]
        public decimal InitialDeposit { get; set; }

        public override string ToString()
        {
            return $"[{nameof(Name)}: {Name}, {nameof(InitialDeposit)}: {InitialDeposit}]";
        }
    }
}