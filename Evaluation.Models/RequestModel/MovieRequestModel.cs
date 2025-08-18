using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Evaluation.Models.RequestModel;

public class MovieRequestModel
{
    [Column("MovieSID")]
    [StringLength(8)]
    [Unicode(false)]
    public string? MovieSid { get; set; }

    [StringLength(255)]
    public string Title { get; set; } = null!;

    [StringLength(100)]
    public string Genre { get; set; } = null!;

    [StringLength(150)]
    public string Director { get; set; } = null!;

    public DateOnly ReleaseDate { get; set; }

    [Column(TypeName = "decimal(3, 1)")]
    public decimal? Rating { get; set; }

    public string? Review { get; set; }
    
}