
using Microsoft.AspNetCore.Mvc;
using Dapper;
using MovieAdmin.API.Data;

namespace MovieAdmin.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly DatabaseConnection _database;

        public MoviesController(DatabaseConnection database)
        {
            _database = database;
        }

        // Get all movies
        [HttpGet]
        public async Task<IActionResult> GetMovies()
        {
            using var connection = _database.CreateConnection();

            var movies = await connection.QueryAsync(
                "SELECT * FROM Movies"
            );

            return Ok(movies);
        }

        // Get one movie with its published reviews
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMovieById(int id)
        {
            using var connection = _database.CreateConnection();

            var movie = await connection.QueryFirstOrDefaultAsync(
                "SELECT * FROM Movies WHERE Id = @Id",
                new { Id = id }
            );

            if (movie == null)
            {
                return NotFound();
            }

            var reviews = await connection.QueryAsync(
                @"SELECT
                    r.Id,
                    r.MovieId,
                    r.Rating,
                    r.ReviewText,
                    r.CreatedAt,
                    u.Name AS CreatorName
                  FROM MovieReviews r
                  LEFT JOIN UserSnapshots u
                    ON r.ReviewerId = u.Auth0UserId
                  WHERE r.MovieId = @Id
                  AND r.IsPublished = 1",
                new { Id = id }
            );

            return Ok(new
            {
                Movie = movie,
                Reviews = reviews
            });
        }
    }
}
