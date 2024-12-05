using BluetoothAttendanceSystem.Data;
using BluetoothAttendanceSystem.Pages;
using BluetoothAttendanceSystem.ViewModels;
using BluetoothAttendanceSystem.Services;
using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui.Storage;

namespace BluetoothAttendanceSystem
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            builder.Services.AddTransient<AppShell>();
            builder.Services.AddTransient<AppShellViewModel>();

            AddPage<MainAppPage, MainAppPageViewModel>(builder.Services, "main");
            AddPage<BlatsyHomePage, BlatsyHomePageViewModel>(builder.Services, "blatsyhome");
            AddPage<BlatsyPage, BlatsyPageViewModel>(builder.Services, "blatsy");
            AddPage<AdministrationPage, AdministrationPageViewModel>(builder.Services, "administration");
            AddPage<AddNewViewClassPage, AddNewViewClassPageViewModel>(builder.Services, "addnewclass");
            AddPage<ViewClassPage, ViewClassPageViewModel>(builder.Services, "viewclass");
            AddPage<AttendanceHomePage, AttendanceHomePageViewModel>(builder.Services, "attendancehome");
            AddPage<AttendancePage, AttendancePageViewModel>(builder.Services, "attendance");

            builder.Services.AddSingleton(FileSystem.Current);
            builder.Services.AddSingleton(FileSaver.Default);
            builder.Services.AddSingleton(SemanticScreenReader.Default);
            builder.Services.AddSingleton(SecureStorage.Default);
            builder.Services.AddSingleton(Preferences.Default);
            builder.Services.AddSingleton<IDatabaseRepository, DatabaseRepository>();

            builder.Services.AddTransient<ImportFromExcelToDatabaseService>();
            builder.Services.AddTransient<ExportStudentAttendanceToExcelService>();
            
            return builder.Build();
        }

        private static IServiceCollection AddPage<TPage, TViewModel>(IServiceCollection services, string route) where TPage : Page where TViewModel : ObservableObject
        {
            services.AddTransient(typeof(TPage))
                .AddTransient(typeof(TViewModel));

            Routing.RegisterRoute(route, typeof(TPage));

            return services;
        }

    }

    
}
