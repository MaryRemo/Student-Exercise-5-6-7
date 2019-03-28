using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using StudentExerciseFive.Models;

namespace StudentExerciseFive.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IConfiguration _config;

        public StudentController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }        // GET: api/Student
        [HttpGet]
        public IEnumerable<Student> GetStudents(string include)
        {

            using (SqlConnection conn = Connection)

            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    if (include == "exercise")
                    {
                        cmd.CommandText = @"select s.id as StudentId, 
                                      s.FirstName,
                                         s.LastName,
                                      s.SlachHandle,
                                      s.CohortId,
                                      c.[Name] as CohortName,
                                      e.id as ExerciseId,
                                      e.[name] as ExerciseName,
                                      e.[Language]
                                    from student s
                                    left join Cohort c on s.CohortId = c.id
                                    left join AssignedExercises se on s.id = se.studentid
                                    left join Exercise e on se.exerciseid = e.id";
                        SqlDataReader reader = cmd.ExecuteReader();

                        //List<Student> students = new List<Student>();
                        Dictionary<int, Student> students = new Dictionary<int, Student>();

                        while (reader.Read())
                        {
                            int StudentId = reader.GetInt32(reader.GetOrdinal("StudentId"));
                            if (!students.ContainsKey(StudentId))
                            {
                                Student newStudent = new Student
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("StudentId")),

                                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                    CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                    cohort = new Cohort
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                        Name = reader.GetString(reader.GetOrdinal("CohortName"))
                                    }


                                };
                                students.Add(StudentId, newStudent);
                            }

                            Student currentStudent = students[StudentId];
                            if (!reader.IsDBNull(reader.GetOrdinal("ExerciseId")))
                            {

                                currentStudent.Exercises.Add(
                                            new Exercise
                                            {
                                                Id = reader.GetInt32(reader.GetOrdinal("ExerciseId")),
                                                Name = reader.GetString(reader.GetOrdinal("ExerciseName")),
                                                Language = reader.GetString(reader.GetOrdinal("Language"))
                                            }
                                            );
                            }
                        }
                        reader.Close();
                        return students.Values.ToList();
                    }

                    else
                    {

                        cmd.CommandText = @" select s.id as StudentId, 
                                      s.FirstName,
                                         s.LastName,
                                      s.SlachHandle,
                                      s.CohortId,
                                      c.[Name] as CohortName,
                                      e.id as ExerciseId,
                                      e.[name] as ExerciseName,
                                      e.[Language]
                                    from student s
                                    left join Cohort c on s.CohortId = c.id
                                    left join AssignedExercises se on s.id = se.studentid
                                    left join Exercise e on se.exerciseid = e.id";
                        SqlDataReader reader = cmd.ExecuteReader();
                        List<Student> students = new List<Student>();
                        while (reader.Read())
                        {
                            Student newStudent2 = new Student
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("SlachHandle")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                cohort = new Cohort
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                    Name = reader.GetString(reader.GetOrdinal("CohortName"))
                                }
                            };
                            students.Add(newStudent2);
                        };
                        reader.Close();
                        return students;
                    }
                }
            }
        }


        // GET: api/Student/5
        [HttpGet("{id}", Name = "GetStudents")]
        public Student Get(int id)
        {
            using (SqlConnection conn = Connection)

            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @" select s.id as StudentId, 
		                                    s.FirstName,
	                                        s.LastName,
		                                    s.SlachHandle,
		                                    s.CohortId,
		                                    c.[Name] as CohortName,
		                                    e.id as ExerciseId,
		                                    e.[name] as ExerciseName,
		                                    e.[Language]
                                    from student s
                                    left join Cohort c on s.CohortId = c.id
                                    left join StudentExercise se on s.id = se.studentid
                                    left join Exercise e on se.exerciseid = e.id
                                    WHERE s.id = @Id";
                    cmd.Parameters.Add(new SqlParameter("@Id", id));
                    SqlDataReader reader = cmd.ExecuteReader();
                    Student student = null;
                    while (reader.Read())
                    {
                        student = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            cohort = new Cohort
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Name = reader.GetString(reader.GetOrdinal("CohortName"))
                            }
                        };

                    }
                    reader.Close();
                    return student;
                }
            }
        }

        // POST: api/Student
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Student newStudent)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Student (FirstName, LastName, SlachHandle, CohortId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@FirstName, @LastName, @SlackHandle, @CohortId);";
                    cmd.Parameters.Add(new SqlParameter("@FirstName", newStudent.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@LastName", newStudent.LastName));
                    cmd.Parameters.Add(new SqlParameter("@SlackHandle", newStudent.SlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@CohortId", newStudent.CohortId));

                    int newId = (int)cmd.ExecuteScalar();
                    newStudent.Id = newId;
                    return CreatedAtRoute("GetStudents", new { id = newId }, newStudent);

                }
            }
        }

        // PUT: api/Student/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] Student student)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE Student
                                        SET FirstName = @FirstName, 
                                            LastName = @LastName,
                                            SlachHandle = @SlackHandle,
                                            CohortId = @CohortId
                                        WHERE id = @id;";
                    cmd.Parameters.Add(new SqlParameter("@FirstName", student.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@LastName", student.LastName));
                    cmd.Parameters.Add(new SqlParameter("@SlackHandle", student.SlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@CohortId", student.CohortId));
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM Student WHERE id = @id;";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
