using BluetoothAttendanceSystem.Data;
using BluetoothAttendanceSystem.Models;
using ClosedXML.Excel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using System.Globalization;

namespace BluetoothAttendanceSystem.Services
{
    public class ExportStudentAttendanceToExcelService
    {
        public readonly IDatabaseRepository databaseRepository;
        public ExportStudentAttendanceToExcelService(IDatabaseRepository databaseRepository)
        {
            this.databaseRepository = databaseRepository;
        }
        public async Task<string> ExportAttendanceToExcelAsync(AttendanceStudentSubjectModel student, CancellationToken cancellationToken)
        {
            string studentFullName = $"{student.Surname},{student.Firstname} {student.Middlename}";
            var userInfo = databaseRepository.GetUsers().Where(s => s.UserId == 1).FirstOrDefault();
            string fileName = $"{studentFullName}_{student.Subject}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            using(var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add($"{studentFullName} Attendance");

                worksheet.Cell(1, 1).Value = "BLATSY Generated Attendance Record";

                worksheet.Cell(3, 1).Value = $"Student: {studentFullName}";
                worksheet.Cell(4, 1).Value = $"Subject: {student.Subject}";
                worksheet.Cell(5, 1).Value = $"{userInfo.Profession}: {userInfo.Surname}, {userInfo.Firstname} {userInfo.Middlename}";

                worksheet.Cell("A1").Style
                    .Font.SetBold()
                    .Fill.SetBackgroundColor(XLColor.LightBlue);
                

                worksheet.Cell(7, 1).Value = "DATE OF ATTENDANCE";
                worksheet.Cell(7, 2).Value = "PRESENT / ABSENT";

                var studentAttendanceRecords = databaseRepository.GetAttendanceRecordsOfStudentToExport(student.StudentId, student.SubjectId, student.ClassID);

                var currentRow = 8;

                foreach(var record in studentAttendanceRecords)
                {
                    worksheet.Cell(currentRow, 1).Value = DateFormatConverter(record.AttendanceDate);
                    var attRecordAsString = record.IsPresent == true ? "PRESENT" : "ABSENT";
                    worksheet.Cell(currentRow, 2).Value = attRecordAsString;
                    if (record.IsPresent == true)
                    {
                        worksheet.Cell(currentRow, 2).Style.Fill.SetBackgroundColor(XLColor.LightGreen);
                    }
                    else
                    {
                        worksheet.Cell(currentRow, 2).Style.Fill.SetBackgroundColor(XLColor.Pink);
                    }

                    currentRow++;
                }

                using(var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.ToArray();
                    var fileSaverResult = await FileSaver.Default.SaveAsync(fileName, stream);
                    if (fileSaverResult.IsSuccessful)
                    {
                        await Toast.Make($"The file was saved successfully to location: {fileSaverResult.FilePath}").Show(cancellationToken);
                    } 
                    else
                    {
                        await Toast.Make($"The file was saved successfully to location: {fileSaverResult.FilePath}").Show(cancellationToken);
                    }
                }
            }

            return "sammy";
        }

        static string DateFormatConverter(string inputDate)
        {
            DateTime date = DateTime.ParseExact(inputDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            return date.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture);
        }

    }
}
