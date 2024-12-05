

namespace BluetoothAttendanceSystem
{
    public partial class App : Application
    {
        public App(AppShell appShell)
        {
            InitializeComponent();
            MainPage = appShell;
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);

            const int newWidth = 1000;
            const int newHeight = 600;

            //window.X = 500;
            //window.Y = 200;

            window.Width = newWidth;
            window.Height = newHeight;

            window.MinimumHeight = newHeight;
            window.MinimumWidth = newWidth;

            return window;
        }

        
    }
}
