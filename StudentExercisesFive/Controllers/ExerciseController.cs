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
    public class ExerciseController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ExerciseController(IConfiguration config)
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

        //2. Code for getting a list of exercises
        [HttpGet]
        public IEnumerable<Exercise> GetExercises()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Name, Language FROM Exercise";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Exercise> exercises = new List<Exercise>();

                    while (reader.Read())
                    {
                        Exercise exercise = new Exercise
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Language = reader.GetString(reader.GetOrdinal("Language"))
                        };

                        exercises.Add(exercise);
                    }
                    reader.Close();
                    return exercises;
                }
            }
        }

        //3. Code for getting an exercise
        [HttpGet("{id}", Name = "GetExercises")]
        public Exercise Get(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, Name, Language 
                                        FROM Exercise 
                                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Exercise exercise = null;

                    if (reader.Read())
                    {
                        exercise = new Exercise
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Language = reader.GetString(reader.GetOrdinal("Language"))
                        };
                    }
                    reader.Close();
                    return exercise;
                }
            }
        }

        // 4. Code for creating an exercise
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Exercise exercise)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Exercise (Name, Language)
                                        OUTPUT INSERTED.Id
                                        Values (@name, @language)";
                    cmd.Parameters.Add(new SqlParameter("@name", exercise.Name));
                    cmd.Parameters.Add(new SqlParameter("@language", exercise.Language));
                    int newId = (int)cmd.ExecuteScalar();
                    exercise.Id = newId;
                    return CreatedAtRoute("GetExercise", new { id = newId }, exercise);
                }
            }
        }

        // 5. Code for editing an exercise
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] Exercise exercise)
        {
            using (SqlConnection conn = Connection)

            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())

                {
                    cmd.CommandText = @"UPDATE Exercise
                                        SET Name = @ExerciseName, Language = @ExerciseLanguage
                                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@ExerciseName", exercise.Name));
                    cmd.Parameters.Add(new SqlParameter("@ExerciseLanguage", exercise.Language));
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
                    cmd.CommandText = "DELETE FROM Exercise WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    cmd.ExecuteNonQuery();
                }
            }

        }
    }
}