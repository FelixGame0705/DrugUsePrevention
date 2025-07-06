namespace DrugUserPreventionUI.Models.Users
{
    public class UserDTO
    {
        public int UserID { get; set; }
        public string UserName { get; set; } = "";
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
    }
}
