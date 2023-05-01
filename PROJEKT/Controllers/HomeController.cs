using FINALNIPROJEKTBOOP.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.Data.SqlClient;

namespace FINALNIPROJEKTBOOP.Controllers
{
    public class HomeController : Controller
    {
        List<IndexView> indexViews = new List<IndexView>();
        IList<Movie> movies = new List<Movie>();
        IList<Actor> actors = new List<Actor>();
        IList<Director> directors = new List<Director>();
        private readonly ILogger<HomeController> _logger;
        private string _connectionString = "Data Source = (localdb)\\MSSQLLocalDB; Initial Catalog = DatabazeFilmu; Integrated Security = True; Connect Timeout = 30; Encrypt = False; Trust Server Certificate = False; Application Intent = ReadWrite; Multi Subnet Failover = False";
        
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        [HttpGet]
        [ActionName("Index")]
        public IActionResult IndexGet()
        {
            // connect to the SQL server using the predefined connection string
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT MOVIES.NameOfMovie, MOVIES.ReleaseDate, MOVIES.Genre, STUFF((SELECT ', ' + RTRIM(LTRIM(Actors.Surname)) + ' ' + RTRIM(LTRIM(Actors.FirstName)) FROM ConActorIdMovieId INNER JOIN Actors ON ConActorIdMovieId.ActorId = Actors.ActorId WHERE ConActorIdMovieId.MovieId = MOVIES.MovieId FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') as Actors, STUFF((SELECT ', ' + RTRIM(LTRIM(Directors.Surname)) + ' ' + RTRIM(LTRIM(Directors.Firstname)) FROM ConDirectorIdMovieId INNER JOIN Directors ON ConDirectorIdMovieId.DirectorId = Directors.DirectorId WHERE ConDirectorIdMovieId.MovieId = MOVIES.MovieId FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') as Directors FROM MOVIES", connection);
                connection.Open();
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var movie = new IndexView
                    {
                        NameOfMovie = reader.GetString(0),
                        ReleaseDate = reader.GetInt32(1),
                        Genre = reader.GetString(2),
                        Actors = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        Directors = reader.IsDBNull(4) ? "" : reader.GetString(4)
                    };
                    indexViews.Add(movie);
                }
            }
            return View(indexViews);
        }

        [HttpGet]
        [ActionName("AddMovie")]
        public IActionResult AddMovieGet() {
            return View();
        }

