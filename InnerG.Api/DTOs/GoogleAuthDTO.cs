namespace InnerG.Api.DTOs
{
    public class GoogleAuthDTO
    {
        public class GoogleLoginRequest
        {
            public string IdToken { get; set; } = string.Empty;
        }
        public class GoogleUserInfo
        {
            public string Id { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string GivenName { get; set; } = string.Empty;
            public string FamilyName { get; set; } = string.Empty;
            public string Picture { get; set; } = string.Empty;
            public bool EmailVerified { get; set; }
        }
    }
}
