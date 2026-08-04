using System.Security.Claims;

namespace AssignmentSystem.Api.Services;

public class CurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int UserId
    {
        get
        {
            var idClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(idClaim ?? "0");
        }
    }

    public string? Role => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);
}