namespace StaffManagemet.ModelsDto
{
    public class UserRegistrationDto
    {
        public string Name { get; set; } = null!;
        public string Surname { get; set; } = null!;
        public string? Patronymic { get; set; }
        public string Login { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool? IsAdmin { get; set; }
    }
}
