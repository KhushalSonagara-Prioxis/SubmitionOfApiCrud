using AutoMapper;
using Evaluation.Common;
using Evaluation.Models.CommonModel;
using Evaluation.Models.Models.MyMoviesDB;
using Evaluation.Models.RequestModel;
using Evaluation.Models.ResponseModel;
using Evaluation.Models.SpDbContext;
using Evaluation.Service.Repository.Interface;
using Evaluation.Service.RepositoryFactory;
using Evaluation.Service.UnitOfWork;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Evaluation.Service.Repository.Implementation;

public class MovieRepository : IMovieRepository
{
    private readonly MoviesDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<MovieRepository> _logger;
    private readonly MovieReviewSpContext _spContext;
    private readonly IUnitOfWork _unitOfWork;

    public MovieRepository(
        MoviesDbContext context, 
        IMapper mapper, 
        ILogger<MovieRepository> logger, 
        MovieReviewSpContext spContext, 
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _spContext = spContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Page> List(Dictionary<string, object> parameters)
    {
        try
        {
            var xmlParam = CommonHelper.DictionaryToXml(parameters, "Search");
            string sqlQuery = "sp_SearchMoviesByXML {0}";
            object[] param = { xmlParam };
            var result = await _spContext.ExecutreStoreProcedureResultList(sqlQuery, param);

            if (result == null)
                throw new HttpStatusCodeException((int)StatusCode.BadRequest, "No movies found");

            return result;
        }
        catch (HttpStatusCodeException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving movies list");
            throw new HttpStatusCodeException((int)StatusCode.InternalServerError, "Error retrieving movies list");
        }
    }

    public async Task<MovieResponseModel> GetMovieBySid(string sid)
    {
        try
        {
            string sqlQuery = "sp_GetMovieBySid {0}";
            object[] param = { sid };
            var jsonResult = await _spContext.ExecuteStoreProcedure(sqlQuery, param);

            if (string.IsNullOrEmpty(jsonResult))
                throw new HttpStatusCodeException((int)StatusCode.BadRequest, $"Movie with SID {sid} not found");

            return JsonConvert.DeserializeObject<MovieResponseModel>(jsonResult);
        }
        catch (HttpStatusCodeException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving movie by SID");
            throw new HttpStatusCodeException((int)StatusCode.InternalServerError, "Error retrieving movie");
        }
    }

    public async Task<List<MovieResponseModel>> CreateMovies(IEnumerable<MovieRequestModelWithoutSid> moviesData)
    {
        try
        {
            if (moviesData == null || !moviesData.Any())
                throw new HttpStatusCodeException((int)StatusCode.BadRequest, "Invalid movie data provided");

            var movieResponseList = new List<MovieResponseModel>();

            foreach (var data in moviesData)
            {
                var existingMovies = await _unitOfWork
                    .GetRepository<Movie>()
                    .GetAllAsync(m => m.Title == data.Title && m.Status != (int)Status.Deleted);

                if (existingMovies.Any())
                {
                    throw new HttpStatusCodeException(
                        (int)StatusCode.BadRequest,
                        $"A movie with the title '{data.Title}' already exists.");
                }

                var movie = _mapper.Map<Movie>(data);
                movie.MovieSid = "MOV" + Guid.NewGuid().ToString().ToUpper();
                movie.Status = (int)Status.Active;
                movie.CreatedAt = DateTime.UtcNow;
                movie.ModifiedAt = DateTime.UtcNow;

                await _unitOfWork.GetRepository<Movie>().InsertAsync(movie);
                movieResponseList.Add(_mapper.Map<MovieResponseModel>(movie));
            }

            await _unitOfWork.CommitAsync();
            return movieResponseList;
        }
        catch (HttpStatusCodeException) { throw; }
        catch (Exception ex)
        {
            if (ex is DbUpdateException dbEx && dbEx.InnerException?.Message.Contains("UQ_Movies_Title") == true)
            {
                throw new HttpStatusCodeException(
                    (int)StatusCode.BadRequest,
                    "A movie with the same title already exists (DB constraint).");
            }

            _logger.LogError(ex, "Error inserting multiple movies");
            throw new HttpStatusCodeException((int)StatusCode.InternalServerError, "Error inserting movies");
        }
    }


    public async Task<MovieResponseModel> UpdateMovie(string sid, MovieRequestModelWithoutSid data)
    {
        try
        {
            var movie = await _unitOfWork.GetRepository<Movie>().SingleOrDefaultAsync(
                x => x.MovieSid == sid && x.Status != (int)Status.Deleted);

            if (movie == null)
                throw new HttpStatusCodeException((int)StatusCode.BadRequest, $"Movie with SID {sid} not found");

            _mapper.Map(data, movie);
            movie.ModifiedAt = DateTime.UtcNow;

            _unitOfWork.GetRepository<Movie>().Update(movie);
            await _unitOfWork.CommitAsync();

            return _mapper.Map<MovieResponseModel>(movie);
        }
        catch (HttpStatusCodeException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating movie");
            throw new HttpStatusCodeException((int)StatusCode.InternalServerError, "Error updating movie");
        }
    }

    public async Task<bool> DeleteMovie(string sid)
    {
        try
        {
            var movies = await _unitOfWork.GetRepository<Movie>().GetAllAsync();
            var movie = movies.FirstOrDefault(u => u.MovieSid == sid && u.Status != (int)Status.Deleted);

            if (movie == null)
                throw new HttpStatusCodeException((int)StatusCode.BadRequest, $"Movie with SID {sid} not found");

            movie.Status = (int)Status.Deleted;
            movie.ModifiedAt = DateTime.UtcNow;

            _context.Movies.Update(movie);
            await _context.SaveChangesAsync();
            return true;
            
        }
        catch (HttpStatusCodeException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting movie");
            throw new HttpStatusCodeException((int)StatusCode.InternalServerError, "Error deleting movie");
        }
    }

    public async Task<int> DeleteMoviesByGenre(string genre)
    {
        try
        {
            var movies = await _unitOfWork.GetRepository<Movie>()
                .GetAllAsync(m => m.Genre == genre && m.Status != (int)Status.Deleted);

            if (!movies.Any())
                throw new HttpStatusCodeException((int)StatusCode.BadRequest, $"No movies found with genre {genre}");

            foreach (var movie in movies)
            {
                movie.Status = (int)Status.Deleted;
                movie.ModifiedAt = DateTime.UtcNow;
                _unitOfWork.GetRepository<Movie>().Update(movie);
            }

            await _unitOfWork.CommitAsync();
            return movies.Count();
        }
        catch (HttpStatusCodeException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting movies by genre");
            throw new HttpStatusCodeException((int)StatusCode.InternalServerError, "Error deleting movies by genre");
        }
    }
}
