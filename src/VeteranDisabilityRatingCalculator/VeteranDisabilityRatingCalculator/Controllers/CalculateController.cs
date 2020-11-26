using System;
using Microsoft.AspNetCore.Mvc;
using VeteranDisabilityCalculator.Core.Services;

namespace VeteranDisabilityRatingCalculator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalculateController : ControllerBase
    {
        private readonly ICalculationService _calculationService;

        public CalculateController(ICalculationService calculationService)
        {
            _calculationService = calculationService;
        }

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

        private IActionResult Calculate(int[] ratings)
        {       
            var calculationResult = _calculationService.Calculate(ratings);

            if (!String.IsNullOrWhiteSpace(calculationResult.ErrorMessage))
            {
                return BadRequest(String.Concat("There was an error calculating your rating. ", calculationResult.ErrorMessage));
            }   
           
            return Ok(calculationResult);
        }
    }
}