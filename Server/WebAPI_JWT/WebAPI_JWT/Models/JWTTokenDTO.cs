namespace WebAPI_JWT.Models
{
    public class JWTTokenDTO
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public string RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
    }
}
