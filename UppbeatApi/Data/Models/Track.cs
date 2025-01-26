using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UppbeatApi.Data.Models;

public class Track
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Name { get; set; } = string.Empty;

    [Required]
    public Guid ArtistId { get; set; }

    [Required]
    public double Duration { get; set; }

    [Required]
    public required string File { get; set; } = string.Empty;

    [Required]
    public required List<string> Genres { get; set; } = [];

    [ForeignKey("Artist")]
    public User? Artist { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
