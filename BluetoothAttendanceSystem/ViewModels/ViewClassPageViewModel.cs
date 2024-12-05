using BluetoothAttendanceSystem.Data;
using BluetoothAttendanceSystem.Models;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BluetoothAttendanceSystem.ViewModels
{
    public partial class ViewClassPageViewModel : ObservableObject, IQueryAttributable
    {
        private readonly ISemanticScreenReader semanticScreenReader;
        private readonly IDatabaseRepository databaseRepository;

        //public ObservableCollection<Student> Students { get; } = new ObservableCollection<Student>();

        [ObservableProperty]
        public ObservableCollection<Student> students;

        [ObservableProperty]
        public string firstname;

        [ObservableProperty]
        public string middlename;

        [ObservableProperty]
        public string surname;

        [ObservableProperty]
        public string bluetoothMacAddress;

        public List<string> GenderOptions { get; } = new List<string> { "Male", "Female" };

        [ObservableProperty]
        public string gender;

        [ObservableProperty]
        public string fullCourseName;

        [ObservableProperty]
        public string courseAbbreviation;

        [ObservableProperty]
        public string year;

        [ObservableProperty]
        public string section;

        [ObservableProperty]
        public string semester;

        [ObservableProperty]
        public string email;

        [ObservableProperty]
        public Guid classID;

        [ObservableProperty]
        public string subject;

        [ObservableProperty]
        public string subjectCode;

        [ObservableProperty]
        public int totalFemales;

        [ObservableProperty]
        public int totalMales;

        [ObservableProperty]
        public string isAddingOrEditingStudent;

        [ObservableProperty]
        public bool isAddingStudent;

        [ObservableProperty]
        public bool isEditingStudentInfo;

        public int IdToEdit { get; set; }
        public int SubjectId { get; set; }

        public RelayCommand SaveStudentInfoCommand { get; }

        [RelayCommand]
        public void IsAddingStudentIsActive()
        {
            IsAddingStudent = true;
            IsEditingStudentInfo = false;
            IsAddingOrEditingStudent = "Adding New Student";

            var studentClass = Students.Where(s => s.ClassID == ClassID).FirstOrDefault();

            CourseAbbreviation = studentClass.CourseAbbreviation;
            FullCourseName = studentClass.FullCourseName;
            Year = studentClass.Year;
            Section = studentClass.Section;
            Semester = studentClass.Semester;
        }

        [RelayCommand]
        public void IsAddingStudentIsInactive()
        {
            IsAddingStudent = false;
            ClearInputs();
        }

        [RelayCommand]
        public void EditStudent(int studentId)
        {
            IsAddingStudent = true;
            IsEditingStudentInfo = true;
            IsAddingOrEditingStudent = "Editing Student Info";
            IdToEdit = studentId;

            var studentInfo = databaseRepository.ListStudents()
                .Where(s => s.StudentId == studentId).FirstOrDefault();

            Firstname = studentInfo.Firstname;
            Middlename = studentInfo.Middlename;
            Surname = studentInfo.Surname;
            BluetoothMacAddress = studentInfo.BluetoothMACAddress;
            Gender = studentInfo.Gender;
            CourseAbbreviation = studentInfo.CourseAbbreviation;
            FullCourseName = studentInfo.FullCourseName;
            Year = studentInfo.Year;
            Section = studentInfo.Section;
            Semester = studentInfo.Semester;
            Email = studentInfo.Email;

        }

        public ViewClassPageViewModel(IDatabaseRepository databaseRepository, ISemanticScreenReader semanticScreenReader)
        {
            this.databaseRepository = databaseRepository;
            this.semanticScreenReader = semanticScreenReader;

            SaveStudentCommand = new RelayCommand(SaveStudent, CanSaveStudent);
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            var studentInfo = query["ClassInfo"] as StudentSubjectModel;

            ClassID = studentInfo.ClassID;
            Subject = studentInfo.Subject;
            SubjectCode = studentInfo.SubjectCode;
            SubjectId = studentInfo.SubjectId;

            var students = databaseRepository.ListStudents();

            if (students.Count > 0)
            {
                var studentClass = students.Where(s => s.ClassID == studentInfo.ClassID)
                    .OrderBy(s => s.Surname).OrderBy(s => s.Gender);

                TotalFemales = studentClass.Where(s => s.Gender.ToLower() == "female").Count();
                TotalMales = studentClass.Where(s => s.Gender.ToLower() == "male").Count();

                Students = new ObservableCollection<Student>(studentClass);

            }
            else
            {
                TotalFemales = 0;
                TotalMales = 0;
                Students = null;
            }
        }

        public void LoadStudents()
        {
            var students = databaseRepository.ListStudents();

            if (students.Count > 0)
            {
                var studentClass = students.Where(s => s.ClassID == classID).ToList()
                    .OrderBy(s => s.Surname).OrderBy(s => s.Gender);

                TotalFemales = studentClass.Where(s => s.Gender.ToLower() == "female").Count();
                TotalMales = studentClass.Where(s => s.Gender.ToLower() == "male").Count();

                Students = new ObservableCollection<Student>(studentClass);
            }
            else
            {
                TotalFemales = 0;
                TotalMales = 0;
                Students = null;
            }
        }

        public RelayCommand SaveStudentCommand { get; }

        public async void SaveStudent()
        {
            if (!IsEditingStudentInfo)
            {
                var student = new Student
                {
                    Firstname = Firstname,
                    Middlename = Middlename,
                    Surname = Surname,
                    Gender = Gender,
                    CourseAbbreviation = CourseAbbreviation,
                    FullCourseName = FullCourseName,
                    Year = Year,
                    Section = Section,
                    Semester = Semester,
                    BluetoothMACAddress = BluetoothMacAddress,
                    Email = Email,
                    ClassID = ClassID,
                    UpdatedAt = DateTime.Now
                };

                databaseRepository.CreateStudentWithSubject(student, SubjectId);

                await ShowToastMessage($"Student {student.Surname}, {student.Firstname} {student.Middlename} Added Successfully!");

                LoadStudents();

                IsAddingStudent = false;
                IsEditingStudentInfo = false;
            } 
            else
            {
                var studentToUpdate = databaseRepository.ListStudents()
                    .Where(s => s.StudentId == IdToEdit).FirstOrDefault();

                studentToUpdate.Firstname = Firstname;
                studentToUpdate.Middlename = Middlename;
                studentToUpdate.Surname = Surname;
                studentToUpdate.BluetoothMACAddress = BluetoothMacAddress;
                studentToUpdate.Gender = Gender;
                studentToUpdate.CourseAbbreviation = CourseAbbreviation;
                studentToUpdate.FullCourseName = FullCourseName;
                studentToUpdate.Year = Year;
                studentToUpdate.Section = Section;
                studentToUpdate.Semester = Semester;
                studentToUpdate.Email = Email;
                studentToUpdate.UpdatedAt = DateTime.Now;

                databaseRepository.UpdateStudent(studentToUpdate);

                await ShowToastMessage($"Student {Surname}, {Firstname} {Middlename} Updated Successfully!");

                
                IsEditingStudentInfo = false;
                LoadStudents();

                IsAddingStudent = false;
            }
            
        }

        public bool CanSaveStudent()
        {
            if (!string.IsNullOrEmpty(Firstname) && !string.IsNullOrEmpty(Surname) &&
                !string.IsNullOrEmpty(Gender) && !string.IsNullOrEmpty(CourseAbbreviation) &&
                !string.IsNullOrEmpty(FullCourseName) && !string.IsNullOrEmpty(Year) &&
                !string.IsNullOrEmpty(Section) && !string.IsNullOrEmpty(Semester) &&
                !string.IsNullOrEmpty(BluetoothMacAddress) && !string.IsNullOrEmpty(Email) && IsValidEmail(Email))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        partial void OnFirstnameChanged(string value) => SaveStudentCommand.NotifyCanExecuteChanged();
        partial void OnSurnameChanged(string value) => SaveStudentCommand.NotifyCanExecuteChanged();
        partial void OnGenderChanged(string value) => SaveStudentCommand.NotifyCanExecuteChanged();
        partial void OnCourseAbbreviationChanged(string value) => SaveStudentCommand.NotifyCanExecuteChanged();
        partial void OnFullCourseNameChanged(string value) => SaveStudentCommand.NotifyCanExecuteChanged();
        partial void OnYearChanged(string value) => SaveStudentCommand.NotifyCanExecuteChanged();
        partial void OnSectionChanged(string value) => SaveStudentCommand.NotifyCanExecuteChanged();
        partial void OnSemesterChanged(string value) => SaveStudentCommand.NotifyCanExecuteChanged();
        partial void OnBluetoothMacAddressChanged(string value) => SaveStudentCommand.NotifyCanExecuteChanged();
        partial void OnEmailChanged(string value) => SaveStudentCommand.NotifyCanExecuteChanged();


        private bool IsValidEmail(string email)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        [RelayCommand]
        public async void DeleteStudent(int studentId)
        {
            var studentToDelete = databaseRepository.ListStudents()
                .Where(s => s.StudentId == studentId).FirstOrDefault();

            bool isConfirmed = await Shell.Current.DisplayAlert("Confirm Delete",
                $"Are you sure to Delete {studentToDelete.Surname}, {studentToDelete.Firstname} in this class?",
                "Yes", "No");

            if (isConfirmed)
            {
                databaseRepository.DeleteStudent(studentToDelete);
                LoadStudents();
                ShowToastMessage($"{studentToDelete.Surname}, {studentToDelete.Firstname} Deleted Successfully!");
            }
        }

        public void ClearInputs()
        {
            Firstname = string.Empty;
            Middlename = string.Empty;
            Surname = string.Empty;
            Gender = string.Empty;
            CourseAbbreviation = string.Empty;
            FullCourseName = string.Empty;
            Year = string.Empty;
            Section = string.Empty;
            Semester = string.Empty;
            BluetoothMacAddress = string.Empty;
            Email = string.Empty;
        }

        //Public Toast Message
        public async Task ShowToastMessage(string message)
        {
            var toast = Toast.Make(message, ToastDuration.Long, 14);
            await toast.Show();
        }
    }
}
