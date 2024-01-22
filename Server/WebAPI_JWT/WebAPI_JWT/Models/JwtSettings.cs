namespace WebAPI_JWT.Models
{
    public class JwtSettings
    {
        public string Key { get; set; }
        public int TokenExpirationInMinutes { get; set; }
        public string ValidIssuer { get; set; }
        public string ValidAudience { get; set; }
    }
}
