using System;
using System.Collections.Generic;
namespace LAB5.Models{
    public class StudentComparer : IComparer<Student>{
        public int Compare(Student? x, Student? y){
            if (ReferenceEquals(x, y))
                return 0;
            if (x == null)
                return -1;
            if (y == null)
                return 1;
            int result = y.AverageMark.CompareTo(x.AverageMark);
            if (result != 0)
                return result;
            return string.Compare(
                x.Name,
                y.Name,
                StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
