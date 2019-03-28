using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExerciseFive.Models
{
    public class Cohort
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Student> StudentList { get; set; } = new List<Student>();
        public List<Instructor> InstructorList { get; set; } = new List<Instructor>();
        public object Values { get; internal set; }
    }
}
