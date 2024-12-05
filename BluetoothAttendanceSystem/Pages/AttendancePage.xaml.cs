using BluetoothAttendanceSystem.ViewModels;

namespace BluetoothAttendanceSystem.Pages;

public partial class AttendancePage : ContentPage
{
	public AttendancePage(AttendancePageViewModel attendancePageViewModel)
	{
		InitializeComponent();

		BindingContext = attendancePageViewModel;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();

        studentCollectionView.SelectedItem = null;

    }

    public async void OnDateSelected(object sender, DateChangedEventArgs e)
    {
        //await Shell.Current.DisplayAlert("Selected Date", $"{e.NewDate}", "OK");
        var attendanceViewModel = (AttendancePageViewModel)BindingContext;

        attendanceViewModel.NewSelectedDate = e.NewDate;
        await attendanceViewModel.LoadStudentsAttendance();
    }
}