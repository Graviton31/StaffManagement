namespace StaffManagementApi.ModelsDto
{
    public class WeekDataDto<T>
    {
        public int WeekNumber { get; set; }
        public string WeekRange { get; set; }
        public List<T> Workers { get; set; }
    }
}
