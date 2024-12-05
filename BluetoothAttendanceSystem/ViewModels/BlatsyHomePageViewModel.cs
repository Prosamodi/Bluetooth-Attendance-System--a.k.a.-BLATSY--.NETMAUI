using BluetoothAttendanceSystem.Data;
using BluetoothAttendanceSystem.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BluetoothAttendanceSystem.ViewModels
{
    public partial class BlatsyHomePageViewModel : ObservableObject
    {
        
        private readonly IDatabaseRepository databaseRepository;

        [ObservableProperty]
        public ObservableCollection<StudentSubjectModel> students;

        [ObservableProperty]
        public StudentSubjectModel selectedClass;

        [ObservableProperty]
        public bool isRefreshing;


        public BlatsyHomePageViewModel(IDatabaseRepository databaseRepository)
        {
            this.databaseRepository = databaseRepository;

            LoadClass();
        }

        [RelayCommand]
        public void LoadClass()
        {
            try
            {
                IsRefreshing = true;
                var students = databaseRepository.ListStudents();
                if (students.Count > 0)
                {
                    var studentClass = databaseRepository.LoadClassAndSubjectAndSection();
                    Students = new ObservableCollection<StudentSubjectModel>(studentClass);
                }
                else
                {
                    Students = null;
                }
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        partial void OnSelectedClassChanged(StudentSubjectModel value)
        {
            if (value != null)
            {
                Dispatcher.GetForCurrentThread().Dispatch(() =>
                {
                    GotoBlatsyPage(value);
                });
                
            }
        }

        public async Task GotoBlatsyPage(StudentSubjectModel classInfo)
        {
            await Shell.Current.GoToAsync(
                "blatsy",
                new Dictionary<string, object>
                {
                    {"ClassInfo",  classInfo}
                });
        }
    }
}
