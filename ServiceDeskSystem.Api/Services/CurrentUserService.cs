using System.Security.Claims;

namespace ServiceDeskSystem.Api.Services;

public interface ICurrentUserService
{
    int? UserId { get; }
    string? UserRole { get; }
}

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public int? UserId
    {
        get
        {
            var idClaim = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim is not null && int.TryParse(idClaim.Value, out var id))
            {
                return id;
            }
            return null;
        }
    }

    public string? UserRole => httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;
}
