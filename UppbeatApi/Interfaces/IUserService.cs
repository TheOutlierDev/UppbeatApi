using UppbeatApi.Data.Models;

namespace UppbeatApi.Interfaces;
public interface IUserService
{
    Task<User?> GetUserAsync(Guid userId);
}
