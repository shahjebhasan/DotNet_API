namespace RedMangoAPI.Models.DTO
{
    public class RegisterRequestDTO
    {
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }

    public static class UserRole
    {
        public const string Role_Admin = "admin";
        public const string Role_Customer = "customer";
    }
}
