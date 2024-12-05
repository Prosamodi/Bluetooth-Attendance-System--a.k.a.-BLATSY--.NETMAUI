using BluetoothAttendanceSystem.Data;
using BluetoothAttendanceSystem.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Storage;

namespace BluetoothAttendanceSystem.ViewModels
{
    public partial class AdministrationPageViewModel : ObservableObject
    {
        private readonly ISemanticScreenReader semanticScreenReader;
        private readonly IDatabaseRepository databaseRepository;
        public ObservableCollection<UserInformation> User { get; } = new ObservableCollection<UserInformation>();
        public ObservableCollection<Subjects> Subjects { get; } = new ObservableCollection<Subjects>();

        [ObservableProperty]
        public string firstName;

        [ObservableProperty]
        public string middleName;

        [ObservableProperty]
        public string surName;

        [ObservableProperty]
        public string profession;

        [ObservableProperty]
        public string email;

        [ObservableProperty]
        public bool isEditingUserInfo;

        [RelayCommand]
        public void EditUserInfoActive()
        {
            IsEditingUserInfo = true;
        }

        [RelayCommand]
        public void EditUserInfoInActive()
        {
            LoadUserInfo();
            IsEditingUserInfo = false;
            
        }
        public string FullName => $"{surName}, {firstName} {middleName}";
        public AdministrationPageViewModel(ISemanticScreenReader semanticScreenReader, IDatabaseRepository databaseRepository)
        {
            this.semanticScreenReader = semanticScreenReader;
            this.databaseRepository = databaseRepository;

            ISaveUserInfoCommand = new RelayCommand(SaveUserInfo, CanSaveActivate);
            SaveNewSubjectCommand = new RelayCommand(SaveNewSubject, CanSaveSubjectActivate);
            LoadSubjects();
            LoadUserInfo();
        }

        public bool CanSaveActivate()
        {
            if (!string.IsNullOrEmpty(FirstName) &&
                !string.IsNullOrEmpty(SurName) &&
                !string.IsNullOrEmpty(Profession) &&
                !string.IsNullOrEmpty(Email) &&
                IsValidEmail(Email))
            {
                return true;
            }
            else { return false; }

        }
        private bool IsValidEmail(string email)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        public RelayCommand ISaveUserInfoCommand { get; }
        public  void SaveUserInfo()
        {

            var UserInfo = new UserInformation
            {
                Firstname = FirstName,
                Middlename = MiddleName,
                Surname = SurName,
                Profession = Profession,
                Email = Email
            };

            var userInfos = databaseRepository.GetUsers();

            if (userInfos.Count == 0)
            {
                databaseRepository.CreateUser(UserInfo);
                ShowToastMessage("User Information Updated Successfully.");
            }
            else
            {
                var userInfo = userInfos.Where(user => user.UserId == 1).FirstOrDefault();

                userInfo.Firstname = FirstName;
                userInfo.Middlename = MiddleName;
                userInfo.Surname = SurName;
                userInfo.Email = Email;
                userInfo.Profession = Profession;

                databaseRepository.UpdateUserInfo(userInfo);
                ShowToastMessage("User Information Updated Succesfully.");
            }


            IsEditingUserInfo = false;
        }

        partial void OnFirstNameChanged(string value) => ISaveUserInfoCommand.NotifyCanExecuteChanged();
        partial void OnSurNameChanged(string value) => ISaveUserInfoCommand.NotifyCanExecuteChanged();
        partial void OnProfessionChanged(string value) => ISaveUserInfoCommand.NotifyCanExecuteChanged();
        partial void OnEmailChanged(string value) => ISaveUserInfoCommand.NotifyCanExecuteChanged();

        public void LoadUserInfo()
        {

            var userInfos = databaseRepository.GetUsers();

            if (userInfos.Count != 0)
            {
                var userInfo = userInfos.Where(user => user.UserId == 1).FirstOrDefault();

                FirstName = userInfo.Firstname;
                MiddleName = userInfo.Middlename;
                SurName = userInfo.Surname;
                Profession = userInfo.Profession;
                Email = userInfo.Email;
            }
        }


        //This Section is for Adding And Editing Subject

        [ObservableProperty]
        public string subject;

        [ObservableProperty]
        public string subjectCode;

        [ObservableProperty]
        public string addingOrEditSubject;

        [ObservableProperty]
        public bool isAddingNewSubject;

        [ObservableProperty]
        public bool isEditingSubject;

        public int IdToEdit { get; set; }

        [RelayCommand]
        public void AddingNewSubjectActive()
        {
            IsAddingNewSubject = true;
            AddingOrEditSubject = "Adding Subject";
            Subject = string.Empty;
            SubjectCode = string.Empty;
        }

