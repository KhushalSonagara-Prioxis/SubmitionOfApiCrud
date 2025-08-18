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
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace Evaluation.Service.Repository.Implementation;

public class MovieRepository : IMovieRepository
{
    private readonly MoviesDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<MovieRepository> _logger;
    private readonly MovieReviewSpContext  _spContext;
    private readonly IUnitOfWork _unitOfWork;

    public MovieRepository(MoviesDbContext context, IMapper mapper, ILogger<MovieRepository> logger, MovieReviewSpContext spContext, IUnitOfWork unitOfWork)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _spContext = spContext;
        _unitOfWork = unitOfWork;
    }
    
    
    public async Task<Page> List(Dictionary<string, object> parameters)
    {
        var xmlParam = CommonHelper.DictionaryToXml(parameters, "Search");
        string sqlQuery = "sp_SearchMoviesByXML {0}";
        object[] param = { xmlParam };
        var result = await _spContext.ExecutreStoreProcedureResultList(sqlQuery, param);
        return result;
    }
    
    public async Task<MovieResponseModel> GetMovieBySid(string sid)
    {
        string sqlQuery = "sp_GetMovieBySid {0}";       
        object[] param = { sid };
        var jsonResult = await _spContext.ExecuteStoreProcedure(sqlQuery, param);
        if (string.IsNullOrEmpty(jsonResult))
            return null;

        return JsonConvert.DeserializeObject<MovieResponseModel>(jsonResult);

    }
    
    public async Task<List<MovieResponseModel>> CreateMovies(IEnumerable<MovieRequestModelWithoutSid> moviesData)
    {
        try
        {
            var movieResponseList = new List<MovieResponseModel>();

            foreach (var data in moviesData)
            {
                var movie = _mapper.Map<Movie>(data);
                movie.MovieSid = "MOV" + Guid.NewGuid().ToString("N").Substring(0, 8);
                movie.Status = (int)Status.Active;
                movie.CreatedAt = DateTime.UtcNow;
                movie.ModifiedAt = DateTime.UtcNow;

                await _unitOfWork.GetRepository<Movie>().InsertAsync(movie);
                movieResponseList.Add(_mapper.Map<MovieResponseModel>(movie));
            }

            await _unitOfWork.CommitAsync();

            return movieResponseList;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw new HttpStatusCodeException(500, "Error inserting multiple movies");
        }
    }


    public async Task<MovieResponseModel> UpdateMovie (string sid,MovieRequestModelWithoutSid data)
    {
        var movie = await _unitOfWork.GetRepository<Movie>().SingleOrDefaultAsync(x =>
            x.MovieSid == sid && x.Status != (int)Status.Deleted);

        if (movie == null) return null;
    
        _mapper.Map(data, movie);
        movie.ModifiedAt = DateTime.UtcNow;

        
        _unitOfWork.GetRepository<Movie>().Update(movie);
        await _unitOfWork.CommitAsync();

        return _mapper.Map<MovieResponseModel>(movie);
    }
    

    public async Task<bool> DeleteMovie(string sid)
    {
            var doctors = await _unitOfWork.GetRepository<Movie>().GetAllAsync();
            var doctor = doctors
                .FirstOrDefault(u => u.MovieSid == sid && u.Status != (int)Status.Deleted);

            if (doctor == null) return false;

            doctor.Status = (int)Status.Deleted;
            doctor.ModifiedAt = DateTime.UtcNow;

            _context.Movies.Update(doctor);
            await _context.SaveChangesAsync();
            return true;
    }
    
    public async Task<int> DeleteMoviesByGenre(string genre)
    {
        var movies = await _unitOfWork.GetRepository<Movie>()
            .GetAllAsync(m => m.Genre == genre && m.Status != (int)Status.Deleted);

        if (!movies.Any())
            return 0;

        foreach (var movie in movies)
        {
            movie.Status = (int)Status.Deleted;
            movie.ModifiedAt = DateTime.UtcNow;
            _unitOfWork.GetRepository<Movie>().Update(movie);
        }

        await _unitOfWork.CommitAsync();
        return movies.Count(); 
    }
}