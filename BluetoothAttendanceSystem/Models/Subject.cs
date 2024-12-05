using SQLite;

namespace BluetoothAttendanceSystem.Models
{
    [Table("Subjects")]
    public class Subjects
    {
        [PrimaryKey, AutoIncrement]
        public int SubjectId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; }

        [Ignore]
        public IList<StudentSubject> StudentSubjects { get; set; }
    }
}
