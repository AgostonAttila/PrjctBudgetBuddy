namespace blazorapp.Models
{
    public class UserInfo
    {
        public string? DisplayName { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public string? Username { get; set; }

        public Dictionary<string, string> Claims { get; set; }
    }
}
