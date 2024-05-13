using Microsoft.AspNetCore.Identity;

namespace REST_API_TEMPLATE.Services
{
    public interface ITokenRepository
    {
        string CreateJWTToken(IdentityUser user, List<string> roles);

    }
}