        [RelayCommand]
        public void AddingNewSubjectInActive()
        {
            IsAddingNewSubject = false;
        }

        [RelayCommand]
        public void EditSubjectActive(int id) 
        {
            IsEditingSubject = true;
            IsAddingNewSubject = true;

            AddingOrEditSubject = "Edit Subject";

            var subjects = databaseRepository.ListSubjects();

            var subjectToEdit = subjects.Where(i => i.SubjectId == id).FirstOrDefault();

            Subject = subjectToEdit.Subject;
            SubjectCode = subjectToEdit.SubjectCode;
            IdToEdit = id;

        }

        public bool CanSaveSubjectActivate()
        {
            if (!string.IsNullOrEmpty(Subject) && !string.IsNullOrEmpty(SubjectCode))
            {
                return true;
            }
            else { return false; }

        }

        partial void OnSubjectChanged(string value) => SaveNewSubjectCommand.NotifyCanExecuteChanged();
        partial void OnSubjectCodeChanged(string value) => SaveNewSubjectCommand.NotifyCanExecuteChanged();

        public RelayCommand SaveNewSubjectCommand { get; }
        public void SaveNewSubject()
        {
            if (!IsEditingSubject)
            {
                var NewSubject = new Subjects
                {
                    Subject = Subject,
                    SubjectCode = SubjectCode,
                    UpdatedAt = DateTime.Now
                };

                databaseRepository.CreateSubject(NewSubject);

                IsAddingNewSubject = false;

                LoadSubjects();

                ShowToastMessage($"{Subject} Added Successfully!");
            }
            else
            {
                var subjects = databaseRepository.ListSubjects();

                var subjectToEdit = subjects.Where(i => i.SubjectId == IdToEdit).FirstOrDefault();

                subjectToEdit.Subject = Subject;
                subjectToEdit.SubjectCode = SubjectCode;

                databaseRepository.UpdateSubject(subjectToEdit);

                IsEditingSubject = false;
                IsAddingNewSubject= false;

                LoadSubjects();

                ShowToastMessage($"{subjectToEdit.Subject} Updated Successfully!");
            }
            
        }

        [RelayCommand]
        public void LoadSubjects()
        {
            Subjects.Clear();

            var subjectsList = databaseRepository.ListSubjects();
            if (subjectsList != null)
            {
                foreach(var subject in subjectsList) 
                {
                    Subjects.Add(subject);
                }
            }    
        }

        [RelayCommand]
        public async void DeleteSubject(int id)
        {
            var subjects = databaseRepository.ListSubjects();

            var itemToDelete = subjects.Where(x => x.SubjectId == id).FirstOrDefault();

            bool isConfirmed =  await Shell.Current.DisplayAlert("Confirm Delete",
                $"Are you sure to Delete {itemToDelete.Subject} subject?",
                "Yes", "No");

            if (isConfirmed)
            {
                databaseRepository.DeleteSubject(itemToDelete);
                LoadSubjects();
                ShowToastMessage($"{itemToDelete.Subject} Deleted Successfuly!");
            }
        }

        [RelayCommand]
        public async void GoToAddNewClassPage()
        {
            await Shell.Current.GoToAsync("addnewclass");
        }

        [RelayCommand]
        public async void ExportDatabaseSQLite()
        {
            CancellationTokenSource cancellation = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellation.Token;

            try
            {
                await SaveFileSQLite(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw new OperationCanceledException("Cancelled");
            }
        }

        public async Task SaveFileSQLite(CancellationToken cancellationToken)
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "blatsy_sqlite.db");

            byte[] databaseContent = File.ReadAllBytes(dbPath);

            var stream = new MemoryStream(databaseContent);

            var fileName = Shell.Current.DisplayPromptAsync("SQLite Database Backup", "The filename should end with '.db' extension!") ?? Task.FromResult("untitled_backup.db");
            var fileSaverResult = await FileSaver.Default.SaveAsync(await fileName, stream, cancellationToken);
            if (fileSaverResult.IsSuccessful)
            {
                await Toast.Make($"The file was saved successfully to location: {fileSaverResult.FilePath}").Show(cancellationToken);
            }
            else
            {
                await Toast.Make($"The file was not saved successfully with error: {fileSaverResult.Exception.Message}").Show(cancellationToken);
            }
        }



        //Public Toast Message
        public async void ShowToastMessage(string message)
        {
            var toast = Toast.Make(message, ToastDuration.Long, 14);
            await toast.Show();
        }
    }
}
