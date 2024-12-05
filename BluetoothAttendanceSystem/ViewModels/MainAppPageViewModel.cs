using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BluetoothAttendanceSystem.ViewModels
{
    public partial class MainAppPageViewModel :  ObservableObject
    {
        string ProsamodiUrl = "https://github.com/Prosamodi";
        public MainAppPageViewModel()
        {
            
        }

        [ObservableProperty]
        public Color borderBackgroundColor = Colors.White;

        
        [RelayCommand]
        private async void GotoProsamodiGitHub()
        {
            if(await Launcher.CanOpenAsync(ProsamodiUrl))
            {
                borderBackgroundColor = Colors.Gray;

                await Task.Delay(100);

                await Launcher.OpenAsync(ProsamodiUrl);

                await Task.Delay(300);
                borderBackgroundColor = Colors.White;
            }
        }
    }
}
