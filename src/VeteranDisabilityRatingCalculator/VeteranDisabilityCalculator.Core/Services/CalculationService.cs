using System;
using System.Linq;
using VeteranDisabilityRatingCalculator.Core.Models;

namespace VeteranDisabilityRatingCalculator.Core.Services
{
    public interface ICalculationService
    {
        CalculationResult Calculate(int[] ratings);
    }

    public class CalculationService : ICalculationService
    {
        /// <summary>
        /// Calculation based on figures from here: https://www.benefits.va.gov/compensation/rates-index.asp
        /// </summary>
        /// <param name="ratings"></param>
        /// <returns></returns>
        public CalculationResult Calculate(int[] ratings)
        {
            CalculationResult calculationResult = new CalculationResult();

            try
            {
                if (ratings == null || ratings.Count() == 0)
                {
                    calculationResult.ErrorMessage = "You must supply at least one rating value";
                    return calculationResult;
                }

                if (!ratings.All(x => x > 0))
                {
                    calculationResult.ErrorMessage = "Ratings must all be positive numbers";
                    return calculationResult;
                }
                 
                // order ratings from largest to smallest
                Array.Sort(ratings);
                Array.Reverse(ratings);

                // skip the highest rating
                int cumulativeDisabilityRating = ratings.First();
                for (int i = 1; i < ratings.Count(); i++)
                {
                    int r = ratings[i];

                    int previousCumulativeDisabilityRating = cumulativeDisabilityRating;

                    // rating as a percentage
                    decimal rating = ((decimal)r) / 100;

                    // disability ratings are not additive - calculation according to https://www.benefits.va.gov/compensation/rates-index.asp#combinedRatingsTable1
                    decimal disabilityCalculation = (decimal)((rating * (100 - previousCumulativeDisabilityRating)) + previousCumulativeDisabilityRating);

                    // round to the nearest integer
                    cumulativeDisabilityRating = (int)Math.Round(disabilityCalculation, 0, MidpointRounding.AwayFromZero);

                    if (cumulativeDisabilityRating >= 100)
                    {
                        break;
                    }
                }

                if (cumulativeDisabilityRating > 100)
                {
                    cumulativeDisabilityRating = 100;
                }

                calculationResult.CumulativeDisabilityRating = cumulativeDisabilityRating;

                // the Estimated Disability Rating is rounded to the nearest 10
                int remainder = calculationResult.CumulativeDisabilityRating % 10;
                if (remainder >= 5)
                {
                    calculationResult.EstimatedDisabilityRating = calculationResult.CumulativeDisabilityRating - remainder + 10;
                }
                else
                {
                    calculationResult.EstimatedDisabilityRating = calculationResult.CumulativeDisabilityRating - remainder;
                }

                calculationResult.CalculationSuccess = true;               
            }
            catch (Exception ex)
            {
                // TODO: log exception
                calculationResult.ErrorMessage = String.Concat("There was an error calculating your rating. ", ex.Message);
            }

            return calculationResult;
        }
    }
}