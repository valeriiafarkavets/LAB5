using System;
using System.Collections.Generic;
namespace LAB5.Models{
    public class StudentGroup{
        public string Name { get; set; } = string.Empty;
        public List<Student> Students { get; set; } = new();
        public void AddStudent(Student student){
            if (student == null)
                throw new ArgumentNullException(nameof(student));

            Students.Add(student);
        }
        public bool RemoveStudent(Guid studentId) {
            Student? student = Students.Find(s => s.Id == studentId);

            if (student == null)
                return false;

            return Students.Remove(student);
        }
    }
}
