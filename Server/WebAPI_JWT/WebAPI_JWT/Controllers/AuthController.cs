using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;
using System.Text;
using WebAPI_JWT.Models;
using WebAPI_JWT.Services;


namespace WebAPI_JWT.Controllers
{
    [Route("")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }


        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<JWTTokenDTO>> Login(LoginDto loginDto)
        {
            Result<JWTTokenDTO> result = await _authService.Login(loginDto);

            if (!result.IsSuccess) return Unauthorized(result.Error);

            if (!String.IsNullOrWhiteSpace(result.Value.RefreshToken))
                Response.Cookies.Append("refreshToken", result.Value.RefreshToken, new CookieOptions { HttpOnly = true, Expires = DateTime.UtcNow.AddDays(7) });

            return Ok(result.Value);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<string>> Register(RegisterDto registerDto)
        {
            Result<string> result = await _authService.Register(registerDto);

            if (!result.IsSuccess) return ValidationProblem(result.Error);

            return Ok(result.Value);
        }

        [AllowAnonymous]
        [HttpPost("confirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            Result<string> result = await _authService.VerifyEmail(token, email);

            if (!result.IsSuccess) return BadRequest("Could not verify email address");

            return Ok("Email confirmed - you can now login");
        }

        [AllowAnonymous]
        [HttpGet("resendConfirmationEmail")]
        public async Task<IActionResult> ResendConfirmationEmail(string email)
        {
            Result<string> result = await _authService.ResendConfirmationEmail(email);

            if (!result.IsSuccess)
                return Unauthorized();

            return Ok("Email verification link resent");
        }

        [Authorize]
        [HttpPost("refresh")]
        public async Task<ActionResult<JWTTokenDTO>> RefreshToken()
        {
            var cookieRefreshToken = Request.Cookies["refreshToken"];

            string userName = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Result<JWTTokenDTO> result = await _authService.RefreshToken(cookieRefreshToken, userName);

            //if (!String.IsNullOrWhiteSpace(result.Value.refresh_token))
            //    Response.Cookies.Append("refreshToken", result.Value.refresh_token, new CookieOptions { HttpOnly = true, Expires = DateTime.UtcNow.AddDays(7) });

            if (!result.IsSuccess)
                return Unauthorized();

            return Ok(result.Value);

        }




    }
}
