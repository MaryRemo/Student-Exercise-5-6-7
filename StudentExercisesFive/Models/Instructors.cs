using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExerciseFive.Models
{
    public class Instructor
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SlackHandle { get; set; }
        public int CohortId { get; set; }
        public Cohort cohort { get; set; }
    }
}
