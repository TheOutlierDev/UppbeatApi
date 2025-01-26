using Dapper;
using Npgsql;
using System.Data;
using UppbeatApi.Handlers;
using UppbeatApi.Interfaces;

namespace UppbeatApi.Data;

public class ConnectionFactory : IConnectionFactory
{
    private readonly string _connectionString;

    public ConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException(nameof(configuration));
    }

    public IDbConnection Create()
    {
        var conn = new NpgsqlConnection(_connectionString);

        SqlMapper.AddTypeHandler(new StringArrayToListHandler());

        return conn;
    }
}
