using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LAB5.Models;
namespace LAB5.Services{
    public class CsvService{
        public async Task ExportAsync( string filePath, IEnumerable<Student> students) {
            var lines = new List<string>{
                "Id;Name;Age;Mass;AverageMark;Marks"
            };
            foreach (Student student in students){
                string marks = string.Join(", ", student.Marks);
                lines.Add(string.Join(";",Escape(student.Id.ToString()),Escape(student.Name),
                    student.Age.ToString(CultureInfo.InvariantCulture),
                    student.Mass.ToString(CultureInfo.InvariantCulture),
                    student.AverageMark.ToString("F2", CultureInfo.InvariantCulture),
                    Escape(marks)));
            }
            await File.WriteAllLinesAsync( filePath, lines, new UTF8Encoding(true));
        }
        private static string Escape(string value){
            if (value.Contains(';') || value.Contains('"') || value.Contains('\n')) {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }
            return value;
        }
    }
}
