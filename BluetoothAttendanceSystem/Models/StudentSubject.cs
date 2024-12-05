using SQLite;

namespace BluetoothAttendanceSystem.Models
{

    public class StudentSubject
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int SubjectId { get; set; }
        public Guid ClassId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; }

    }
}
