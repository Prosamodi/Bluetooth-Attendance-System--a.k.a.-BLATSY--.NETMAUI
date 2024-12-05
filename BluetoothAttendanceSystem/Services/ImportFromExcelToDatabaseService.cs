using BluetoothAttendanceSystem.Data;
using BluetoothAttendanceSystem.Models;
using ClosedXML.Excel;

namespace BluetoothAttendanceSystem.Services
{
    public class ImportFromExcelToDatabaseService
    {
        private IDatabaseRepository databaseRepository;
        
        public ImportFromExcelToDatabaseService(IDatabaseRepository databaseRepository)
        {
            this.databaseRepository = databaseRepository;
        }

        public async Task<string> ImportStudent(string filePath, int subjectId)
        {
            Guid ClassID = Guid.NewGuid();
            try
            {
                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RangeUsed().RowsUsed();

                    foreach(var row in rows.Skip(1))
                    {
                        var student = new Student
                        {
                            Firstname = row.Cell(1).GetValue<string>(),
                            Middlename = row.Cell(2).GetValue<string>(),
                            Surname = row.Cell(3).GetValue<string>(),
                            Gender = row.Cell(4).GetValue<string>(),
                            CourseAbbreviation = row.Cell(5).GetValue<string>(),
                            FullCourseName = row.Cell(6).GetValue<string>(),
                            Year = row.Cell(7).GetValue<string>(),
                            Section = row.Cell(8).GetValue<string>(),
                            Semester = row.Cell(9).GetValue<string>(),
                            BluetoothMACAddress = row.Cell(10).GetValue<string>(),
                            BluetoothGUID = Guid.Parse(row.Cell(11).GetValue<string>()),
                            Email = row.Cell(12).GetValue<string>(),
                            IsPresent = false,
                            ClassID = ClassID,
                            UpdatedAt = DateTime.Now
                        };

                        databaseRepository.CreateStudent(student);

                        var subjectStudent = new StudentSubject
                        {
                            StudentId = student.StudentId,
                            SubjectId = subjectId,
                            ClassId = ClassID,
                            UpdatedAt = DateTime.Now
                        };

                        databaseRepository.CreateStudentSubject(subjectStudent);

                        Console.WriteLine($"Successfully Added");

                    }

                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error reading Excel file: {ex.Message}");
            }

            return "sammy";
        }

    }
}
