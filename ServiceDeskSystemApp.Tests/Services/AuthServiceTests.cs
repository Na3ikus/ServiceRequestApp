using FluentAssertions;
using Moq;
using ServiceDeskSystemApp.Models.Auth;
using ServiceDeskSystemApp.Services;

namespace ServiceDeskSystemApp.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<ApiService> _apiServiceMock;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        // Mocking ApiService requires a real HttpClient in the base constructor, 
        // but since we made methods virtual, we can pass null or a dummy.
        _apiServiceMock = new Mock<ApiService>(new HttpClient());
        _sut = new AuthService(_apiServiceMock.Object);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsLoginResponse()
    {
        // Arrange
        var username = "testuser";
        var password = "password123";
        var expectedResponse = new LoginResponse 
        { 
            Token = "fake-jwt-token", 
            User = new UserDto { Id = 1, FirstName = "Test", LastName = "User" } 
        };

        _apiServiceMock.Setup(api => api.PostAsync<LoginRequest, LoginResponse>(
                "/api/auth/login", 
                It.Is<LoginRequest>(r => r.Username == username && r.Password == password)))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _sut.LoginAsync(username, password);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().Be("fake-jwt-token");
        _apiServiceMock.Verify(api => api.SetAuthTokenAsync("fake-jwt-token"), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidCredentials_ReturnsNull()
    {
        // Arrange
        _apiServiceMock.Setup(api => api.PostAsync<LoginRequest, LoginResponse>(
                "/api/auth/login", 
                It.IsAny<LoginRequest>()))
            .ReturnsAsync((LoginResponse?)null);

        // Act
        var result = await _sut.LoginAsync("wrong", "wrong");

        // Assert
        result.Should().BeNull();
        _apiServiceMock.Verify(api => api.SetAuthTokenAsync(It.IsAny<string>()), Times.Never);
    }
}
