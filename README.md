# Electric Vehicle (EV) Range Calculator

A WPF application built with .NET 6 that calculates the estimated range of an electric vehicle based on battery capacity and efficiency.

## Features

- **Battery Capacity Input**: Enter the battery capacity in kWh
- **Efficiency Input**: Enter the vehicle's efficiency in Wh/km  
- **Distance Input**: Enter a planned route distance in km
- **Range Calculation**: Calculates the estimated driving range
- **Route Feasibility**: Checks if the vehicle can complete a specified route

## Getting Started

### Prerequisites

- .NET 6.0 or later
- Windows operating system
- Visual Studio 2022 (recommended) or Visual Studio Code with C# extension

### Building and Running

1. **Clone or download the project**
2. **Build the project**:
   ```powershell
   dotnet build EVRangeCalculator.csproj
   ```
3. **Run the application**:
   ```powershell
   dotnet run --project EVRangeCalculator.csproj
   ```

### How to Use

1. Launch the application
2. Enter the battery capacity in kWh (e.g., 75)
3. Enter the vehicle efficiency in Wh/km (e.g., 180)
4. Optionally enter a planned route distance in km
5. Click "Calculate Range" to see the results

## Formula

The range calculation uses the formula:
```
Range (km) = (Battery Capacity (kWh) × 1000) / Efficiency (Wh/km)
```

## Example

- Battery Capacity: 75 kWh
- Efficiency: 180 Wh/km
- Calculated Range: 416.67 km

## Project Structure

- `MainWindow.xaml` - UI layout and design
- `MainWindow.xaml.cs` - Application logic and calculations
- `App.xaml` - Application-level resources and configuration
- `EVRangeCalculator.csproj` - Project configuration file

## Future Enhancements (Stage 2 & Beyond)

- Route simulation with elevation and weather factors
- Multiple vehicle profiles
- Charging station integration
- Real-time traffic considerations
- Historical trip analysis

## Technical Notes

This is a WPF (.NET 6) project following C# and WPF best practices. The UI is defined in XAML and all calculations are implemented in the code-behind file.
