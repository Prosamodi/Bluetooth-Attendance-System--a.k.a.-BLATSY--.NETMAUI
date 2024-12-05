using SQLite;

namespace BluetoothAttendanceSystem.Models
{
    [Table("Attendance")]
    public class Attendance
    {
        [PrimaryKey, AutoIncrement]
        public int AttendanceId { get; set; }
        public int StudentId { get; set; }
        public int SubjectId { get; set; }
        public Guid ClassId { get; set; }
        public string? AttendanceDate { get; set; } 
        public bool IsPresent { get; set; }

        [Ignore]
        public Student? Student { get; set; }
        [Ignore]
        public Subjects? Subject { get; set; }
    }
}
