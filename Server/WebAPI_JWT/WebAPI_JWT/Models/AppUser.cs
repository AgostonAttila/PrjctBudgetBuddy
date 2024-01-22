using Microsoft.AspNetCore.Identity;

namespace WebAPI_JWT.Models
{
    public class AppUser : IdentityUser
    {          

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
