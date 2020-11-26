
namespace VeteranDisabilityRatingCalculator.Core.Models
{
    public class CalculationResult
    {
        public int CumulativeDisabilityRating { get; set; }
        public int EstimatedDisabilityRating { get; set; }
        public string ErrorMessage { get; set; }
    }
}