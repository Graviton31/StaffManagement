namespace StaffManagementApi.ModelsDto
{
    public class WorkerRegistrationDto
    {
        public string WorkEmail { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string? Patronymic { get; set; }
        public DateOnly BirthDate { get; set; }
        public string? Phone { get; set; }
        public int? IdRole { get; set; }
    }
}
