using BluetoothAttendanceSystem.ViewModels;

namespace BluetoothAttendanceSystem.Pages;

public partial class BlatsyHomePage : ContentPage
{
	public BlatsyHomePage(BlatsyHomePageViewModel blatsyHomePageViewModel)
	{
		InitializeComponent();

		BindingContext = blatsyHomePageViewModel;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();

        collectionView.SelectedItem = null;
    }
}