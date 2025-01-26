using UppbeatApi.Data.Models;

namespace UppbeatApi.Interfaces;

public interface ITrackRepository
{
    Task<Track> AddTrackAsync(Track track);
    Task<Track?> GetTrackByIdAsync(Guid id);
    Task<(IEnumerable<Track> Tracks, int TotalCount)> GetTracksAsync(string? genre = null, string? search = null, int page = 1, int pageSize = 10);
    Task UpdateTrackAsync(Guid id, Track track);
    Task DeleteTrackAsync(Guid id);
}
