using BluetoothAttendanceSystem.ViewModels;

namespace BluetoothAttendanceSystem.Pages;

public partial class AttendanceHomePage : ContentPage
{
	public AttendanceHomePage(AttendanceHomePageViewModel attendanceHomePageViewModel)
	{
		InitializeComponent();

		BindingContext = attendanceHomePageViewModel;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();

        attendanceCollectionView.SelectedItem = null;
    }
}