using System.Windows;
using EVRangeCalculator.ViewModels; // Add this using directive

namespace EVRangeCalculator;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel(); // Set the DataContext
    }
}