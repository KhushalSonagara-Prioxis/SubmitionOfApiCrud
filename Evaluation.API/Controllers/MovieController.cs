using Evaluation.Model.CommonModel;
using Evaluation.Models.Models.MyMoviesDB;
using Evaluation.Models.RequestModel;
using Evaluation.Models.ResponseModel;
using Evaluation.Service.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace Evaluation.API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class MovieController : BaseController
{
    private readonly IMovieRepository _movieRepository;
    private readonly ILogger<MovieController> _logger;

    public MovieController(IMovieRepository movieRepository, ILogger<MovieController> logger)
    {
        _movieRepository = movieRepository;
        _logger = logger;
    }
    
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MovieResponseModel>>> GetAllMovies([FromQuery] SearchRequestModel model)
    {
        try
        {
            _logger.LogInformation("==========Get all Movies=========");
            var paramaters = FillParamesFromModel(model);
            _logger.LogInformation("Get all Movies with search {paramaters}", paramaters);
        
            var list = await _movieRepository.List(paramaters);
            if (list != null)
            {
                var result = JsonConvert.DeserializeObject<List<MovieResponseModel>>(list.Result?.ToString() ?? "[]") ?? [];
                list.Result = result;
                return Ok(BindSearchResult(list,model,"Movie List"));
            }
            _logger.LogInformation("========No Movies with search {paramaters}", paramaters);
            return NoContent();

        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            Console.WriteLine(e);
            throw;
        }
    }
    
    [HttpGet("{sid}")]
    public async Task<ActionResult<MovieResponseModel>> GetMovieBySID([FromRoute]string sid)
    {
        try
        {
            _logger.LogInformation("GetMovieBySID {sid}",sid);
            var movie = await _movieRepository.GetMovieBySid(sid);
            if (movie == null)
            {
                _logger.LogInformation("Movie Not found with {sid} sid", sid);
                return NotFound(new { message = $"Movie with SID '{sid}' not found" });
            }
            return Ok(movie);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "GetMovieBySID {sid}", sid);
            Console.WriteLine(e);
            throw;
        }
        
    }

    [HttpPost]
    public async Task<ActionResult<MovieResponseModel>> CreateMovie(List<MovieRequestModelWithoutSid> data)
    {
        try
        {
            _logger.LogInformation("==========Create movie=========");
            var movie = await _movieRepository.CreateMovies(data);
            _logger.LogInformation("Movie Created");
            return Ok(movie);

        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            Console.WriteLine(e);
            throw;
        }
    }
    
    [HttpPost("{sid}")]
    public async Task<ActionResult<MovieResponseModel>> UpdateMovie([FromRoute]string sid,[FromBody]MovieRequestModelWithoutSid data)
    {
        try
        {
            _logger.LogInformation("Updating movie with id {sid}", sid);
            var movie = await _movieRepository.UpdateMovie(sid, data);
            if (movie == null)
            {
                _logger.LogInformation("Movie with id {sid} not found", sid);
                return NotFound(new { message = $"Movie with SID '{sid}' not found" });
            }
            return Ok(movie);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpDelete("{sid}")]
    public async Task<ActionResult<bool>> DeleteMovie([FromRoute] string sid)
    {
        try
        {
            _logger.LogInformation("Delete Movie called {sid}",sid);
            bool result = await _movieRepository.DeleteMovie(sid);
            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpDelete("DeleteByGenre/{genre}")]
    public async Task<ActionResult<int>> DeleteMoviesByGenre([FromRoute] string genre)
    {
        try
        {
            _logger.LogInformation("Delete Movies called with {genre}",genre);
            int result = await _movieRepository.DeleteMoviesByGenre(genre);
            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            Console.WriteLine(e);
            throw;
        }
    }
}