using System.ComponentModel; // Added for IDataErrorInfo
using System.Windows.Input;
using EVRangeCalculator.Models;
using EVRangeCalculator.Commands;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Text;

namespace EVRangeCalculator.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IDataErrorInfo
    {
        private readonly EVCalculationModel _evCalculationModel;
        private string _batteryCapacityText = "75";
        private string _efficiencyText = "160";
        private string _distanceText = "300";
        private string _elevationGainText = "0";
        private string _drivingStyleFactorText = "1.0";
        private string _selectedWeatherCondition = "Ideal";
        private string _resultText = string.Empty;
        private VehicleProfile? _selectedVehicleProfile; // Made nullable
        private ObservableCollection<VehicleProfile> _vehicleProfiles;
        private string _startLocationText = string.Empty;
        private string _endLocationText = string.Empty;
        private bool _isElevationReadOnly = false;

        public static string[] WeatherConditions { get; } = { "Ideal", "Moderate", "Adverse" }; // Made static

        public ICommand CalculateRangeCommand { get; }
        public ICommand ResetCustomProfileCommand { get; }
        public ICommand FetchRouteCommand { get; }

        public MainWindowViewModel()
        {
            _evCalculationModel = new EVCalculationModel();
            _vehicleProfiles = new ObservableCollection<VehicleProfile> // Ensure _vehicleProfiles is initialized
            {
                new VehicleProfile { ProfileName = "Custom", BatteryCapacity = 75, DefaultEfficiency = 160 }, // Corrected to DefaultEfficiency
                new VehicleProfile { ProfileName = "Tesla Model 3 LR", BatteryCapacity = 75, DefaultEfficiency = 150 }, // Corrected to DefaultEfficiency
                new VehicleProfile { ProfileName = "Nissan Leaf", BatteryCapacity = 40, DefaultEfficiency = 160 }, // Corrected to DefaultEfficiency
                new VehicleProfile { ProfileName = "Chevy Bolt", BatteryCapacity = 65, DefaultEfficiency = 170 } // Corrected to DefaultEfficiency
            };
            // Initialize _selectedVehicleProfile, ensuring it's not null if possible, or handle null appropriately.
            _selectedVehicleProfile = VehicleProfiles.FirstOrDefault(p => p.ProfileName == "Custom") ?? VehicleProfiles.First();


            CalculateRangeCommand = new RelayCommand(CalculateRange, CanCalculateRange);
            ResetCustomProfileCommand = new RelayCommand(ResetCustomProfile);
            FetchRouteCommand = new RelayCommand(async () => await FetchRouteData(), CanFetchRoute);
        }

        public string BatteryCapacityText
        {
            get => _batteryCapacityText;
            set
            {
                _batteryCapacityText = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested(); // Corrected
                UpdateCustomProfile();
            }
        }

        public string EfficiencyText
        {
            get => _efficiencyText;
            set
            {
                _efficiencyText = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested(); // Corrected
                UpdateCustomProfile();
            }
        }

        public string DistanceText
        {
            get => _distanceText;
            set
            {
                _distanceText = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested(); // Corrected
                UpdateCustomProfile();
            }
        }

        public string ElevationGainText
        {
            get => _elevationGainText;
            set
            {
                _elevationGainText = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested(); // Corrected
                UpdateCustomProfile();
            }
        }

        public string DrivingStyleFactorText
        {
            get => _drivingStyleFactorText;
            set
            {
                _drivingStyleFactorText = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested(); // Corrected
            }
        }

        public string SelectedWeatherCondition
        {
            get => _selectedWeatherCondition;
            set
            {
                _selectedWeatherCondition = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested(); 
                // UpdateCustomProfile(); // Weather condition is not part of the vehicle profile itself
            }
        }

        public string ResultText
        {
            get => _resultText;
            set => SetProperty(ref _resultText, value);
        }

        public ObservableCollection<VehicleProfile> VehicleProfiles
        {
            get => _vehicleProfiles;
            set // This setter might not be strictly necessary if the collection is initialized once and modified internally.
            {
                _vehicleProfiles = value;
                OnPropertyChanged();
            }
        }

        public VehicleProfile? SelectedVehicleProfile // Made nullable
        {
            get => _selectedVehicleProfile;
            set
            {
                if (SetProperty(ref _selectedVehicleProfile, value) && _selectedVehicleProfile != null)
                {
                    // Update fields based on selected profile if it's not the "Custom" one or if behavior dictates
                    if (_selectedVehicleProfile.ProfileName != "Custom")
                    {
                        BatteryCapacityText = _selectedVehicleProfile.BatteryCapacity.ToString();
                        EfficiencyText = _selectedVehicleProfile.DefaultEfficiency.ToString(); // Corrected to DefaultEfficiency
                    }
                    // Do not reset Distance, Elevation, Weather, Driving Style on profile change
                    // as these are independent of the vehicle itself.
                    CommandManager.InvalidateRequerySuggested(); // Corrected
                }
            }
        }

        public string StartLocationText
        {
            get => _startLocationText;
            set
            {
                _startLocationText = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested(); // Corrected
            }
        }

        public string EndLocationText
        {
            get => _endLocationText;
            set
            {
                _endLocationText = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested(); // Corrected
            }
        }

        public bool IsElevationReadOnly
        {
            get => _isElevationReadOnly;
            set
            {
                _isElevationReadOnly = value;
                OnPropertyChanged();
            }
        }

        private void UpdateCustomProfile()
        {
            var customProfile = VehicleProfiles.FirstOrDefault(p => p.ProfileName == "Custom");
            if (customProfile != null)
            {
                customProfile.BatteryCapacity = double.TryParse(BatteryCapacityText, out var cap) ? cap : 0;
                customProfile.DefaultEfficiency = double.TryParse(EfficiencyText, out var eff) ? eff : 0; // Corrected to DefaultEfficiency
            }
            CommandManager.InvalidateRequerySuggested(); // Corrected
        }

        private bool CanCalculateRange()
        {
            return !string.IsNullOrWhiteSpace(BatteryCapacityText) && double.TryParse(BatteryCapacityText, out _)
                && !string.IsNullOrWhiteSpace(EfficiencyText) && double.TryParse(EfficiencyText, out _)
                && !string.IsNullOrWhiteSpace(DistanceText) && double.TryParse(DistanceText, out _)
                && !string.IsNullOrWhiteSpace(ElevationGainText) && double.TryParse(ElevationGainText, out _)
                && !string.IsNullOrWhiteSpace(DrivingStyleFactorText) && double.TryParse(DrivingStyleFactorText, out _)
                && SelectedVehicleProfile != null
                && !string.IsNullOrWhiteSpace(SelectedWeatherCondition)
                && string.IsNullOrEmpty(this[nameof(BatteryCapacityText)])
                && string.IsNullOrEmpty(this[nameof(EfficiencyText)])
                && string.IsNullOrEmpty(this[nameof(DistanceText)])
                && string.IsNullOrEmpty(this[nameof(ElevationGainText)])
                && string.IsNullOrEmpty(this[nameof(DrivingStyleFactorText)]);
        }

        private void CalculateRange()
        {
            if (!double.TryParse(BatteryCapacityText, out double batteryCapacity) || batteryCapacity <= 0)
            {
                ResultText = "Battery Capacity must be a positive number."; return;
            }
            if (!double.TryParse(EfficiencyText, out double efficiency) || efficiency <= 0)
            {
                ResultText = "Efficiency must be a positive number."; return;
            }

            double distance = 0;
            if (!string.IsNullOrWhiteSpace(DistanceText))
            {
                if (!double.TryParse(DistanceText, out distance) || distance < 0)
                {
                    ResultText = "Invalid Distance. Must be a non-negative number if provided."; return;
                }
            }

            double elevationGain = 0;
            if (!string.IsNullOrWhiteSpace(ElevationGainText))
            {
                if (!double.TryParse(ElevationGainText, out elevationGain))
                {
                    ResultText = "Invalid Elevation Gain. Must be a number if provided."; return;
                }
            }

            if (!double.TryParse(DrivingStyleFactorText, out double drivingStyleFactor) || drivingStyleFactor <= 0)
            {
                ResultText = "Driving Style Factor must be a positive number."; return;
            }

            _evCalculationModel.BatteryCapacity = batteryCapacity;
            _evCalculationModel.Efficiency = efficiency;
            _evCalculationModel.Distance = distance;
            _evCalculationModel.ElevationGain = elevationGain;
            _evCalculationModel.CurrentWeather = (WeatherCondition)Enum.Parse(typeof(WeatherCondition), SelectedWeatherCondition);
            _evCalculationModel.DrivingStyleFactor = drivingStyleFactor; // Add this line

            double calculatedRange = _evCalculationModel.CalculateRange();
            _evCalculationModel.CalculatedRange = calculatedRange;

            string result = $"Estimated Range: {calculatedRange:F2} km";

            if (distance > 0)
            {
                bool canComplete = _evCalculationModel.CanCompleteSpecifiedRoute(calculatedRange);
                result += $"\\nCan complete {distance:F2} km route: {(canComplete ? "Yes" : "No")}";
            }
            else
            {
                result += "\\n(Distance for route feasibility not provided)";            }
            
            ResultText = result;
        }

        private bool CanFetchRoute()
        {
            return !string.IsNullOrWhiteSpace(StartLocationText) && !string.IsNullOrWhiteSpace(EndLocationText)
                   && string.IsNullOrEmpty(this[nameof(StartLocationText)]) 
                   && string.IsNullOrEmpty(this[nameof(EndLocationText)]);
        }        private async Task FetchRouteData()
        {
            try
            {
                ResultText = "Initiating route data fetch...";
                IsElevationReadOnly = false;

                // Step 1: Geocode start location
                ResultText = "Finding start location coordinates...";
                var startCoords = await GeocodeLocationAsync(StartLocationText);
                if (startCoords == null)
                {
                    // GeocodeLocationAsync might have already set a more specific error,
                    // or we set a generic one if it returned null without throwing a handled exception.
                    if (!ResultText.Contains("Could not find coordinates") && !ResultText.Contains("error geocoding") && !ResultText.Contains("Timeout geocoding") && !ResultText.Contains("Location name cannot be empty") && !ResultText.Contains("No coordinates found for") && !ResultText.Contains("Could not parse coordinates for"))
                    {
                        ResultText = $"Could not find coordinates for: {StartLocationText}";
                    }
                    return;
                }

                // Step 2: Geocode end location
                ResultText = "Finding end location coordinates...";
                var endCoords = await GeocodeLocationAsync(EndLocationText);
                if (endCoords == null)
                {
                    if (!ResultText.Contains("Could not find coordinates") && !ResultText.Contains("error geocoding") && !ResultText.Contains("Timeout geocoding") && !ResultText.Contains("Location name cannot be empty") && !ResultText.Contains("No coordinates found for") && !ResultText.Contains("Could not parse coordinates for"))
                    {
                        ResultText = $"Could not find coordinates for: {EndLocationText}";
                    }
                    return;
                }

                // Step 3: Get route
                ResultText = $"Getting route from {startCoords.Value.lat:F4},{startCoords.Value.lon:F4} to {endCoords.Value.lat:F4},{endCoords.Value.lon:F4}...";
                
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30); // OSRM can also take time
                
                // Format coordinates using InvariantCulture to ensure '.' as decimal separator
                string lon1 = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:F6}", startCoords.Value.lon);
                string lat1 = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:F6}", startCoords.Value.lat);
                string lon2 = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:F6}", endCoords.Value.lon);
                string lat2 = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:F6}", endCoords.Value.lat);

                string routeUrl = $"http://router.project-osrm.org/route/v1/driving/{lon1},{lat1};{lon2},{lat2}?overview=false&alternatives=false&steps=false";
                
                ResultText = "Sending route request...";
                string routeResponse = await httpClient.GetStringAsync(routeUrl);
                
                var routeJson = JObject.Parse(routeResponse);
                string code = routeJson["code"]?.ToString() ?? "";
                
                if (code == "Ok")
                {
                    var routes = routeJson["routes"] as JArray;
                    if (routes?.Count > 0)
                    {
                        var route = routes[0];
                        double distanceMeters = route["distance"]?.Value<double>() ?? 0;
                        double durationSeconds = route["duration"]?.Value<double>() ?? 0;
                        
                        DistanceText = (distanceMeters / 1000).ToString("F1"); // Uses current culture for display, which is fine for UI
                        
                        var duration = TimeSpan.FromSeconds(durationSeconds);
                        ResultText = $"Route found! Distance: {(distanceMeters / 1000):F1} km, Duration: {duration:hh\\\\:mm\\\\:ss}. Please enter elevation manually.";
                    }
                    else
                    {
                        ResultText = "No route found between locations.";
                    }
                }
                else
                {
                    string message = routeJson["message"]?.ToString() ?? "Route calculation failed";
                    ResultText = $"Route error: {message}";
                }
            }
            catch (HttpRequestException httpEx)
            {
                ResultText = $"Network error: {httpEx.Message}";
            }
            catch (TaskCanceledException) // Catches timeout from HttpClient
            {
                ResultText = "Request timed out (30 seconds for route, 15 for geocoding).";
            }
            catch (Newtonsoft.Json.JsonException jsonEx) // Catch Json parsing errors specifically
            {
                ResultText = $"Error parsing API response: {jsonEx.Message}";
            }
            catch (Exception ex) // General catch-all
            {
                ResultText = $"Error: {ex.Message}";
            }
            finally
            {
                try
                {
                    CommandManager.InvalidateRequerySuggested();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in InvalidateRequerySuggested: {ex.Message}");
                    // Optionally, inform the user that UI command states might not be up-to-date
                    // ResultText += " (UI update issue)"; 
                }
            }
        }

        private async Task<(double lat, double lon)?> GeocodeLocationAsync(string locationName)
        {
            if (string.IsNullOrWhiteSpace(locationName))
            {
                ResultText = "Location name cannot be empty for geocoding.";
                return null;
            }

            try
            {
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(15); // Nominatim can be slow
                httpClient.DefaultRequestHeaders.Add("User-Agent", "EVRangeCalculator/1.0"); // Nominatim policy
                
                string encodedLocation = Uri.EscapeDataString(locationName);
                // Nominatim URL requires format=json and limit=1 for single best result
                string geocodeUrl = $"https://nominatim.openstreetmap.org/search?q={encodedLocation}&format=json&limit=1&addressdetails=0";
                
                // System.Diagnostics.Debug.WriteLine($"Geocoding URL for {locationName}: {geocodeUrl}");
                string responseString = await httpClient.GetStringAsync(geocodeUrl);
                // System.Diagnostics.Debug.WriteLine($"Geocoding response for {locationName}: {responseString}");

                var results = JArray.Parse(responseString); // Potential JsonReaderException
                
                if (results.Count > 0)
                {
                    var firstResult = results[0];
                    string? latStr = firstResult["lat"]?.ToString(); // Changed to string?
                    string? lonStr = firstResult["lon"]?.ToString(); // Changed to string?

                    // Use InvariantCulture for parsing doubles from API response
                    if (latStr != null && lonStr != null &&
                        double.TryParse(latStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double lat) &&
                        double.TryParse(lonStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double lon))
                    {
                        return (lat, lon);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to parse lat/lon from geocoding response for \'{locationName}\'. Lat: \'{latStr ?? "null"}\', Lon: \'{lonStr ?? "null"}\'. Response: {firstResult.ToString()}");
                        ResultText = $"Could not parse coordinates for {locationName}. Received Lat: '{latStr ?? "null"}', Lon: '{lonStr ?? "null"}'.";
                        return null; // Indicate parsing failure
                    }
                }
                System.Diagnostics.Debug.WriteLine($"No geocoding results for \'{locationName}\' from Nominatim.");
                ResultText = $"No coordinates found for {locationName}.";
                return null; // No results found
            }
            catch (HttpRequestException httpEx)
            {
                System.Diagnostics.Debug.WriteLine($"Geocoding HttpRequestException for \'{locationName}\': {httpEx.Message}");
                ResultText = $"Network error geocoding {locationName}: {httpEx.Message}";
                throw; // Re-throw to be caught by FetchRouteData's specific HttpRequestException handler
            }
            catch (TaskCanceledException taskEx) // Catches HttpClient timeout
            {
                System.Diagnostics.Debug.WriteLine($"Geocoding TaskCanceledException (timeout) for \'{locationName}\': {taskEx.Message}");
                ResultText = $"Timeout geocoding {locationName}.";
                throw; // Re-throw to be caught by FetchRouteData's specific TaskCanceledException handler
            }
            catch (Newtonsoft.Json.JsonException jsonEx) // Catch Json parsing errors
            {
                // Log the problematic JSON string if possible, be careful with large strings
                string responseInfo = jsonEx.Source ?? "N/A"; // Or try to get response string if available and not too large
                System.Diagnostics.Debug.WriteLine($"Geocoding JsonException for \'{locationName}\': {jsonEx.Message}. Response info: {responseInfo}");
                ResultText = $"Error parsing geocoding response for {locationName}. Details: {jsonEx.Message}";
                return null; // Treat as "could not find" or handle as a distinct error
            }
            catch (Exception ex) // Catch-all for other unexpected errors
            {
                System.Diagnostics.Debug.WriteLine($"Unexpected error in GeocodeLocationAsync for \'{locationName}\': {ex.ToString()}");
                ResultText = $"Unexpected error geocoding {locationName}. Details: {ex.Message}";
                return null; // Treat as "could not find" or let it propagate if critical
            }
        }


        private void ResetCustomProfile()
        {
            var customProfile = VehicleProfiles.FirstOrDefault(p => p.ProfileName == "Custom");
            if (customProfile != null)
            {
                customProfile.BatteryCapacity = 75; // Default
                customProfile.DefaultEfficiency = 160; // Default, Corrected to DefaultEfficiency
                
                // If SelectedVehicleProfile is the custom one, update the text boxes directly
                if (SelectedVehicleProfile == customProfile || SelectedVehicleProfile?.ProfileName == "Custom")
                {
                    BatteryCapacityText = customProfile.BatteryCapacity.ToString();
                    EfficiencyText = customProfile.DefaultEfficiency.ToString(); // Corrected to DefaultEfficiency
                }
            }
            // Reset other fields
            DistanceText = "300";
            ElevationGainText = "0";
            DrivingStyleFactorText = "1.0";
            SelectedWeatherCondition = WeatherConditions.First(); // Use static property
            StartLocationText = string.Empty;
            EndLocationText = string.Empty;
            ResultText = "Inputs reset to default values for Custom profile.";
            IsElevationReadOnly = false;
            OnPropertyChanged(string.Empty); // Refresh all bindings
            CommandManager.InvalidateRequerySuggested(); // Corrected
        }

        public string Error => string.Empty; // Not typically used

        public string this[string columnName]
        {
            get
            {
                string result = string.Empty;
                switch (columnName)
                {
                    case nameof(BatteryCapacityText):
                        if (string.IsNullOrWhiteSpace(BatteryCapacityText))
                            result = "Battery capacity is required.";
                        else if (!double.TryParse(BatteryCapacityText, out double cap) || cap <= 0)
                            result = "Battery capacity must be a positive number.";
                        break;

                    case nameof(EfficiencyText):
                        if (string.IsNullOrWhiteSpace(EfficiencyText))
                            result = "Efficiency is required.";
                        else if (!double.TryParse(EfficiencyText, out double eff) || eff <= 0)
                            result = "Efficiency must be a positive number (Wh/km).";
                        break;

                    case nameof(DistanceText):
                        if (!string.IsNullOrWhiteSpace(DistanceText) && (!double.TryParse(DistanceText, out double dist) || dist < 0)) // Changed <= to <
                            result = "Distance must be a non-negative number."; // Updated message
                        break;

                    case nameof(ElevationGainText):
                        // Ensure elev is declared before use in the condition
                        double elev;
                        if (!double.TryParse(ElevationGainText, out elev) || elev < 0) // Elevation can be 0
                            result = "Elevation gain must be a non-negative number.";
                        break;
                    case nameof(DrivingStyleFactorText):
                        if (string.IsNullOrWhiteSpace(DrivingStyleFactorText))
                            result = "Driving style factor is required.";
                        else if (!double.TryParse(DrivingStyleFactorText, out double factor) || factor <= 0)
                            result = "Driving style factor must be a positive number (e.g., 0.8 for eco, 1.2 for sporty).";
                        break;
                    // Removed SelectedVehicleProfile case as it's nullable and handled by UI enabling/disabling or direct checks.
                    // case nameof(SelectedVehicleProfile):
                    //     if (SelectedVehicleProfile == null)
                    //         result = "Please select a vehicle profile.";
                    //     break;
                    case nameof(SelectedWeatherCondition):
                        if (string.IsNullOrWhiteSpace(SelectedWeatherCondition))
                            result = "Please select a weather condition.";
                        break;
                    case nameof(StartLocationText):
                        // Validation for StartLocationText - only if trying to fetch route.
                        // This depends on how you want validation to behave.
                        // If it should always be valid unless a route fetch is attempted, this is complex for IDataErrorInfo.
                        // For now, simple non-empty check.
                        if (string.IsNullOrWhiteSpace(StartLocationText) && !string.IsNullOrWhiteSpace(EndLocationText)) // If end is filled, start should be too for a route
                             result = "Start location cannot be empty if End location is provided.";
                        break;
                    case nameof(EndLocationText):
                        if (string.IsNullOrWhiteSpace(EndLocationText) && !string.IsNullOrWhiteSpace(StartLocationText)) // If start is filled, end should be too for a route
                            result = "End location cannot be empty if Start location is provided.";
                        break;
                }
                return result;
            }
        }
    }
}
