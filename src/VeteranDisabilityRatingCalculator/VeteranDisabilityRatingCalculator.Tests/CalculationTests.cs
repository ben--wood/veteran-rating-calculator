using Microsoft.Extensions.DependencyInjection;
using Xunit;
using VeteranDisabilityRatingCalculator.Core.Models;
using VeteranDisabilityRatingCalculator.Core.Services;

namespace VeteranDisabilityRatingCalculator.Tests
{
    public class CalculationTests
    {
        private readonly ICalculationService _calculationService;

        public CalculationTests()
        {
            var services = new ServiceCollection();
            services.AddTransient<ICalculationService, CalculationService>();

            var serviceProvider = services.BuildServiceProvider();
            _calculationService = serviceProvider.GetService<ICalculationService>();
        }

        [Theory]
        [InlineData(new int[] { 0 }, 0)]
        [InlineData(new int[] { 100 }, 100)]
        [InlineData(new int[] { 110 }, 100)]
        [InlineData(new int[] { 60 }, 60)]
        [InlineData(new int[] { 40, 60 }, 76)]
        [InlineData(new int[] { 20, 40, 60 }, 81)]
        [InlineData(new int[] { 50, 30 }, 65)]
        [InlineData(new int[] { 40, 20 }, 52)]
        public void CumulativeDisabilityRating_CalculationTests(int[] ratings, int expectedCumulativeDisabilityRatingResult)
        {
            CalculationResult actualResult = _calculationService.Calculate(ratings);
            Assert.True(actualResult.CumulativeDisabilityRating == expectedCumulativeDisabilityRatingResult, $"Cumulative disability should be {expectedCumulativeDisabilityRatingResult}");
        }

        [Theory]
        [InlineData(new int[] { 0 }, 0)]
        [InlineData(new int[] { 100 }, 100)]
        [InlineData(new int[] { 110 }, 100)]
        [InlineData(new int[] { 60 }, 60)]
        [InlineData(new int[] { 40, 60 }, 80)]
        [InlineData(new int[] { 20, 40, 60 }, 80)]
        [InlineData(new int[] { 50, 30 }, 70)]
        [InlineData(new int[] { 40, 20 }, 50)]
        public void EstimatedDisabilityRating_CalculationTests(int[] ratings, int expectedEstimatedDisabilityRatingResult)
        {
            CalculationResult actualResult = _calculationService.Calculate(ratings);
            Assert.True(actualResult.EstimatedDisabilityRating == expectedEstimatedDisabilityRatingResult, $"Estimated disability should be {expectedEstimatedDisabilityRatingResult}");
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData(new int[] { }, false)]        
        [InlineData(new int[] { -20, 110 }, false)]        
        public void FailingDisabilityRating_CalculationTests(int[] ratings, bool expectedCalculationSuccess)
        {
            CalculationResult actualResult = _calculationService.Calculate(ratings);
            Assert.True(actualResult.CalculationSuccess == expectedCalculationSuccess, "Disability calculaton should fail");
        }
    }
}
