using Evaluation.Models.CommonModel;
using Evaluation.Models.RequestModel;
using Evaluation.Models.ResponseModel;

namespace Evaluation.Service.Repository.Interface;

public interface IMovieRepository
{
    
    Task<Page> List(Dictionary<string, object> parameters);
    
    Task<MovieResponseModel> GetMovieBySid(string sid);
    
    Task<List<MovieResponseModel>> CreateMovies(IEnumerable<MovieRequestModelWithoutSid> moviesData);
    
    Task<MovieResponseModel> UpdateMovie(string sid,MovieRequestModelWithoutSid data);
    
    Task<bool> DeleteMovie(string doctorSID);

    Task<int> DeleteMoviesByGenre(string genre);


}