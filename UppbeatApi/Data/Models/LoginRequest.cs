using System.ComponentModel.DataAnnotations;

namespace UppbeatApi.Data.Models;

public class LoginRequest
{
    [Required]
    public Guid UserId { get; set; }
}