        [HttpPost]
        [ActionName("AddMovie")]
        public IActionResult AddMoviePost(string NameOfMovie, string Genre, int ReleaseDate)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "INSERT INTO MOVIES (NameOfMovie, Genre, ReleaseDate) VALUES (@NameOfMovie, @Genre, @ReleaseDate)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@NameOfMovie", NameOfMovie);
                    command.Parameters.AddWithValue("@Genre", Genre);
                    command.Parameters.AddWithValue("@ReleaseDate", ReleaseDate);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Index");
        }


        [HttpGet]
        [ActionName("AddActor")]
        public IActionResult AddActorGet()
        {
            return View();
        }

        [HttpPost]
        [ActionName("AddActor")]
        public async Task<IActionResult> AddActorPost(string NameOfMovie, string Firstname, string Surname)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Get the MovieId for the specified movie name
                var getMovieIdCommand = new SqlCommand("SELECT MovieId FROM MOVIES WHERE NameOfMovie = @NameOfMovie", connection);
                getMovieIdCommand.Parameters.AddWithValue("@NameOfMovie", NameOfMovie);
                var movieId = await getMovieIdCommand.ExecuteScalarAsync() as int?;

                if (!movieId.HasValue)
                {
                    return NotFound();
                }

                // Insert the actor into the Actors table
                var insertActorCommand = new SqlCommand("INSERT INTO Actors (Firstname, Surname) VALUES (@Firstname, @Surname)", connection);
                insertActorCommand.Parameters.AddWithValue("@Firstname", Firstname);
                insertActorCommand.Parameters.AddWithValue("@Surname", Surname);
                await insertActorCommand.ExecuteNonQueryAsync();

                // Get the ActorId for the newly inserted actor
                var getActorIdCommand = new SqlCommand("SELECT ActorId FROM Actors WHERE Firstname = @Firstname AND Surname = @Surname", connection);
                getActorIdCommand.Parameters.AddWithValue("@Firstname", Firstname);
                getActorIdCommand.Parameters.AddWithValue("@Surname", Surname);
                var actorId = await getActorIdCommand.ExecuteScalarAsync() as int?;

                if (!actorId.HasValue)
                {
                    return BadRequest();
                }

                // Insert the ConActorIdMovieId record linking the actor to the movie
                var insertConActorIdMovieIdCommand = new SqlCommand("INSERT INTO ConActorIdMovieId (ActorId, MovieId) VALUES (@ActorId, @MovieId)", connection);
                insertConActorIdMovieIdCommand.Parameters.AddWithValue("@ActorId", actorId.Value);
                insertConActorIdMovieIdCommand.Parameters.AddWithValue("@MovieId", movieId.Value);
                await insertConActorIdMovieIdCommand.ExecuteNonQueryAsync();

                return RedirectToAction("Index");
            }

        }


        [HttpGet]
        [ActionName("AddDirector")]
        public IActionResult AddDirectorGet()
        {
            return View();
        }

        [HttpPost]
        [ActionName("AddDirector")]
        public async Task<IActionResult> AddDirectorPost(string NameOfMovie, string Firstname, string Surname)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Get the MovieId for the specified movie name
                var getMovieIdCommand = new SqlCommand("SELECT MovieId FROM MOVIES WHERE NameOfMovie = @NameOfMovie", connection);
                getMovieIdCommand.Parameters.AddWithValue("@NameOfMovie", NameOfMovie);
                var movieId = await getMovieIdCommand.ExecuteScalarAsync() as int?;

                if (!movieId.HasValue)
                {
                    return NotFound();
                }

                // Insert the actor into the Actors table
                var insertDirectorCommand = new SqlCommand("INSERT INTO Directors (FirstName, Surname) VALUES (@Firstname, @Surname)", connection);
                insertDirectorCommand.Parameters.AddWithValue("@Firstname", Firstname);
                insertDirectorCommand.Parameters.AddWithValue("@Surname", Surname);
                await insertDirectorCommand.ExecuteNonQueryAsync();

                // Get the ActorId for the newly inserted actor
                var getDirectorIdCommand = new SqlCommand("SELECT DirectorId FROM Directors WHERE FirstName = @Firstname AND Surname = @Surname", connection);
                getDirectorIdCommand.Parameters.AddWithValue("@Firstname", Firstname);
                getDirectorIdCommand.Parameters.AddWithValue("@Surname", Surname);
                var directorId = await getDirectorIdCommand.ExecuteScalarAsync() as int?;

                if (!directorId.HasValue)
                {
                    return BadRequest();
                }

                // Insert the ConActorIdMovieId record linking the actor to the movie
                var insertConDirectorIdMovieId = new SqlCommand("INSERT INTO ConDirectorIdMovieId (DirectorId, MovieId) VALUES (@DirectorId, @MovieId)", connection);
                insertConDirectorIdMovieId.Parameters.AddWithValue("@DirectorId", directorId.Value);
                insertConDirectorIdMovieId.Parameters.AddWithValue("@MovieId", movieId.Value);
                await insertConDirectorIdMovieId.ExecuteNonQueryAsync();

                return RedirectToAction("Index");
            }
        }




        [HttpGet]
        [ActionName("EditMovie")]
        public IActionResult EditMovieGet() {
            // connect to the SQL server using the predefined connection string
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT * FROM dbo.MOVIES", connection);
                connection.Open();
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var movie = new Movie
                    {
                        NameOfMovie = reader.GetString(0),
                        ReleaseDate = reader.GetInt32(1),
                        Genre = reader.GetString(2),
                        Id = reader.GetInt32(3)
                    };
                    movies.Add(movie);
                }
            }
            return View(movies);
        }
        [HttpPost]
        [ActionName("EditMovie")]
        public IActionResult EditMoviePost(IList<Movie> model, string submitButton)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                SqlCommand command;
                connection.Open();
                switch(submitButton) {
                    case "0":
                    foreach (var movie in model)
                    {
                        command = new SqlCommand("UPDATE dbo.MOVIES SET NameOfMovie = @NameOfMovie, Genre = @Genre, ReleaseDate = @ReleaseDate WHERE MovieId = @Id", connection);
                        command.Parameters.AddWithValue("@Id", movie.Id);
                        command.Parameters.AddWithValue("@NameOfMovie", movie.NameOfMovie);
                        command.Parameters.AddWithValue("@Genre", movie.Genre);
                        command.Parameters.AddWithValue("@ReleaseDate", movie.ReleaseDate);

                        command.ExecuteNonQuery();
                    }
                    break;

                    default:
                        command = new SqlCommand("DELETE FROM MOVIES WHERE MovieId = @Id", connection);
                        command.Parameters.AddWithValue("@Id", submitButton);
                        command.ExecuteNonQuery();
                    break;
                }

                
            }
            return RedirectToAction("Index");
        }


        [HttpGet]
        [ActionName("EditActor")]
        public IActionResult EditActorGet()
        {
            // connect to the SQL server using the predefined connection string
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT * FROM Actors", connection);
                connection.Open();
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var actor = new Actor
                    {
                        FirstName = reader.GetString(0),
                        Surname = reader.GetString(1),
                        Id = reader.GetInt32(2),
                    };
                    actors.Add(actor);
                }
            }
            return View(actors);
        }

        [HttpPost]
        [ActionName("EditActor")]
        public IActionResult EditActorPost(IList<Actor> model, string submitButton)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                SqlCommand command;
                connection.Open();
                switch (submitButton)
                {
                    case "0":
                        foreach (var actor in model)
                        {
                            command = new SqlCommand("UPDATE Actors SET FirstName = @FirstName, Surname = @Surname WHERE ActorId = @Id", connection);
                            command.Parameters.AddWithValue("@Id", actor.Id);
                            command.Parameters.AddWithValue("@FirstName", actor.FirstName);
                            command.Parameters.AddWithValue("@Surname", actor.Surname);
                            command.ExecuteNonQuery();
                        }
                        break;

                    default:

                        command = new SqlCommand("DELETE FROM ConActorIdMovieId WHERE ActorId = @Id", connection);
                        command.Parameters.AddWithValue("@Id", submitButton);
                        command.ExecuteNonQuery();
                        command = new SqlCommand("DELETE FROM Actors WHERE ActorId = @Id", connection);
                        command.Parameters.AddWithValue("@Id", submitButton);
                        command.ExecuteNonQuery();
                        break;
                }


            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        [ActionName("EditDirector")]
        public IActionResult EditDirectorGet()
        {
            // connect to the SQL server using the predefined connection string
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT * FROM Directors", connection);
                connection.Open();
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var director = new Director
                    {
                        FirstName = reader.GetString(0),
                        Surname = reader.GetString(1),
                        Id = reader.GetInt32(2),
                    };
                    directors.Add(director);
                }
            }
            return View(directors);
        }



        [HttpPost]
        [ActionName("EditDirector")]
        public IActionResult EditMoviePost(IList<Director> model, string submitButton)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                SqlCommand command;
                connection.Open();
                switch (submitButton)
                {
                    case "0":
                        foreach (var director in model)
                        {
                            command = new SqlCommand("UPDATE Directors SET Firstname = @FirstName, Surname = @Surname WHERE directorId = @Id", connection);
                            command.Parameters.AddWithValue("@Id", director.Id);
                            command.Parameters.AddWithValue("@FirstName", director.FirstName);
                            command.Parameters.AddWithValue("@Surname", director.Surname);
                            command.ExecuteNonQuery();
                        }
                        break;

                    default:
                        command = new SqlCommand("DELETE FROM ConDirectorIdMovieId WHERE directorId = @Id", connection);
                        command.Parameters.AddWithValue("@Id", submitButton);
                        command.ExecuteNonQuery();
                        command = new SqlCommand("DELETE FROM directors WHERE directorId = @Id", connection);
                        command.Parameters.AddWithValue("@Id", submitButton);
                        command.ExecuteNonQuery();
                        break;
                }


            }
            return RedirectToAction("Index");
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}