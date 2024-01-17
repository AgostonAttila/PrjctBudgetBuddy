using System.Security.Principal;
using blazorapp.Models;
using blazorapp.Models.DTO;

namespace blazorapp.Services.Account
{
    public interface IAccountService
    {
        Task<Result<string>> Register(RegisterDTO user);
        Task<Result<TokenDTO>> Login(LoginDTO user);
        Task<string> Refresh();
        Task<Result<string>> Logout();
        Task<Result<string>> ConfirmEmail(string token, string email);
        Task<Result<string>> ResendConfirmationEmail(string email);
        Task<Result<string>> ForgotPassword(string email);
        Task<Result<string>> ResetPassword(string email);     
        Task<IIdentity> GetCurrentUser();      
    }
}
