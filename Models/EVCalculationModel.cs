namespace EVRangeCalculator.Models
{
    /// <summary>
    /// Model representing EV calculation parameters and results
    /// </summary>
    public class EVCalculationModel
    {
        public double BatteryCapacity { get; set; }
        public double Efficiency { get; set; }
        public double Distance { get; set; }
        public double ElevationGain { get; set; } // Added for Stage 2
        public WeatherCondition CurrentWeather { get; set; } // Added for Stage 2
        public double DrivingStyleFactor { get; set; } = 1.0; // Added for Driving Style
        public double CalculatedRange { get; set; }
        public bool CanCompleteRoute { get; set; }
        public string ResultMessage { get; set; } = string.Empty;

        /// <summary>
        /// Calculates the EV range based on battery capacity and efficiency
        /// </summary>
        /// <returns>Range in kilometers</returns>
        public double CalculateRange()
        {
            if (Efficiency <= 0)
                return 0;

            double adjustedEfficiency = Efficiency;

            // Adjust for elevation: Reduce efficiency by 5% per 100m gain
            if (ElevationGain > 0)
            {
                // Corrected: Apply percentage reduction to the base efficiency, not subtract a fixed value
                adjustedEfficiency *= (1 - (ElevationGain / 100) * 0.05);
            }

            // Adjust for weather
            switch (CurrentWeather)
            {
                case WeatherCondition.Rainy:
                    adjustedEfficiency *= 0.90; // 10% reduction for rain
                    break;
                case WeatherCondition.Snowy:
                    adjustedEfficiency *= 0.80; // 20% reduction for snow
                    break;
                // No adjustment for Sunny/Clear
            }

            // Adjust for Driving Style Factor
            if (DrivingStyleFactor > 0) // Ensure factor is positive
            {
                adjustedEfficiency /= DrivingStyleFactor; // Higher factor means less efficient (e.g., sporty driving)
            }
            
            if (adjustedEfficiency <= 0) return 0; // Prevent division by zero or negative range

            return (BatteryCapacity * 1000) / adjustedEfficiency;
        }

        /// <summary>
        /// Determines if the vehicle can complete the specified route
        /// </summary>
        /// <param name="range">The calculated range</param>
        /// <returns>True if the route can be completed</returns>
        public bool CanCompleteSpecifiedRoute(double range)
        {
            return Distance > 0 && range >= Distance;
        }
    }

    // Added for Stage 2
    public enum WeatherCondition
    {
        Sunny,
        Rainy,
        Snowy
    }
}
