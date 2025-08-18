using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Evaluation.Models.Models.MyMoviesDB;

public partial class Movie
{
    [Key]
    public int Id { get; set; }

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

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedAt { get; set; }

    public int Status { get; set; }
}
