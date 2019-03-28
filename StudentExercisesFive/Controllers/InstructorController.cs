using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Collections.Generic;
using StudentExerciseFive.Models;

namespace StudentExerciseFive.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstructorController : ControllerBase
    {
        private readonly IConfiguration _config;

        public InstructorController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }


        // GET: api/Instructor
        [HttpGet]
        public IEnumerable<Instructor> GetInstructors()
        {
            using (SqlConnection conn = Connection)

            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT i.id as InstructorId, 
                                                i.FirstName, 
                                                i.LastName, 
                                                i.SlachHandle, 
                                                c.Id as CohortId,
                                                c.Name as CohortName
                                        FROM Instructor as i
                                        LEFT JOIN Cohort as c on i.CohortId = c.id";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Instructor> instructors = new List<Instructor>();
                    while (reader.Read())
                    {
                        Instructor newInstructor = new Instructor
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("InstructorId")),
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
                        instructors.Add(newInstructor);
                    }
                    reader.Close();
                    return instructors;
                }
            }
        }

        // GET: api/Instructor/5
        [HttpGet("{id}", Name = "GetInstructors")]
        public Instructor Get(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT i.id as InstructorId, 
                                                i.FirstName, 
                                                i.LastName, 
                                                i.SlachHandle, 
                                                c.Id as CohortId,
                                                c.Name as CohortName
                                        FROM Instructor as i
                                        LEFT JOIN Cohort as c on i.CohortId = c.id
                                        WHERE i.id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();
                    Instructor instructor = null;
                    while (reader.Read())
                    {
                        instructor = new Instructor
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("InstructorId")),
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
                    return instructor;
                }
            }
        }

        // POST: api/Instructor
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Instructor newInstructor)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Instructor (FirstName, LastName, SlachHandle, CohortId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@FirstName, @LastName, @SlackHandle, @CohortId);";
                    cmd.Parameters.Add(new SqlParameter("@FirstName", newInstructor.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@LastName", newInstructor.LastName));
                    cmd.Parameters.Add(new SqlParameter("@SlackHandle", newInstructor.SlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@CohortId", newInstructor.CohortId));

                    int newId = (int)cmd.ExecuteScalar();
                    newInstructor.Id = newId;
                    return CreatedAtRoute("GetInstructors", new { id = newId }, newInstructor);

                }
            }
        }

        // PUT
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] Instructor instructor)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE Instructor
                                        SET FirstName = @FirstName,
                                            LastName = @LastName,
                                            SlachHandle = @SlackHandle,
                                            CohortId = @CohortId
                                        WHERE id = @id;";
                    cmd.Parameters.Add(new SqlParameter("@FirstName", instructor.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@LastName", instructor.LastName));
                    cmd.Parameters.Add(new SqlParameter("@SlackHandle", instructor.SlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@CohortId", instructor.CohortId));
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    cmd.ExecuteNonQuery();

                }
            }
        }

        // DELETE
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM Instructor WHERE id = @id;";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
