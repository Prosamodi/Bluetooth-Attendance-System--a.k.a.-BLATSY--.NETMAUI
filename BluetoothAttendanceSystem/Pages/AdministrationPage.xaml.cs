using BluetoothAttendanceSystem.ViewModels;

namespace BluetoothAttendanceSystem.Pages;

public partial class AdministrationPage : ContentPage
{
    
    public AdministrationPage(AdministrationPageViewModel administrationPageViewModel)
	{
		InitializeComponent();
        BindingContext = administrationPageViewModel;
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        ((AdministrationPageViewModel)BindingContext).LoadUserInfo();
       
    }
}