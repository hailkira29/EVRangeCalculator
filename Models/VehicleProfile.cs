using System.ComponentModel;
using EVRangeCalculator.ViewModels; // For ViewModelBase if INotifyPropertyChanged is needed directly here

namespace EVRangeCalculator.Models
{
    public class VehicleProfile : ViewModelBase // Inherit for easy property change notification if needed directly
    {
        private string _profileName = string.Empty;
        public string ProfileName
        {
            get => _profileName;
            set => SetProperty(ref _profileName, value);
        }

        private double _batteryCapacity;
        public double BatteryCapacity
        {
            get => _batteryCapacity;
            set => SetProperty(ref _batteryCapacity, value);
        }

        private double _defaultEfficiency;
        public double DefaultEfficiency
        {
            get => _defaultEfficiency;
            set => SetProperty(ref _defaultEfficiency, value);
        }

        // Constructor
        public VehicleProfile() { }

        public VehicleProfile(string name, double capacity, double efficiency)
        {
            ProfileName = name;
            BatteryCapacity = capacity;
            DefaultEfficiency = efficiency;
        }

        public override string ToString()
        {
            return ProfileName; // For display in ComboBox
        }
    }
}
