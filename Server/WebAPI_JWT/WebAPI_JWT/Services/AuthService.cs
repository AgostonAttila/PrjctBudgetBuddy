using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebAPI_JWT.Models;

namespace WebAPI_JWT.Services
{
    public interface IAuthService
    {
        Task<Result<JWTTokenDTO>> Login(LoginDto loginDto);
        Task<Result<string>> Register(RegisterDto registerDto);
        Task<Result<string>> ResendConfirmationEmail(string email);
        Task<Result<string>> VerifyEmail(string email, string token);
        Task<Result<JWTTokenDTO>> RefreshToken(string token, string email);
    }

    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _config;

        public AuthService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IConfiguration config)
        {
            _config = config;
            _signInManager = signInManager;
            _userManager = userManager;
        }


        public async Task<Result<JWTTokenDTO>> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null) return new Result<JWTTokenDTO> { Error = "Invalid email", IsSuccess = false };

            //if (!user.EmailConfirmed) new Result<JWTTokenDTO> { Error = "Email not confirmed", IsSuccess = false };

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (result.Succeeded)
            {
                await SetRefreshToken(user);

                JWTTokenDTO token = GenerateJWT(user);
                token.RefreshToken = user.RefreshTokens.FirstOrDefault()?.Token;

                return new Result<JWTTokenDTO> { Value = token, IsSuccess = true };
            }

            return new Result<JWTTokenDTO> { Error = "Unauthorized", IsSuccess = false };
        }

        public async Task<Result<string>> Register(RegisterDto registerDto)
        {


            if (await _userManager.FindByEmailAsync(registerDto.Email) != null)
            {
                return new Result<string> { IsSuccess = false, Message = "Email taken" };
            }
            //if (await _userManager.FindByNameAsync(registerDto.DisplayName) != null)
            //{            
            //    return new Result<string> { IsSuccess = false, Message = "Username taken" };
            //}

            var user = new AppUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
                return new Result<string> { IsSuccess = false, Message = "Problem registering user" };

            //var origin = Request.Headers["origin"];
            //var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            // token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            //var verifyUrl = $"{origin}/account/verifyEmail?token={token}&email={user.Email}";
            //var message = $"<p>Please click the below link to verify your email address:</p><p><a href='{verifyUrl}'>Click to verify email</a></p>";
            //await _emailSender.SendEmailAsync(user.Email, "Please verify email", message);

            return new Result<string> { IsSuccess = true, Value = "Registration success" };


        }

        public async Task<Result<string>> VerifyEmail(string email, string token)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) new Result<string> { IsSuccess = false, Message = "Unauthorized" };
            var decodedTokenBytes = WebEncoders.Base64UrlDecode(token);
            var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);
            await _userManager.ConfirmEmailAsync(user, decodedToken);
            return new Result<string> { IsSuccess = true };
        }

        public async Task<Result<string>> ResendConfirmationEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return new Result<string> { IsSuccess = false, Message = "Unauthorized" };
            }

            return new Result<string> { IsSuccess = true };

            //var origin = Request.Headers["origin"];
            //var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            //token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            //var verifyUrl = $"{origin}/account/verifyEmail?token={token}&email={user.Email}";
            //var message = $"<p>Please click the below link to verify your email address:</p><p><a href='{verifyUrl}'>Click to verify email</a></p>";

            //await _emailSender.SendEmailAsync(user.Email, "Please verify email", message);


        }

        public async Task<Result<JWTTokenDTO>> RefreshToken(string? refreshToken, string email)
        {
            var user = await _userManager.Users
                .Include(r => r.RefreshTokens)
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                return new Result<JWTTokenDTO> { IsSuccess = false, Message = "Unauthorized" };

            var oldToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken);

            if (oldToken != null && !oldToken.IsActive)
                return new Result<JWTTokenDTO> { IsSuccess = false, Message = "Unauthorized" };

            JWTTokenDTO jWTTokenDTO = GenerateJWT(user);
            SetRefreshToken(user);

            jWTTokenDTO.RefreshToken = user.RefreshTokens.FirstOrDefault()?.Token;

            return new Result<JWTTokenDTO> { IsSuccess = true, Value = jWTTokenDTO };
        }


        //HELPERS -> other file

        private async Task SetRefreshToken(AppUser user)
        {
            var refreshToken = GenerateRefreshToken(user);

            user.RefreshTokens.Add(refreshToken);
            await _userManager.UpdateAsync(user);

            //var cookieOptions = new CookieOptions
            //{
            //    HttpOnly = true,
            //    Expires = DateTime.UtcNow.AddDays(7)
            //};

            //Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);
        }

        public JWTTokenDTO GenerateJWT(AppUser user)
        {
            int minutes = 0;
            string token = GenerateSecurityToken(user, out minutes);

            return new JWTTokenDTO { AccessToken = token, ExpiresIn = minutes, TokenType = "JWT" };
        }

        public RefreshToken GenerateRefreshToken(AppUser user)
        {
            //var randomNumber = new byte[32];
            //using var rng = RandomNumberGenerator.Create();
            //rng.GetBytes(randomNumber);

            //here need to be different datas

            int minutes = 0;
            string token = GenerateSecurityToken(user, out minutes);

            return new RefreshToken { Token = token };
        }

        public string GenerateSecurityToken(AppUser user, out int minutes)
        {
            var rsaKey = RSA.Create();
            rsaKey.ImportRSAPrivateKey(File.ReadAllBytes("key"), out _);

            var jwtSettings = _config.GetSection("JwtSettings").Get<JwtSettings>();

            minutes = jwtSettings.TokenExpirationInMinutes;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.NormalizedEmail),
                new Claim(ClaimTypes.NameIdentifier,  user.Email),
            };


            var key = new RsaSecurityKey(rsaKey);
            var creds = new SigningCredentials(key, SecurityAlgorithms.RsaSha512);


            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = jwtSettings.ValidIssuer,
                Audience = jwtSettings.ValidAudience,
                Expires = DateTime.Now.AddMinutes(jwtSettings.TokenExpirationInMinutes),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);

        }

    }
}
