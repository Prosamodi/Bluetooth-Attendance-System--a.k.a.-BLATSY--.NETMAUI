using SQLite;

namespace BluetoothAttendanceSystem.Models
{
    public class UserInformation
    {
        [PrimaryKey, AutoIncrement]
        public int UserId { get; set; }
        public string? Firstname { get; set; }
        public string? Middlename { get; set; }
        public string? Surname { get; set; }
        public string? Email { get; set; }
        public string? Profession { get; set; }
    }
}
