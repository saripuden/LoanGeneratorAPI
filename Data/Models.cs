using System.ComponentModel.DataAnnotations;

namespace LoanGeneratorAPI.Data
{
    public class LoanRequest
    {
        [Required]
        [Range(1000000, 100000000, ErrorMessage = "Plafond should be between 1 million and 100 million.")]
        public decimal Plafond { get; set; }

        [Required]
        [Range(1, 240, ErrorMessage = "Loan term should be between 1 and 240 months.")]

        public int TermMonths { get; set; }

        [Required]
        [Range(0.1, 100, ErrorMessage = "Interest rate should be between 0.1% and 100%.")]

        public decimal InterestRate { get; set; } // Annual Interest Rate in %

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
    }

    public class LoanResponse
    {
        public List<Installment> Installments { get; set; } = new List<Installment>();
    }

    public class Installment
    {
        public int InstallmentNumber { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalPayment { get; set; }
        public decimal PrincipalPayment { get; set; }
        public decimal InterestPayment { get; set; }
        public decimal RemainingPrincipal { get; set; }
    }
}
