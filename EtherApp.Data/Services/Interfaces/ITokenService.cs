using EtherApp.Data.Models;
using System.Security.Claims;

namespace EtherApp.Data.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(IEnumerable<Claim> claims);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        Task<string> GenerateJwtTokenAsync(User user);
    }
}