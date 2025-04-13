namespace StaffManagementApi.ModelsDto
{
    /// <summary>
    /// DTO для обновления условий работы (без обязательных полей)
    /// </summary>
    public class WorkingConditionUpdateDto
    {
        public string? Name { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? Description { get; set; }
    }
}
