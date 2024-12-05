namespace BluetoothAttendanceSystem.Models
{
    public class StudentSubjectModel
    {
        public Guid ClassID {get; set;}
        public string? FullCourseName { get; set;}
        public string? CourseAbbreviation { get; set;}
        public string? Year { get; set; }
        public string? Section { get; set; }
        public int SubjectId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
    }
}
