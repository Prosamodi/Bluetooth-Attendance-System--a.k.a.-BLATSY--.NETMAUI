using BluetoothAttendanceSystem.Data;
using BluetoothAttendanceSystem.Models;
using BluetoothAttendanceSystem.Services;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BluetoothAttendanceSystem.ViewModels
{
    public partial class AddNewViewClassPageViewModel : ObservableObject
    {
        private readonly ISemanticScreenReader semanticScreenReader;
        public ImportFromExcelToDatabaseService importFromExcelToDatabaseService;
        private IDatabaseRepository databaseRepository;

        public ObservableCollection<Subjects> Subjects { get; } = new ObservableCollection<Subjects>();

        [ObservableProperty]
        public ObservableCollection<StudentSubjectModel> students;

        [ObservableProperty]
        public StudentSubjectModel selectedClass;

        [ObservableProperty]
        public bool isAddingNewClass;

        [ObservableProperty]
        public bool isButtonClickable = true;

        [ObservableProperty]
        public string addingStudent;

        [ObservableProperty]
        public string selectedSubject;

        [ObservableProperty]
        public string excelFullPath;

        [ObservableProperty]
        IList<string> allSubjects;

        [ObservableProperty]
        public bool isImportRunning;

        public AddNewViewClassPageViewModel(ImportFromExcelToDatabaseService importFromExcelToDatabaseService, 
            IDatabaseRepository databaseRepository, ISemanticScreenReader semanticScreenReader)
        {
            this.importFromExcelToDatabaseService = importFromExcelToDatabaseService;
            this.databaseRepository = databaseRepository;
            this.semanticScreenReader = semanticScreenReader;

            SaveClassCommand = new RelayCommand(SaveClassFromExcel, CanSaveStudentActivate);
            AddSubjectToExistingClassCommand = new RelayCommand(SaveNewClassSubject, CanSaveSubjectToExistingClassActivate);

            LoadSubjects();
            LoadClass();
        }

        [RelayCommand]
        public async Task PickExcel()
        {
            try
            {
                var customFileType = new FilePickerFileType(
               new Dictionary<DevicePlatform, IEnumerable<string>>
               {
                    { DevicePlatform.iOS, new[] { "public.xlsx" } },
                    { DevicePlatform.Android, new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" }},
                    { DevicePlatform.WinUI, new[] { ".xlsx" } }
               });

                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Please select an excel file.",
                    FileTypes = customFileType
                });

                if (result == null) {
                    return; 
                } else
                {
                    ExcelFullPath = result.FullPath.ToString();
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Result error: {ex}");
                return;
            }
              
        }
        public RelayCommand SaveClassCommand { get; }
        
        public async void SaveClassFromExcel()
        {
            
            var subjects = databaseRepository.ListSubjects();
            var selectedSubjectInfo = subjects.Where(x => x.Subject == SelectedSubject).FirstOrDefault();
            try
            {
                IsImportRunning = true;

                AddingStudent = "Adding Student Please wait...";

                await Dispatcher.GetForCurrentThread().DispatchAsync(async () =>
                {
                    if (SelectedSubject != null && ExcelFullPath is not null)
                    {
                        IsButtonClickable = false;

                        await importFromExcelToDatabaseService.ImportStudent(ExcelFullPath, selectedSubjectInfo.SubjectId);

                    }
                });

            } 
            catch(Exception ex)
            {
                ShowToastMessage($"There's an error in importing students. Error: {ex.Message}");
            }
            finally
            {
                ShowToastMessage("Successfully Added Students.");

                LoadClass();
                IsAddingNewClass = false;
                IsButtonClickable = true;
                IsImportRunning = false;
                AddingStudent = string.Empty;
                ExcelFullPath = string.Empty;
                SelectedSubject = string.Empty;
            }

        }

        public bool CanSaveStudentActivate()
        {
            if(!string.IsNullOrEmpty(SelectedSubject) && !string.IsNullOrEmpty(ExcelFullPath))
            {
                return true;
            } 
            else
            {
                return false;
            }
        }

        partial void OnSelectedSubjectChanged(string value) => SaveClassCommand.NotifyCanExecuteChanged();
        partial void OnExcelFullPathChanged(string value) => SaveClassCommand.NotifyCanExecuteChanged();


        [RelayCommand]
        public void AddingNewClassActive()
        {
            IsAddingNewClass = true;
        }

        [RelayCommand]
        public void AddingNewClassInActive()
        {
            IsAddingNewClass = false;
        }

        [RelayCommand]
        public void LoadSubjects()
        {
            var subjs = databaseRepository.ListSubjects();
            var allSubsName = subjs.Select(x => x.Subject).ToList();
            AllSubjects = allSubsName;
        }

        [RelayCommand]
        public void LoadClass()
        {
            var students = databaseRepository.ListStudents();
            if(students.Count > 0)
            {
                var studentClass = databaseRepository.LoadClassAndSubjectAndSection();
                Students = new ObservableCollection<StudentSubjectModel>(studentClass);
            } else
            {
                Students = null;
            }
        }


        [RelayCommand]
        public async void DeleteClass(StudentSubjectModel classInfo)
        {

            var subjectCount = databaseRepository.ListStudentSubject()
                .Where(s => s.ClassId == classInfo.ClassID)
                .Select(s => s.SubjectId).ToList().Distinct().Count();

            var subjectToDelete = databaseRepository.ListSubjects()
                .Where(s => s.SubjectId == classInfo.SubjectId).FirstOrDefault();

            var classToDelete = databaseRepository.ListStudents()
                .Where(x => x.ClassID == classInfo.ClassID).FirstOrDefault();

            bool isConfirmed = await Shell.Current.DisplayAlert("Confirm Delete",
                $"Are your sure you want to delete " +
                $"{classToDelete.CourseAbbreviation} {classToDelete.Year} - {classToDelete.Section} with" +
                $" {subjectToDelete.Subject} subject?",
                "Yes", "No");

            if (isConfirmed)
            {
                try
                {
                    if(subjectCount > 1)
                    {
                        databaseRepository.DeleteClassWithSubjectOnly(classInfo.ClassID, classInfo.SubjectId);
                        ShowToastMessage($"{classToDelete.CourseAbbreviation} {classToDelete.Year} - {classToDelete.Section} with" +
                            $"{subjectToDelete.Subject} Deleted Successfuly!");
                    }
                    else if(subjectCount == 1)
                    {
                        bool isConfirmed2 = await Shell.Current.DisplayAlert("Confirm Delete",
                            $"Are your sure you want to delete " +
                            $"{classToDelete.CourseAbbreviation} {classToDelete.Year} - {classToDelete.Section} with" +
                            $" {subjectToDelete.Subject} subject? This will delete the whole class?",
                            "Yes", "No");

                        if(isConfirmed2)
                        {
                            databaseRepository.DeleteClass(classInfo.ClassID, classInfo.SubjectId);
                            ShowToastMessage($"{classToDelete.CourseAbbreviation} {classToDelete.Year} - {classToDelete.Section} Deleted Successfuly!");
                        }
                    }
                }
                finally
                {
                    LoadClass();
                }
                
            }
        }

        partial void OnSelectedClassChanged(StudentSubjectModel value)
        {
            if(value != null)
            {
                Dispatcher.GetForCurrentThread().Dispatch(() =>
                {
                    GotoViewClassPage(value);
                });
            }
        }
         
        [RelayCommand]
        public async Task GotoViewClassPage(StudentSubjectModel classInfo)
        {
            await Shell.Current.GoToAsync(
                "viewclass",
                new Dictionary<string, object>
                {
                    {"ClassInfo",  classInfo}
                });
        }



        //Adding New Subject to Existing Class

        [ObservableProperty]
        public bool isAddingNewSubjectToClass;

        [ObservableProperty]
        public string courseAbb;

        [ObservableProperty]
        public string courseYear;

        [ObservableProperty]
        public string courseSection;

        [ObservableProperty]
        public string fullCourse;

        [ObservableProperty]
        public Guid classID;

        [ObservableProperty]
        public string selectedAddSubject;

        [RelayCommand]
        public void AddingNewSubjectToClassActive(StudentSubjectModel studentSubject)
        {
            IsAddingNewSubjectToClass = true;

            CourseAbb = studentSubject.CourseAbbreviation;
            CourseYear = studentSubject.Year;
            CourseSection = studentSubject.Section;
            FullCourse = studentSubject.FullCourseName;
            ClassID = studentSubject.ClassID;
        }

        [RelayCommand]
        public void AddingNewSubjectToClassInActive()
        {
            IsAddingNewSubjectToClass = false;

        }

        public RelayCommand AddSubjectToExistingClassCommand { get; }

        public void SaveNewClassSubject()
        {
            var subject = databaseRepository.ListSubjects()
                .Where(s => s.Subject == SelectedAddSubject).FirstOrDefault();

            databaseRepository.AddSubjectToExistingClass(ClassID, subject.SubjectId);

            ShowToastMessage($"Succesfully added {CourseAbb} {CourseYear} - {CourseSection} to {SelectedAddSubject} subject.");

            IsAddingNewSubjectToClass = false;

            LoadClass();
        }

        public bool CanSaveSubjectToExistingClassActivate()
        {
            if (!string.IsNullOrEmpty(SelectedAddSubject))
            {
                return true;
            } 
            else
            {
                return false;
            }
        }

        partial void OnSelectedAddSubjectChanged(string value) => AddSubjectToExistingClassCommand.NotifyCanExecuteChanged();
        
        //Public Toast Message
        public async void ShowToastMessage(string message)
        {
            var toast = Toast.Make(message, ToastDuration.Long, 14);
            await toast.Show();
        }

    }
}
