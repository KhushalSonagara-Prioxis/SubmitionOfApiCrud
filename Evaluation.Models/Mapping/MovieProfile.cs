using AutoMapper;
using Evaluation.Models.Models.MyMoviesDB;
using Evaluation.Models.RequestModel;
using Evaluation.Models.ResponseModel;

namespace Evaluation.Models.Mapping;

public class MovieProfile : Profile
{
    public MovieProfile()
    {
        CreateMap<MovieRequestModelWithoutSid, Movie>();
        CreateMap<Movie, MovieResponseModel>();
    }
}