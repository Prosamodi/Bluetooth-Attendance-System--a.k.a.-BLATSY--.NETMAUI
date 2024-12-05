using BluetoothAttendanceSystem.ViewModels;

namespace BluetoothAttendanceSystem.Pages;

public partial class BlatsyPage : ContentPage
{
	public BlatsyPage(BlatsyPageViewModel blatsyPageViewModel)
	{
		InitializeComponent();

		BindingContext = blatsyPageViewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		studentCollectionView.SelectedItem = null;

    }
}