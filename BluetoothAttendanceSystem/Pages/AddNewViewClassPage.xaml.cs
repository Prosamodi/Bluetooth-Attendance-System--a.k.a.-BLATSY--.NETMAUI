namespace BluetoothAttendanceSystem.Pages;
using BluetoothAttendanceSystem.ViewModels;

public partial class AddNewViewClassPage : ContentPage
{
	public AddNewViewClassPage(AddNewViewClassPageViewModel addNewViewClassPageViewModel)
	{
		InitializeComponent();

		BindingContext = addNewViewClassPageViewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		collectionView.SelectedItem = null;
	}
}