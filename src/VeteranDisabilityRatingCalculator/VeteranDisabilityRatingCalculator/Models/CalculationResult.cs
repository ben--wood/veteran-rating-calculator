using Microsoft.AspNetCore.Mvc;

namespace VeteranDisabilityRatingCalculator.Models
{
    public class CalculationResult : ActionResult
    {
        public int CumulativeDisabilityRating { get; set; }
        public int EstimatedDisabilityRating { get; set; }
    }
}
