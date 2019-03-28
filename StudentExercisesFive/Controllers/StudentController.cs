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
        public async Task<IActionResult> Get(string include, string q)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    if (include == "exercise")
                    {
                        cmd.CommandText = $@"SELECT s.FirstName, s.LastName, e.Name, e.[Language], er.StudentId, er.ExerciseId, e.Id as eId, s.Id, s.SlachHandle, s.CohortId
                                        FROM Student s
                                        JOIN  AssignedExercises er ON s.Id = er.StudentId
                                        JOIN Exercise e on er.ExerciseId = e.Id WHERE 1 = 1 ";
                    }
                    else
                    {
                        cmd.CommandText = $@"SELECT s.Id, s.FirstName, s.LastName, s.SlachHandle, c.Name, s.CohortId FROM Student s 
                                          LEFT JOIN Cohort c ON s.CohortId = c.Id WHERE 1 = 1";

                    }
                    if (!string.IsNullOrWhiteSpace(q))
                    {
                        cmd.CommandText += @" AND FirstName LIKE @b OR LastName LIKE @b OR SlachHandle LIKE @b";
                        cmd.Parameters.Add(new SqlParameter("@b", $"%{q}%"));

                    }
                    SqlDataReader reader = cmd.ExecuteReader();

                    Dictionary<int, Student> students = new Dictionary<int, Student>();
                    while (reader.Read())
                    {
                        int studentid = reader.GetInt32(reader.GetOrdinal("Id"));

                        if (!students.ContainsKey(studentid))
                        {
                            Student student = new Student
                            {
                                Id = studentid,
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("SlachHandle")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Exercises = new List<Exercise>(),
                            };
                            students.Add(studentid, student);
                        }

                        if (include == "exercise")
                        {
                            if (!reader.IsDBNull(reader.GetOrdinal("eId")))
                            {
                                Student currentStudent = students[studentid];
                                currentStudent.Exercises.Add(
                                    new Exercise
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("eId")),
                                        Name = reader.GetString(reader.GetOrdinal("Name")),
                                        Language = reader.GetString(reader.GetOrdinal("Language")),
                                    }
                                );
                            }
                        }

                    }
                    reader.Close();

                    return Ok(students);
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
