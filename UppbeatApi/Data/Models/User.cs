using System.ComponentModel.DataAnnotations;

namespace UppbeatApi.Data.Models;

public class User
{
    public Guid Id { get; set; }

    [Required]
    public required string Name { get; set; }

    [Required]
    public required string Role { get; set; } // Role can be 'SignedOut', 'Regular', or 'Artist'
}
