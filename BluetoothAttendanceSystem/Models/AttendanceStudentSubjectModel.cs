namespace BluetoothAttendanceSystem.Models
{
    public class AttendanceStudentSubjectModel
    {
        public int StudentId { get; set; }
        public string? Firstname { get; set; }
        public string? Middlename { get; set; }
        public string? Surname { get; set; }
        public string? Gender { get; set; }
        public Guid ClassID { get; set; }
        public string? FullCourseName { get; set; }
        public string? CourseAbbreviation { get; set; }
        public string? Year { get; set; }
        public string? Section { get; set; }
        public int SubjectId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public string? AttendanceDate { get; set; }
        public bool IsPresent { get; set; }
    }
}
