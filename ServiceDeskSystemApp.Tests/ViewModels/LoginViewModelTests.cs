using FluentAssertions;
using Moq;
using ServiceDeskSystemApp.Models.Auth;
using ServiceDeskSystemApp.Services;
using ServiceDeskSystemApp.ViewModels;

namespace ServiceDeskSystemApp.Tests.ViewModels;

public class LoginViewModelTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly LoginViewModel _sut;

    public LoginViewModelTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _sut = new LoginViewModel(_authServiceMock.Object);
    }

    [Fact]
    public async Task LoginCommand_WithValidCredentials_CallsAuthService()
    {
        // Arrange
        _sut.Username = "user1";
        _sut.Password = "pass1";
        
        _authServiceMock.Setup(s => s.LoginAsync("user1", "pass1"))
            .ReturnsAsync(new LoginResponse { Token = "token", User = new UserDto { Id = 1, FirstName = "Name", LastName = "Name" } });

        // Act
        await _sut.LoginCommand.ExecuteAsync(null);

        // Assert
        _authServiceMock.Verify(s => s.LoginAsync("user1", "pass1"), Times.Once);
        _sut.IsLoading.Should().BeFalse();
        _sut.ErrorMessage.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task LoginCommand_WithInvalidCredentials_SetsErrorMessage()
    {
        // Arrange
        _sut.Username = "user1";
        _sut.Password = "pass1";
        
        _authServiceMock.Setup(s => s.LoginAsync("user1", "pass1"))
            .ReturnsAsync((LoginResponse?)null);

        // Act
        await _sut.LoginCommand.ExecuteAsync(null);

        // Assert
        _authServiceMock.Verify(s => s.LoginAsync("user1", "pass1"), Times.Once);
        _sut.IsLoading.Should().BeFalse();
        _sut.ErrorMessage.Should().NotBeNullOrEmpty();
    }
}
