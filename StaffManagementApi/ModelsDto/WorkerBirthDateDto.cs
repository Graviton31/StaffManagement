namespace StaffManagemet.ModelsDto
{
    public class WorkerBirthDateDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string BirthDayMonthAndDay { get; set; }
        public string DayOfWeekThisYear { get; set; }
        public DateTime NextBirthDay { get; set; } // Добавляем это свойство для сортировки
    }
}
