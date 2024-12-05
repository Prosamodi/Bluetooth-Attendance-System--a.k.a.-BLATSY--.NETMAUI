using BluetoothAttendanceSystem.Data;
using BluetoothAttendanceSystem.Models;
using BluetoothAttendanceSystem.ViewModels;

namespace BluetoothAttendanceSystem
{
    public partial class AppShell : Shell
    {
        private readonly IDatabaseRepository databaseRepository;
        public AppShell(AppShellViewModel appShellViewModel, IDatabaseRepository databaseRepository)
        {
            InitializeComponent();

            this.databaseRepository = databaseRepository;

            BindingContext = appShellViewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            var userInfo = new UserInformation
            {
                Firstname = "Sammy",
                Middlename = "Vicente",
                Surname = "Odi",
                Profession = "App Developer",
                Email = "pro.odisammyv@gmail.com"
            };

            databaseRepository.CreateUser(userInfo);
        }
    }
}
