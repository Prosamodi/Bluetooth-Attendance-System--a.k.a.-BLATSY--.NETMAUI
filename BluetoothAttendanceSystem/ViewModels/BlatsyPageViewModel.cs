using BluetoothAttendanceSystem.Data;
using BluetoothAttendanceSystem.Models;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System.Collections.ObjectModel;

namespace BluetoothAttendanceSystem.ViewModels
{
    public partial class BlatsyPageViewModel : ObservableObject, IQueryAttributable
    {
        private readonly IDatabaseRepository databaseRepository;
        private readonly ISemanticScreenReader semanticScreenReader;
        private readonly IPreferences preferences;

        public ObservableCollection<BluetoothDeviceInfo> BluetoothDevices { get; set; }

        [ObservableProperty]
        public List<string> detectedUUIDs;

        [ObservableProperty]
        public List<string> detectedMacAddresses;

        [ObservableProperty]
        public List<string> detectedBTNames;

        [ObservableProperty]
        public ObservableCollection<Student> students;

        [ObservableProperty]
        public Student selectedStudent;

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
        double bluetoothScanTimeSpan;
        public ObservableCollection<string> ScanDuration { get; } = new ObservableCollection<string> { "30 Seconds", "1 Minute", "2 Minutes", "3 Minutes", "5 Minutes" };

        [ObservableProperty]
        public string selectedScanDuration;

        [ObservableProperty]
        public bool isCheckingAttendance = false;

        public bool IsAlreadyChecked = false;


        public BlatsyPageViewModel(IDatabaseRepository databaseRepository, ISemanticScreenReader semanticScreenReader, IPreferences preferences)
        {
            this.databaseRepository = databaseRepository;
            this.semanticScreenReader = semanticScreenReader;
            this.preferences = preferences;

            BluetoothDevices = new ObservableCollection<BluetoothDeviceInfo>();

            DetectedMacAddresses = new List<string>();
            DetectedBTNames = new List<string>();

            var durationPreference = preferences.Get("LastScanDurationSelected", 30);
            var durationTextPreference = preferences.Get("LastScanDurationSelectedText", "30 Seconds");

            if (ScanDuration.Count > 0)
            {
                SelectedScanDuration = durationTextPreference;
                BluetoothScanTimeSpan = durationPreference;
            }

            IsAlreadyChecked = false;
            
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
                var studentClassToFalse = students.Where(s => s.ClassID == studentInfo.ClassID);

                foreach(var student in studentClassToFalse)
                {
                    student.IsPresent = false;
                    //student.LastAttendance = DateTime.Today;
                    //student.LastSubjectId = SubjectId;
                    student.UpdatedAt = DateTime.Now;

                    databaseRepository.UpdateStudent(student);
                }

                var studentsUpdated = databaseRepository.ListStudents();

                var studentClass = studentsUpdated.Where(s => s.ClassID == studentInfo.ClassID)
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
#if ANDROID
            AndroidPermission();
#endif
            IfBluetoothOnOrOff();
        }

        public async Task AndroidPermission()
        {

            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                await Shell.Current.DisplayAlert("Permission Required", "Location permission is required for Bluetooth scanning.", "OK");
                return;
            }

        }

