using BluetoothAttendanceSystem.Models;
using SQLite;

namespace BluetoothAttendanceSystem.Data
{
    public class DatabaseRepository : IDatabaseRepository
    {
        private readonly SQLiteConnection connection;
        public DatabaseRepository(IFileSystem fileSystem)
        {
            var dbPath = Path.Combine(fileSystem.AppDataDirectory, "blatsy_sqlite.db");
            connection = new SQLiteConnection(dbPath);

            connection.CreateTable<UserInformation>();
            connection.CreateTable<Subjects>();
            connection.CreateTable<Student>();
            connection.CreateTable<StudentSubject>();
            connection.CreateTable<Attendance>();

        }

        public void AddSubjectToExistingClass(Guid classId, int subjectId)
        {
            var allStudentId = connection.Table<Student>()
                .Where(s => s.ClassID == classId)
                .Select(s => s.StudentId).ToList();

            foreach(var studentId in allStudentId)
            {
                var studentSubject = new StudentSubject
                {
                    StudentId = studentId,
                    SubjectId = subjectId,
                    ClassId = classId,
                    UpdatedAt = DateTime.Now
                };

                CreateStudentSubject(studentSubject);
            }
        }

        public bool AlreadyCheckedAttendance(int subjectId, Guid classId)
        {

            string todayDateString = DateTime.Today.ToString("yyyy-MM-dd");

            string query = @"
                SELECT EXISTS (
                    SELECT 1
                    FROM Attendance
                    WHERE SubjectId = ?
                    AND ClassId = ?
                    AND date(AttendanceDate) = ?
                );
            ";

            int result = connection.ExecuteScalar<int>(query, subjectId, classId, todayDateString);

            return result == 1;

        }

        public void CreateStudent(Student student)
        {
            connection.Insert(student);
        }

        public void CreateStudentSubject(StudentSubject studentSubject)
        {
            connection.Insert(studentSubject);
        }

        public void CreateStudentWithSubject(Student student, int subjectId)
        {
            connection.Insert(student);

            var subjectStudent = new StudentSubject
            {
                StudentId = student.StudentId,
                SubjectId = subjectId,
                ClassId = student.ClassID,
                UpdatedAt = DateTime.Now
            };

            connection.Insert(subjectStudent);

        }

        public void CreateSubject(Subjects subject)
        {
            connection.Insert(subject);
        }

        public void CreateUser(UserInformation userInformation)
        {
            connection.Insert(userInformation);
        }

        public void DeleteClass(Guid classId, int subjectId)
        {
            var classToDelete = connection.Table<Student>()
                .Where(s => s.ClassID == classId).ToList();

            foreach(var studentToDelete in classToDelete)
            {
                connection.Delete(studentToDelete);
            }

            var studentSubjectToDelete = connection.Table<StudentSubject>()
                .Where(ss => ss.ClassId == classId)
                .Where(ss => ss.SubjectId == subjectId).ToList();

            foreach(var ssToDelete in studentSubjectToDelete)
            {
                connection.Delete(ssToDelete);
            }
        }

        public void DeleteClassWithSubjectOnly(Guid classId, int subjectId)
        {
            var studentSubjectToDelete = connection.Table<StudentSubject>()
                .Where(ss => ss.ClassId == classId)
                .Where(ss => ss.SubjectId == subjectId).ToList();

            foreach(var ssToDelete in studentSubjectToDelete)
            {
                connection.Delete(ssToDelete);
            }
        }

        public void DeleteCurrentAttendance(int subjectId, Guid classId)
        {
            string todayDateString = DateTime.Today.ToString("yyyy-MM-dd");

            string deleteQuery = @"
                DELETE FROM Attendance
                WHERE SubjectId = ?
                AND ClassId = ?
                AND date(AttendanceDate) = ?;
            ";

            connection.Execute(deleteQuery, subjectId, classId, todayDateString);

        }

        public void DeleteStudent(Student student)
        {
            connection.Delete(student);
        }

        public void DeleteSubject(Subjects subjectToDelete)
        {
            connection.Delete(subjectToDelete);
        }

        public List<UserInformation> GetUsers()
        {
            return connection.Table<UserInformation>().ToList();
        }

        public IReadOnlyList<Student> ListStudents()
        {
            return connection.Table<Student>().ToList();
        }

        public IReadOnlyList<StudentSubject> ListStudentSubject()
        {
            return connection.Table<StudentSubject>().ToList();
        }

