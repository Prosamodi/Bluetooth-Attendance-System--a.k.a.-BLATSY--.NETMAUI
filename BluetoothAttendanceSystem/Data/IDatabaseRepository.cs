using BluetoothAttendanceSystem.Models;

namespace BluetoothAttendanceSystem.Data
{
    public interface IDatabaseRepository
    {
       
        void CreateUser(UserInformation userInformation);
        void UpdateUserInfo(UserInformation userInformation);
        List<UserInformation> GetUsers();

        void CreateSubject(Subjects subject);
        void UpdateSubject(Subjects subject);
        void DeleteSubject(Subjects subject);
        IReadOnlyList<Subjects> ListSubjects();
        IList<string> LoadSubjects();
        void AddSubjectToExistingClass(Guid classId, int subjectId);

        void CreateStudent(Student student);
        void UpdateStudent(Student student);
        void CreateStudentWithSubject(Student student, int subjectId);
        IReadOnlyList<Student> ListStudents();
        IList<StudentSubjectModel> LoadClassAndSubjectAndSection();
        void DeleteClass(Guid classId, int subjectId);
        void DeleteClassWithSubjectOnly(Guid classId, int subjectId);
        void DeleteStudent(Student student);

        void CreateStudentSubject(StudentSubject student);
        IReadOnlyList<StudentSubject> ListStudentSubject();

        void SaveAttendance(Attendance attendance);
        void UpdateAttendance(Attendance attendance);
        void UpdateCurrentAttendanceManually(bool isPresent, int studentId, int subjectId, Guid classId);
        void UpdateAttendanceInRecordManually(bool isPresent, int studentId, int subjectId, Guid classId, string targetDate);
        bool AlreadyCheckedAttendance(int subjectId, Guid classId);
        void DeleteCurrentAttendance(int subjectId, Guid classId);
        IReadOnlyList<Attendance> ListAttendance();
        IList<AttendanceStudentSubjectModel> LoadAttendanceStudentSubject(string dateOfAttendance, Guid classId, int subjectId);
        DateTime? GetMinimumDate(int subjectId, Guid classId);
        IList<Attendance> GetAttendanceRecordsOfStudentToExport(int studentId, int subjectId, Guid classId);
    }
}