namespace StaffManagemet.ModelsDto
{
    public class WorkerDto
    {
        public int IdWorker { get; set; }

        public string? Name { get; set; } = null!;

        public string? Surname { get; set; } = null!;

        public string? Patronymic { get; set; }

        public DateOnly? BirthDate { get; set; }

        public int IdRole { get; set; }

        public string? WorkEmail { get; set; }

        public string? Phone { get; set; }

        public string? PcNumber { get; set; }
    }
}