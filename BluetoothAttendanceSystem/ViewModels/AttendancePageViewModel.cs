using BluetoothAttendanceSystem.Data;
using BluetoothAttendanceSystem.Models;
using BluetoothAttendanceSystem.Services;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;

namespace BluetoothAttendanceSystem.ViewModels
{
    public partial class AttendancePageViewModel : ObservableObject, IQueryAttributable
    {
        private readonly IDatabaseRepository databaseRepository;
        private readonly ISemanticScreenReader semanticScreenReader;
        public ExportStudentAttendanceToExcelService exportAttendanceRecords;

        [ObservableProperty]
        public ObservableCollection<AttendanceStudentSubjectModel> students;

        [ObservableProperty]
        public AttendanceStudentSubjectModel selectedStudent;

        [ObservableProperty]
        public Guid classID;

        [ObservableProperty]
        public string subject;

        [ObservableProperty]
        public string subjectCode;

        [ObservableProperty]
        public int subjectId;

        [ObservableProperty]
        public int totalFemales;

        [ObservableProperty]
        public int totalMales;

        [ObservableProperty]
        public string attendanceMinDate;

        [ObservableProperty]
        public string attendanceMaxDate;

        [ObservableProperty]
        public string attendanceSelectedDate;

        [ObservableProperty]
        public DateTime newSelectedDate = DateTime.Today;

        [ObservableProperty]
        public DateTime selectedDate = DateTime.Now;

        [ObservableProperty]
        public bool isLoadingStudents;

        public AttendancePageViewModel(IDatabaseRepository databaseRepository, ISemanticScreenReader semanticScreenReader, ExportStudentAttendanceToExcelService exportAttendanceRecords)
        {
            this.databaseRepository = databaseRepository;
            this.semanticScreenReader = semanticScreenReader;
            this.exportAttendanceRecords = exportAttendanceRecords;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            var studentInfo = query["ClassInfo"] as StudentSubjectModel;

            ClassID = studentInfo.ClassID;
            SubjectId = studentInfo.SubjectId;
            Subject = studentInfo.Subject;
            SubjectCode = studentInfo.SubjectCode;

            if(databaseRepository.ListAttendance().Count > 0)
            {
                var todayDateString = DateTime.Today.ToString("yyyy-MM-dd");
                var minDate = databaseRepository.GetMinimumDate(SubjectId, ClassID);
                var minimumDate = ((DateTime)minDate).ToString("yyyy-MM-dd");
                
                AttendanceMinDate = DateConverterToDatePicker(minimumDate);
                AttendanceMaxDate = "12/30/2050"; //DateConverterToDatePicker(todayDateString);
                AttendanceSelectedDate = DateConverterToDatePicker(todayDateString);
                //NewSelectedDate = DateTime.ParseExact(todayDateString, "MM/dd/yyyy", CultureInfo.InvariantCulture);

                var studentToLoad = databaseRepository.LoadAttendanceStudentSubject(todayDateString, ClassID, SubjectId);

                TotalFemales = studentToLoad.Where(s => s.Gender.ToLower() == "female").Count();
                TotalMales = studentToLoad.Where(s => s.Gender.ToLower() == "male").Count();

                Students = new ObservableCollection<AttendanceStudentSubjectModel>(studentToLoad);
            }
            else
            {
                TotalFemales = 0;
                TotalMales = 0;
                Students = null;
            }
        }

        
        public async Task LoadStudentsAttendance()
        {
            try
            {
                IsLoadingStudents = true;

                await Task.Run(async () =>
                {
                    if (databaseRepository.ListAttendance().Count > 0)
                    {
                        var newSelectedDate = NewSelectedDate.ToString("MM/dd/yyyy");

                        var queryDateString = DateConverterToDatabaseFormat(newSelectedDate);

                        var studentToLoad = databaseRepository.LoadAttendanceStudentSubject(queryDateString, ClassID, SubjectId);

                        TotalFemales = studentToLoad.Where(s => s.Gender.ToLower() == "female").Count();
                        TotalMales = studentToLoad.Where(s => s.Gender.ToLower() == "male").Count();

                        Students = new ObservableCollection<AttendanceStudentSubjectModel>(studentToLoad);
                    }
                    else
                    {
                        TotalFemales = 0;
                        TotalMales = 0;
                        Students = null;
                    }
                });
            }
            finally
            {
                IsLoadingStudents = false;
            }
        }

        partial void OnSelectedStudentChanged(AttendanceStudentSubjectModel students)
        {
            if(students != null)
            {
                IsPresentAbsentAlert(students);
            }
        }

        public async void IsPresentAbsentAlert(AttendanceStudentSubjectModel student)
        {
            var studentIsPresentAbsent = student.IsPresent == true ? "ABSENT" : "PRESENT";
            bool boolIsPresentAbsent = student.IsPresent == true ? false : true;

            bool isConfirmed = await Shell.Current.DisplayAlert("Manual Attendance Checking",
                $"Mark {student.Surname}, {student.Firstname} {student.Middlename} as {studentIsPresentAbsent}!",
                "Yes", "No");

            if (isConfirmed)
            {
                databaseRepository.UpdateAttendanceInRecordManually(boolIsPresentAbsent, student.StudentId, SubjectId, ClassID, student.AttendanceDate);

                await ShowToastMessage($"Student {student.Surname}, {student.Firstname} {student.Middlename} mark as {studentIsPresentAbsent}!");

                LoadStudentsAttendance();
            }
        }

        [RelayCommand]
        public async Task ExportStudentAttendanceRecords(AttendanceStudentSubjectModel student)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken cancellationToken = cts.Token;

            try
            {
                IsLoadingStudents = true;
                await Dispatcher.GetForCurrentThread().DispatchAsync(async () =>
                {
                   await exportAttendanceRecords.ExportAttendanceToExcelAsync(student, cancellationToken);
                });
            }
            catch (OperationCanceledException)
            {
                throw new OperationCanceledException("It was cancelled");
            }
            finally
            {
                IsLoadingStudents = false;
            }
        }

        static string DateConverterToDatePicker(string inputDate)
        {
            DateTime date = DateTime.ParseExact(inputDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            return date.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
        }

        static string DateConverterToDatabaseFormat(string inputDate)
        {
            DateTime date = DateTime.ParseExact(inputDate, "MM/dd/yyyy", CultureInfo.InvariantCulture);
            return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        public async Task ShowToastMessage(string message)
        {
            var toast = Toast.Make(message, ToastDuration.Long, 14);
            await toast.Show();
        }
    }
}
