using SQLite;

namespace BluetoothAttendanceSystem.Models
{
    [Table("Students")]
    public class Student
    {
        [PrimaryKey, AutoIncrement]
        public int StudentId { get; set; }
        public string? Firstname { get; set; }
        public string? Middlename { get; set; }
        public string? Surname { get; set; }
        public string? Gender { get; set; }
        public string? CourseAbbreviation { get; set; }
        public string? FullCourseName { get; set; }
        public string? Year { get; set; }
        public string? Section { get; set; }
        public string? Semester { get; set; }
        public Guid ClassID { get; set; } 
        public Guid BluetoothGUID { get; set; }
        public string? BluetoothMACAddress { get; set; }
        public string? Email { get; set; }
        public bool IsPresent { get; set; }
        public DateTime LastAttendance { get; set; }
        public int LastSubjectId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } 

        [Ignore]
        public IList<StudentSubject> StudentSubjects { get; set; }
    }
}
