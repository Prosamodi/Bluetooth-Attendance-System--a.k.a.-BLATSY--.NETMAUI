using BluetoothAttendanceSystem.ViewModels;

namespace BluetoothAttendanceSystem.Pages;

public partial class ViewClassPage : ContentPage
{
	public ViewClassPage(ViewClassPageViewModel viewClassPageViewModel)
	{
		InitializeComponent();

		BindingContext = viewClassPageViewModel;
	}
}