        public IReadOnlyList<Subjects> ListSubjects()
        {
            return connection.Table<Subjects>().ToList();
        }

        
        public IList<StudentSubjectModel> LoadClassAndSubjectAndSection()
        {
            var query = @"
                SELECT DISTINCT S.ClassID, S.FullCourseName, 
                S.CourseAbbreviation, S.Year, S.Section,
                SUB.SubjectId, SUB.Subject, SUB.SubjectCode 
                FROM Students S
                JOIN StudentSubject SS ON S.StudentId = SS.StudentId
                JOIN Subjects SUB ON SUB.SubjectId = SS.SubjectId
            ";

            return connection.Query<StudentSubjectModel>(query);
        }

        public IList<string> LoadSubjects()
        {
            var subjects = connection.Table<Subjects>().ToList();
            var allSubjectName = subjects.Select(s => s.Subject).ToList();

            return allSubjectName;

        }

        public void SaveAttendance(Attendance attendance)
        {
            connection.Insert(attendance);
        }

        public void UpdateAttendance(Attendance attendance)
        {
            connection.Update(attendance);
        }

        public void UpdateCurrentAttendanceManually(bool isPresent, int studentId, int subjectId, Guid classId)
        {
            string todayDateString = DateTime.Today.ToString("yyyy-MM-dd");

            string updateQuery = @"
                UPDATE Attendance
                SET IsPresent = ?
                WHERE SubjectId = ?
                AND ClassId = ?
                AND StudentId = ?
                AND date(AttendanceDate) = ?;
            ";

            connection.Execute(updateQuery, isPresent, subjectId, classId, studentId, todayDateString);
        }

        public void UpdateStudent(Student student)
        {
            connection.Update(student);
        }

        public void UpdateSubject(Subjects subject)
        {
            connection.Update(subject);
        }

        public void UpdateUserInfo(UserInformation userInformation)
        {
            connection.Update(userInformation);
        }

        public IReadOnlyList<Attendance> ListAttendance()
        {
            return connection.Table<Attendance>().ToList();
        }

        public IList<AttendanceStudentSubjectModel> LoadAttendanceStudentSubject(string dateOfAttendance, Guid classId, int subjectId)
        {
            if (ListAttendance().Count > 0)
            {
                var query = @"
                SELECT S.StudentId, S.Firstname, S.Middlename, S.Surname, S.Gender,
                S.ClassID, S.FullCourseName, S.CourseAbbreviation, S.Year, S.Section,
                SUB.SubjectId, SUB.Subject, SUB.SubjectCode,
                A.AttendanceDate, A.IsPresent
                FROM Students S
                JOIN StudentSubject SS ON S.StudentId = SS.StudentId
                JOIN Subjects SUB ON SUB.SubjectId = SS.SubjectId
                JOIN Attendance A ON A.StudentId = S.StudentId AND A.SubjectId = SUB.SubjectId
                WHERE A.SubjectId = ? AND A.ClassId = ? AND date(A.AttendanceDate) = ?
            ";

                return connection.Query<AttendanceStudentSubjectModel>(query, subjectId, classId, dateOfAttendance);
            }

             return new List<AttendanceStudentSubjectModel>();
        }

        public DateTime? GetMinimumDate(int subjectId, Guid classId)
        {
            string query = @"
                SELECT MIN(AttendanceDate)
                FROM Attendance
                WHERE SubjectId = ? AND ClassId = ?
            ";

            string minDateString = connection.ExecuteScalar<string>(query, subjectId, classId);

            if(DateTime.TryParse(minDateString, out DateTime minDate))
            {
                return minDate;
            }

            return null;
        }

        public void UpdateAttendanceInRecordManually(bool isPresent, int studentId, int subjectId, Guid classId, string targetDate)
        {
            DateTime td = DateTime.Parse(targetDate);
            var TargetDate = td.ToString("yyyy-MM-dd");

            string updateQuery = @"
                UPDATE Attendance
                SET IsPresent = ?
                WHERE SubjectId = ?
                AND ClassId = ?
                AND StudentId = ?
                AND date(AttendanceDate) = ?;
            ";

            connection.Execute(updateQuery, isPresent, subjectId, classId, studentId, TargetDate);
        }

        public IList<Attendance> GetAttendanceRecordsOfStudentToExport(int studentId, int subjectId, Guid classId)
        {
            if (ListAttendance().Count > 0)
            {
                var query = @"
                SELECT *
                FROM Attendance A
                WHERE A.StudentId = ? AND A.SubjectId = ? AND A.ClassId = ?
                ORDER BY A.AttendanceDate ASC;
            ";

                return connection.Query<Attendance>(query, studentId, subjectId, classId);
            }

            return new List<Attendance>();
        }
    }
}
