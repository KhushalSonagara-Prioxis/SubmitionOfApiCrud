namespace Evaluation.Models.ResponseModel;

public class PaginatedResponse<T>
{
    public int TotalRecords { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
    public List<T> Data { get; set; }
}