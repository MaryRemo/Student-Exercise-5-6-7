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
    public class CohortsController : ControllerBase
    {
        private readonly IConfiguration configuration;

        public CohortsController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        
        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            }
        }
        // GET: api/Cohort
        [HttpGet]
        public List<Cohort> GetAllCohorts()
        {
            using (SqlConnection conn = Connection)

            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT c.id AS CohortId, c.Name as CohortName,
                                        s.Id AS StudentId, s.FirstName AS StudentFirstName, s.LastName AS StudentLastName, s.SlachHandle as StudentSlackHandle,
                                          i.Id AS InstructorId, i.FirstName AS InstructorFirstName,  i.LastName AS InstructorLastName, i.SlachHandle as                            InstructorSlackHandle
                                            FROM Cohort c
                                            LEFT JOIN Student as s ON s.CohortId = c.id
                                            LEFT JOIN Instructor as i ON i.CohortId = c.id;";

                    SqlDataReader reader = cmd.ExecuteReader();

                    Dictionary<int, Cohort> cohorts = new Dictionary<int, Cohort>();
                    while (reader.Read())
                    {
                        int CohortId = reader.GetInt32(reader.GetOrdinal("CohortId"));
                        if (!cohorts.ContainsKey(CohortId))
                        {
                            Cohort newCohort = new Cohort
                            {
                                Id = CohortId,
                                Name = reader.GetString(reader.GetOrdinal("CohortName")),
                                //StudentList = new List<Student>(),
                                //InstructorList = new List<Instructor>()
                            };

                            cohorts.Add(CohortId, newCohort);
                        }
                        if (!reader.IsDBNull(reader.GetOrdinal("CohortId")))
                        {
                            Cohort currentCohort = cohorts[CohortId];
                            if (!reader.IsDBNull(reader.GetOrdinal("InstructorId")))
                            {
                                if (!currentCohort.StudentList.Exists(x => x.Id == reader.GetInt32(reader.GetOrdinal("StudentId"))))
                                {
                                    currentCohort.StudentList.Add(
                                    new Student
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                                        FirstName = reader.GetString(reader.GetOrdinal("StudentFirstName")),
                                        LastName = reader.GetString(reader.GetOrdinal("StudentLastName")),
                                    }
                                );
                                }
                            }


                            if (!reader.IsDBNull(reader.GetOrdinal("InstructorId")))
                            {
                                if (!currentCohort.InstructorList.Exists(x => x.Id == reader.GetInt32(reader.GetOrdinal("InstructorId"))))

                                {
                                    currentCohort.InstructorList.Add(
                                        new Instructor
                                        {
                                            Id = reader.GetInt32(reader.GetOrdinal("InstructorId")),
                                            FirstName = reader.GetString(reader.GetOrdinal("InstructorFirstName")),
                                            LastName = reader.GetString(reader.GetOrdinal("InstructorLastName"))
                                        }
                                    );
                                }

                            }
                        }
                    }
                    reader.Close();
                    return cohorts.Values.ToList();
                }
            }
        }

        // GET: api/Cohort/5
       /* [HttpGet("{id}", Name = "GetCohort")]
        public Cohort Get(int Id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT c.id AS CohortId, c.Name as CohortName,
                                        s.Id AS StudentId, s.FirstName AS StudentFirstName,
                                        s.LastName AS StudentLastName, s.SlachHandle as StudentSlackHandle,
                                        i.Id AS InstructorId, i.FirstName AS InstructorFirstName,
                                        i.LastName AS InstructorLastName, i.SlachHandle as InstructorSlackHandle
                                        FROM Cohort c
                                        LEFT JOIN Student as s ON s.CohortId = c.id
                                        LEFT JOIN Instructor as i ON i.CohortId = c.id
                                    WHERE c.Id = @Id";
                    cmd.Parameters.Add(new SqlParameter("@Id", Id));

                    SqlDataReader reader = cmd.ExecuteReader();

                    Cohort cohort = null;
                    while (reader.Read())
                    {
                        if (cohort == null)
                        {
                            cohort = new Cohort
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("CohortName")),
             
                            };
                        }

                        int studentId = reader.GetInt32(reader.GetOrdinal("StudentId"));
                        if (!cohort.StudentList.Any(s => s.Id == studentId))
                        {

                            Student student = new Student
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                FirstName = reader.GetString(reader.GetOrdinal("StudentFirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("StudentLastName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("StudentSlackHandle")),
                                CohortId = cohort.Id
                            };
                            cohort.StudentList.Add(student);
                        }

                        int instructorId = reader.GetInt32(reader.GetOrdinal("InstructorId"));
                        if (!cohort.InstructorList.Any(i => i.Id == instructorId))
                        {
                            Instructor instructor = new Instructor
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("InstructorId")),
                                FirstName = reader.GetString(reader.GetOrdinal("InstructorFirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("InstructorLastName"))
                            };
                            cohort.InstructorList.Add(instructor);
                        }

                        reader.Close();
                        return cohort;
                    }
            }
        }
    }*/

        // POST: api/Cohort
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Cohort newCohort)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @" INSERT INTO Cohort (Name)
                                         OUTPUT INSERTED.Id
                                         Values(@CohortName)";
                    cmd.Parameters.Add(new SqlParameter("@CohortName", newCohort.Name));

                    int newId = (int)cmd.ExecuteScalar();
                    newCohort.Id = newId;
                    return CreatedAtRoute("GetCohorts", new { id = newId }, newCohort);

                }
            }
        }

        // PUT: api/Cohort/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] Cohort cohort)
        {
            using (SqlConnection conn = Connection)

            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE Cohort 
                                        SET Name = @CohortName
                                        WHERE id = @id;";
                    cmd.Parameters.Add(new SqlParameter("@CohortName", cohort.Name));
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
                    cmd.CommandText = "DELETE FROM Cohort WHERE id = @id;";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
