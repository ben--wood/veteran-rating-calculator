using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using VeteranDisabilityRatingCalculator.Models;

namespace VeteranDisabilityRatingCalculator.Controllers
{
    [Produces("application/json")]
    [Route("api/Calculate")]
    public class CalculateController : Controller
    {
        /// <summary>
        /// Send a comma or pipe separated string of disability rating values, eg: 20,60,40 
        /// Individual rating values will be converted to integers so no decimal places please
        /// The return calculation will include a Cumulative Disability Rating - the raw figure calculated from the passed ratings values
        /// And an Estimated Disability Rating - the Cumulative Rating rounded to the nearest 10 which is what the VA use to calculate payments
        /// Calculation/figures are from here: https://www.benefits.va.gov/compensation/rates-index.asp
        /// </summary>
        /// <param name="ratings"></param>
        /// <returns>{"cumulativeDisabilityRating":n,"estimatedDisabilityRating":n}</returns>
        // GET api/calculate/20,40
        [HttpGet("{ratings}", Name = "Get")]
        public IActionResult Get(string ratings)
        {
            if (String.IsNullOrWhiteSpace(ratings))
            {
                return BadRequest();
            }

            string[] separatingChars = { ",", "|" };
            string[] ratingsArray = ratings.Split(separatingChars, StringSplitOptions.RemoveEmptyEntries);
            var ratingsList = new List<int>();

            for (int i = 0; i < ratingsArray.Length; i++)
            {
                int rating;
                if (int.TryParse(ratingsArray[i], out rating))
                {
                    ratingsList.Add(rating);
                }
            }

            return Ok(Calculate(ratingsList));
        }

        /// <summary>
        /// Post an array of ratings values and receive cumulative and estimated disability ratings in return  
        /// The return calculation will include a Cumulative Disability Rating - the raw figure calculated from the passed ratings values
        /// And an Estimated Disability Rating - the Cumulative Rating rounded to the nearest 10 which is what the VA use to calculate payments
        /// Calculation/figures are from here: https://www.benefits.va.gov/compensation/rates-index.asp
        /// </summary>
        /// <param name="ratings"></param>
        /// <returns>{"cumulativeDisabilityRating":n,"estimatedDisabilityRating":n}</returns>
        // POST api/calculate
        [HttpPost]
        public IActionResult Post([FromBody]int[] ratings)
        {
            if (ratings != null && ratings.Length > 0)
            {
                return Ok(Calculate(ratings.ToList()));
            }

            return BadRequest();
        }

        /// <summary>
        /// Calculation based on figures from here: https://www.benefits.va.gov/compensation/rates-index.asp
        /// </summary>
        /// <param name="ratings"></param>
        /// <returns></returns>
        private CalculationResult Calculate(List<int> ratings)
        {
            var calculationResult = new CalculationResult();

            try
            {
                if (ratings != null && ratings.Count > 0)
                {
                    // ratings need to be ordered from largest to smallest
                    ratings = ratings.OrderByDescending(x => x).ToList();

                    // skip the highest rating
                    var cumulativeDisabilityRating = ratings.First();
                    for (int i = 1; i < ratings.Count; i++)
                    {
                        var r = ratings[i];

                        var previousCumulativeDisabilityRating = cumulativeDisabilityRating;

                        var rating = ((decimal)r) / 100;
                        var disabilityCalculation = (decimal)(
                            (rating * (100 - previousCumulativeDisabilityRating)) + previousCumulativeDisabilityRating
                        );

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

                    // Estimated Disability Rating is rounded to the nearest 10
                    var remainder = calculationResult.CumulativeDisabilityRating % 10;
                    calculationResult.EstimatedDisabilityRating = remainder >= 5 ?
                        (calculationResult.CumulativeDisabilityRating - remainder + 10) :
                        (calculationResult.CumulativeDisabilityRating - remainder);
                }
            }
            catch (Exception ex)
            {
                // TODO: log exception
            }

            return calculationResult;
        }
    }
}
