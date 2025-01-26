using System.Data;

namespace UppbeatApi.Interfaces;

public interface IConnectionFactory
{
    IDbConnection Create();
}
