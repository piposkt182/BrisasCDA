namespace CDABrisasAPI.Dto
{
    public class SystemUserDto
    {
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public int RoleId { get; set; }
    }
}
