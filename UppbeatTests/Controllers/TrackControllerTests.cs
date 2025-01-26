using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using UppbeatApi.Controllers;
using UppbeatApi.Data.Models;
using UppbeatApi.Interfaces;

namespace UppbeatTests.Controllers;

public class TrackControllerTests
{
    private readonly Mock<ITrackRepository> _mockTrackRepository;
    private readonly TrackController _controller;

    public TrackControllerTests()
    {
        _mockTrackRepository = new Mock<ITrackRepository>();
        _controller = new TrackController(_mockTrackRepository.Object);

        // Setup ClaimsPrincipal for authorization tests
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Role, "Artist")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        // Setup ControllerContext
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };
    }

    [Fact]
    public async Task GetTracks_ReturnsOkResult_WithTracks()
    {
        // Arrange
        var expectedTracks = new List<Track>
        {
            new Track
            {
                Id = Guid.NewGuid(),
                Name = "Test Track",
                Artist = new User
                {
                    Id = Guid.NewGuid(),
                    Name = "Test Artist",
                    Role = "Artist"
                },
                Genres = new List<string>{ "Rock" },
                Duration = 180,
                File = "test.mp3"
            }
        };

        _mockTrackRepository.Setup(repo => repo.GetTracksAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<int>()))
            .ReturnsAsync((expectedTracks, 1));

        // Act
        var result = await _controller.GetTracks();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedTracks = Assert.IsAssignableFrom<IEnumerable<Track>>(okResult.Value);
        Assert.Single(returnedTracks);
        Assert.Equal(expectedTracks.First().Name, returnedTracks.First().Name);
        Assert.Equal(expectedTracks.First().Artist, returnedTracks.First().Artist);
    }

    [Fact]
    public async Task AddTrack_ValidTrack_ReturnsCreatedResult()
    {
        // Arrange
        var newTrack = new Track
        {
            Name = "New Track",
            Artist = new User
            {
                Id = Guid.NewGuid(),
                Name = "Test Artist",
                Role = "Artist"
            },
            Genres = new List<string> { "Pop" },
            Duration = 240,
            File = "newtrack.mp3"
        };

        var createdTrack = new Track
        {
            Id = Guid.NewGuid(),
            Name = newTrack.Name,
            Artist = newTrack.Artist,
            Genres = newTrack.Genres,
            Duration = newTrack.Duration,
            File = newTrack.File
        };

        _mockTrackRepository.Setup(repo => repo.AddTrackAsync(It.IsAny<Track>()))
            .ReturnsAsync(createdTrack);

        // Act
        var result = await _controller.AddTrack(newTrack);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, createdResult.StatusCode);

        var returnedTrack = Assert.IsType<Track>(createdResult.Value);
        Assert.Equal(newTrack.Name, returnedTrack.Name);
        Assert.Equal(newTrack.Artist, returnedTrack.Artist);
        Assert.NotEqual(Guid.Empty, returnedTrack.Id);
    }
}
