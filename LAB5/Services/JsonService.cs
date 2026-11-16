using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using LAB5.Models;
namespace LAB5.Services{
    public class JsonService{
        private readonly JsonSerializerOptions _options = new(){
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
        public async Task SaveAsync( string filePath, IEnumerable<Student> students) {
            string json = JsonSerializer.Serialize(students, _options);
            await File.WriteAllTextAsync(filePath, json);
        }
        public async Task<List<Student>> LoadAsync(string filePath){
            if (!File.Exists(filePath))
                return new List<Student>();
            string json = await File.ReadAllTextAsync(filePath);
            if (string.IsNullOrWhiteSpace(json))
                return new List<Student>();
            try{
                return JsonSerializer.Deserialize<List<Student>>(
                    json, _options) ?? new List<Student>();
            }
            catch (JsonException ex){
                throw new InvalidDataException(
                    "JSON-файл пошкоджений або має неправильний формат.", ex);
            }
        }
    }
}
