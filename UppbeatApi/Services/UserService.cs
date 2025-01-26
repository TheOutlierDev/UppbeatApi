using Dapper;
using UppbeatApi.Data.Models;
using UppbeatApi.Interfaces;

namespace UppbeatApi.Services;

public class UserService : IUserService
{
    private readonly IConnectionFactory _connectionFactory;

    public UserService(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<User?> GetUserAsync(Guid userId)
    {
        using var connection = _connectionFactory.Create();
        var result = await connection.QueryAsync<dynamic>(
            "SELECT * FROM sp_get_user_by_id(@p_user_id)",
            new { p_user_id = userId }
        );

        var row = result.FirstOrDefault();
        if (row == null)
            return null;

        return new User
        {
            Id = row.Id,
            Name = row.Name,
            Role = row.Role
        };
    }
}
