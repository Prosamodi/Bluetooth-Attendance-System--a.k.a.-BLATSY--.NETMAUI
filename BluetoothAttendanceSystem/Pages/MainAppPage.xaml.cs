
using BluetoothAttendanceSystem.ViewModels;

namespace BluetoothAttendanceSystem.Pages;

public partial class MainAppPage : ContentPage
{
	public MainAppPage(MainAppPageViewModel mainAppPageViewModel)
	{
		InitializeComponent();

		BindingContext = mainAppPageViewModel;
	}

    
}