using Dapper;
using System.Data;
using UppbeatApi.Data.Models;
using UppbeatApi.Interfaces;

namespace UppbeatApi.Data.Repositories;

public class TrackRepository : ITrackRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public TrackRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Track> AddTrackAsync(Track track)
    {
        using var connection = _connectionFactory.Create();
        var parameters = new DynamicParameters();

        parameters.Add("p_name", track.Name);
        parameters.Add("p_artist_id", track.ArtistId);
        parameters.Add("p_duration", track.Duration);
        parameters.Add("p_file_path", track.File);
        parameters.Add("p_genres", track.Genres.ToArray());
        parameters.Add("p_track_id", dbType: DbType.Guid, direction: ParameterDirection.InputOutput);

        await connection.ExecuteAsync(
            "CALL sp_add_track(@p_name, @p_artist_id, @p_duration, @p_file_path, @p_genres, @p_track_id)",
            parameters,
            commandType: CommandType.Text);

        track.Id = parameters.Get<Guid>("p_track_id");
        return track;
    }

    public async Task<Track?> GetTrackByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.Create();
        var result = await connection.QueryAsync<dynamic>(
            "SELECT * FROM sp_get_track_by_id(@p_track_id)",
            new { p_track_id = id }
        );

        var row = result.FirstOrDefault();
        if (row == null)
            return null;

        return new Track
        {
            Id = row.Id,
            Name = row.Name,
            ArtistId = row.ArtistId,
            Duration = row.Duration,
            File = row.File,
            Genres = ((string[])row.Genres).ToList(),
            CreatedAt = row.CreatedAt,
            UpdatedAt = row.UpdatedAt,
            Artist = new User
            {
                Id = Guid.Parse(((IDictionary<string, object>)row)["Artist.Id"].ToString()!),
                Name = ((IDictionary<string, object>)row)["Artist.Name"].ToString()!,
                Role = ((IDictionary<string, object>)row)["Artist.Role"].ToString()!
            }
        };
    }

    public async Task<(IEnumerable<Track> Tracks, int TotalCount)> GetTracksAsync(
        string? genre = null,
        string? search = null,
        int page = 1,
        int pageSize = 10)
    {
        using var connection = _connectionFactory.Create();
        var parameters = new
        {
            p_genre = genre,
            p_search = search,
            p_page = page,
            p_page_size = pageSize
        };

        var result = await connection.QueryAsync<dynamic>(
            "SELECT * FROM sp_get_tracks(@p_genre, @p_search, @p_page, @p_page_size)",
            parameters
        );

        var tracks = result.Select(row => new Track
        {
            Id = row.Id,
            Name = row.Name,
            ArtistId = row.ArtistId,
            Duration = row.Duration,
            File = row.File,
            Genres = ((string[])row.Genres).ToList(),
            CreatedAt = row.CreatedAt,
            UpdatedAt = row.UpdatedAt,
            Artist = new User
            {
                Id = Guid.Parse(((IDictionary<string, object>)row)["Artist.Id"].ToString()!),
                Name = ((IDictionary<string, object>)row)["Artist.Name"].ToString()!,
                Role = ((IDictionary<string, object>)row)["Artist.Role"].ToString()!
            }
        }).ToList();

        return (tracks, tracks.Count());
    }

    public async Task UpdateTrackAsync(Guid id, Track track)
    {
        using var connection = _connectionFactory.Create();
        var parameters = new
        {
            p_track_id = id,
            p_name = track.Name,
            p_duration = track.Duration,
            p_file_path = track.File,
            p_genres = track.Genres.ToArray()
        };

        await connection.ExecuteAsync(
            "CALL sp_update_track(@p_track_id, @p_name, @p_duration, @p_file_path, @p_genres)",
            parameters,
            commandType: CommandType.Text);
    }

    public async Task DeleteTrackAsync(Guid id)
    {
        using var connection = _connectionFactory.Create();
        await connection.ExecuteAsync(
            "CALL sp_delete_track(@p_track_id)",
            new { p_track_id = id },
            commandType: CommandType.Text);
    }
}
