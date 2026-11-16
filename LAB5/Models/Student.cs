using System;
using System.Collections.Generic;
using System.Linq;
namespace LAB5.Models{
    public class Student{
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public double Mass { get; set; }
        public List<int> Marks { get; set; } = new();
        public double AverageMark =>
            Marks.Count == 0 ? 0 : Marks.Average();
    }
}
