namespace StaffManagement.ModelsDto
{
    public class WorkerBirthDateDto
    {
        public int Id { get; set; }
        public string? Avatar { get; set; }
        public string FullWorkerName { get; set; }
        public string BirthDayMonthAndDay { get; set; }
        public string DayOfWeekThisYear { get; set; }
        public DateTime NextBirthDay { get; set; } // Добавляем это свойство для сортировки
    }
}
