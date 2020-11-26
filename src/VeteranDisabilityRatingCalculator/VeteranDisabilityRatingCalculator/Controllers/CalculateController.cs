using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using VeteranDisabilityRatingCalculator.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace VeteranDisabilityRatingCalculator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalculateController : ControllerBase
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
                return BadRequest("You must supply at least one rating value");
            }

            string[] separatingChars = { ",", "|" };
            int[] ratingsArray = Array.ConvertAll(ratings.Split(separatingChars, StringSplitOptions.RemoveEmptyEntries), s => int.Parse(s));
            return Calculate(ratingsArray);
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
        public IActionResult Post([FromBody] int[] ratings)
        {
            return Calculate(ratings);
        }

        /// <summary>
        /// Calculation based on figures from here: https://www.benefits.va.gov/compensation/rates-index.asp
        /// </summary>
        /// <param name="ratings"></param>
        /// <returns></returns>
        private IActionResult Calculate(int[] ratings)
        {
            CalculationResult calculationResult = new CalculationResult();

            try
            {
                if (ratings != null && ratings.Count() > 0)
                {
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
                }
                else
                {
                    return BadRequest("You must supply at least one rating value");
                }
            }
            catch (Exception ex)
            {
                // TODO: log exception
                return BadRequest(String.Concat("There was an error calculating your rating. ", ex.Message));
            }

            return Ok(calculationResult);
        }
    }
}