        public async void IfBluetoothOnOrOff()
        {
            var radio = BluetoothRadio.Default;

            if (radio.Mode == RadioMode.PowerOff)
            {
                await Shell.Current.DisplayAlert("Bluetooth Required", "Turn On your Bluetooth First!", "OK");
                return;
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

        partial void OnSelectedStudentChanged(Student value)
        {

            if(value is not null)
            {
                if (IsAlreadyChecked == true)
                {
                    IsPresentAbsentAlert(value);
                }
                else
                {
                    CheckAttendanceFirstAlert(value);
                }
            }
            
        }

        public async void IsPresentAbsentAlert(Student student)
        {
            var studentIsPresentAbsent = student.IsPresent == true ? "ABSENT" : "PRESENT";
            bool boolIsPresentAbsent = student.IsPresent == true ? false : true;

            bool isConfirmed = await Shell.Current.DisplayAlert("Manual Attendance Checking",
                $"Mark {student.Surname}, {student.Firstname} {student.Middlename} as {studentIsPresentAbsent}!",
                "Yes", "No");

            if (isConfirmed)
            {
                //ToUpdate ListView Appearance GREEN Border for PRESENT and RED for ABSENT
                student.IsPresent = student.IsPresent == true ? false : true;
                student.LastAttendance = DateTime.Now;
                student.LastSubjectId = SubjectId;
                student.UpdatedAt = DateTime.Now;

                databaseRepository.UpdateStudent(student);

                databaseRepository.UpdateCurrentAttendanceManually(boolIsPresentAbsent, student.StudentId, SubjectId, ClassID);

                await ShowToastMessage($"Student {student.Surname}, {student.Firstname} {student.Middlename} mark as {studentIsPresentAbsent}!");

                LoadStudents();
            }
        }

        public async void CheckAttendanceFirstAlert(Student student)
        {
            await Shell.Current.DisplayAlert("Manual Attendance Checking",
                $"Attendance Checking has not yet run. Please tap the Check Attendance button First!",
                "Okay");
        }




        ////
        //  BLUETOOTH ATTENDANCE SYSTEM aka BLATSY
        ////

        [RelayCommand]
        public async Task StartBlatsy()
        {
            
            DetectedBTNames.Clear();
            DetectedMacAddresses.Clear();
            BluetoothDevices.Clear();
            
            if (!databaseRepository.AlreadyCheckedAttendance(SubjectId, ClassID))
            {

                try
                {
                    IsCheckingAttendance = true;

                    TimeSpan scanDuration = TimeSpan.FromSeconds(BluetoothScanTimeSpan);

                    using CancellationTokenSource cts = new CancellationTokenSource(scanDuration);
                    //var radio = BluetoothRadio.Default;

                    BluetoothClient client = new BluetoothClient();

                    var devices = await Task.Run(() => client.DiscoverDevices(), cts.Token);

                    foreach (var device in devices)
                    {
                        if (!BluetoothDevices.Contains(device))
                        {
                            BluetoothDevices.Add(device);
                        }

                    }

                    DetectedMacAddresses = BluetoothDevices.Select(s => s.DeviceAddress.ToString()).ToList();

                    if (BluetoothDevices.Count > 0)
                    {

                        foreach (var student in Students)
                        {

                            var trimmedMacs = student.BluetoothMACAddress.Replace(":", "");

                            student.IsPresent = DetectedMacAddresses.Contains(trimmedMacs);
                            student.LastAttendance = DateTime.Today;
                            student.LastSubjectId = SubjectId;
                            student.UpdatedAt = DateTime.Now;

                            databaseRepository.UpdateStudent(student);

                            var attendance = new Attendance
                            {
                                StudentId = student.StudentId,
                                SubjectId = SubjectId,
                                IsPresent = DetectedMacAddresses.Contains(trimmedMacs),
                                ClassId = ClassID,
                                AttendanceDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") // Save as ISO 8601 string
                                //AttendanceDate = DateTime.UtcNow.ToString("o")
                            };

                            databaseRepository.SaveAttendance(attendance);
                        }
                    }

                    LoadStudents();
                    IsAlreadyChecked = true;

                    //await ShowToastMessage("Successfully saved Attendance!");

                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Bluetooth scan canceled after reaching the time limit.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
                finally
                {
                    IsCheckingAttendance = false;
                }
            }
            else
            {
                IsAlreadyChecked = false;
                IfAlreadyCheckedAttendanceAlert();
            }

        }

        //Just the Same Implementation Above
        //Just Keeping it for testing and debugging
        public void CheckAttendance()
        {
            DetectedMacAddresses = BluetoothDevices.Select(s => s.DeviceAddress.ToString()).ToList();

            if (BluetoothDevices.Count > 0)
            {

                foreach (var student in Students)
                {

                    var trimmedMacs = student.BluetoothMACAddress.Replace(":", "");

                    student.IsPresent = DetectedMacAddresses.Contains(trimmedMacs);
                    student.LastAttendance = DateTime.Today;
                    student.LastSubjectId = SubjectId;
                    student.UpdatedAt = DateTime.Now;

                    databaseRepository.UpdateStudent(student);

                    var attendance = new Attendance
                    {
                        StudentId = student.StudentId,
                        SubjectId = SubjectId,
                        IsPresent = DetectedMacAddresses.Contains(trimmedMacs),
                        ClassId = ClassID,
                        AttendanceDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") // Save as ISO 8601 string
                        //AttendanceDate = DateTime.UtcNow.ToString("o")
                    };

                    databaseRepository.SaveAttendance(attendance);
                }
            }
        }

        public async void IfAlreadyCheckedAttendanceAlert()
        {
            bool isConfirmed = await Shell.Current.DisplayAlert("Already Checked Attendance",
                "You have already checked attendance for this Class and Subject. Do You want to Check Attendance Again?",
                "Yes", "No");

            if (isConfirmed)
            {
                databaseRepository.DeleteCurrentAttendance(SubjectId, ClassID);

                foreach(var student in Students)
                {
                    student.IsPresent = false;

                    databaseRepository.UpdateStudent(student);
                }

                await ShowToastMessage("Current Attendance Deleted!");

                await Shell.Current.DisplayAlert("Attendance is Refresh",
                "You can now check attendance again.",
                "Okay");

                IsAlreadyChecked = false;
                LoadStudents();
            }
            
        }


        //For Testing
        public async void IfNoCheckedAttendanceAlert()
        {
            bool isConfirmed = await Shell.Current.DisplayAlert("No Attendance",
                "You already checked attendance for this Class and Subject. Do You want to Check Attendance Again?",
                "Yes", "No");
        }

        partial void OnSelectedScanDurationChanged(string value)
        {
            //"30 Seconds", "1 Minute", "2 Minutes", "3 Minutes", "5 Minutes"

            switch (value)
            {
                case "30 Seconds":
                    BluetoothScanTimeSpan = 30;
                    preferences.Set("LastScanDurationSelected", 30);
                    preferences.Set("LastScanDurationSelectedText", "30 Seconds");
                    break;
                case "1 Minute":
                    BluetoothScanTimeSpan = 60;
                    preferences.Set("LastScanDurationSelected", 60);
                    preferences.Set("LastScanDurationSelectedText", "1 Minute");
                    break;
                case "2 Minutes":
                    BluetoothScanTimeSpan = 120;
                    preferences.Set("LastScanDurationSelected", 120);
                    preferences.Set("LastScanDurationSelectedText", "2 Minutes");
                    break;
                case "3 Minutes":
                    BluetoothScanTimeSpan = 180;
                    preferences.Set("LastScanDurationSelected", 180);
                    preferences.Set("LastScanDurationSelectedText", "3 Minutes");
                    break;
                case "5 Minutes":
                    BluetoothScanTimeSpan = 300;
                    preferences.Set("LastScanDurationSelected", 300);
                    preferences.Set("LastScanDurationSelectedText", "5 Minutes");
                    break;
            }

            ShowToastMessage($"Bluetooth Scan Time Span set to {value}.");

            //DisplaySelectedDuration();
        }

        public void ClearStudentAttendance()
        {
            foreach(var student in Students)
            {
                student.IsPresent = false;
                student.LastAttendance = DateTime.Today;
                student.LastSubjectId = SubjectId;
                student.UpdatedAt = DateTime.Now;

                databaseRepository.UpdateStudent(student);
            }
        }

        //For Testing

        [RelayCommand]
        private async void DisplayDetectedDevice()
        {
           
            DetectedMacAddresses = BluetoothDevices.Select(s => s.DeviceAddress.ToString()).ToList();

            string message = string.Join("\n", DetectedMacAddresses);

            await Shell.Current.DisplayAlert("Items", message, "OK");

            DetectedBTNames = BluetoothDevices.Select(s => s.DeviceName.ToString()).ToList();

            string message1 = string.Join("\n", DetectedBTNames);

            await Shell.Current.DisplayAlert("Items", message1, "OK");

        }

        public async void DisplaySelectedDuration()
        {
            await Shell.Current.DisplayAlert("Selected Scan Duration", $"{SelectedScanDuration} with timespan of {BluetoothScanTimeSpan}", "OK");
        }

        public async Task ShowToastMessage(string message)
        {
            var toast = Toast.Make(message, ToastDuration.Long, 14);
            await toast.Show();
        }
    }
}
