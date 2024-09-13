using LoanGeneratorAPI.Data;
using LoanGeneratorAPI.Middleware;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace LoanGeneratorAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoanController : ControllerBase
    {
        private readonly ILogger<LoanController> _logger;

        public LoanController(ILogger<LoanController> logger)
        {
            _logger = logger;
        }

        [HttpPost("create-loan")]                                                                                                                                                                                                                                                                                                                                                                           
        [RequiresValidation]
        public IActionResult CreateLoan([FromBody] LoanRequest loanRequest)
        {
            try
            {
                // Log receiving data
                _logger.LogInformation("Received loan request at {Time}: {LoanRequest}", DateTime.UtcNow, loanRequest);

                var loanResponse = CalculateLoanSchedule(loanRequest);

                // Log after successful calculation
                _logger.LogInformation("Loan calculation successful for Plafond: {Plafond}, Term: {TermMonths} months at {Time}",
                    loanRequest.Plafond, loanRequest.TermMonths, DateTime.UtcNow);

                return Ok(loanResponse);

            }
            catch (Exception ex) 
            {
                // Log error
                _logger.LogError(ex, "Error processing loan request at {Time}", DateTime.UtcNow);
                return StatusCode(500, "Internal server error");
            }

           
        }

        private LoanResponse CalculateLoanSchedule(LoanRequest request)
        {
            var loanResponse = new LoanResponse();
            decimal remainingPrincipal = request.Plafond;
            decimal monthlyInterestRate = request.InterestRate / 100 / 12;
            decimal monthlyPayment = (request.Plafond * monthlyInterestRate) /
                (1 - (decimal)Math.Pow(1 + (double)monthlyInterestRate, -request.TermMonths));

            for (int i = 1; i <= request.TermMonths; i++)
            {
                decimal interestPayment = remainingPrincipal * monthlyInterestRate;
                decimal principalPayment = monthlyPayment - interestPayment;
                remainingPrincipal -= principalPayment;

                var installment = new Installment
                {
                    InstallmentNumber = i,
                    Date = request.StartDate.AddMonths(i),
                    TotalPayment = Math.Round(monthlyPayment,2),
                    PrincipalPayment = Math.Round(principalPayment),
                    InterestPayment = Math.Round(interestPayment),
                    RemainingPrincipal = remainingPrincipal < 0 ? 0 : Math.Round(remainingPrincipal)
                };
                
                loanResponse.Installments.Add(installment);
            }

            return loanResponse;
        }
    }
}
