namespace StaffManagementApi.ModelsDto
{
    public class WorkingConditionDto
    {
        public int IdWorker { get; set; }
        public string Name { get; set; } = null!;
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? Description { get; set; }
    }
}
