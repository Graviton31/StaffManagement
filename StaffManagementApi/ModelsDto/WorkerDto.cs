using System.ComponentModel.DataAnnotations;

namespace StaffManagementApi.ModelsDto
{
    public class WorkerDto
    {
        public int IdWorker { get; set; }

        [Required(ErrorMessage = "Обязательное поле")]
        [Display(Name = "Имя")]
        public string? Name { get; set; } = null!;

        [Required(ErrorMessage = "Обязательное поле")]
        [Display(Name = "Фамилия")]
        public string? Surname { get; set; } = null!;

        [Required(ErrorMessage = "Обязательное поле")]
        [Display(Name = "Отчество")]
        public string? Patronymic { get; set; }

        [Required(ErrorMessage = "Обязательное поле")]
        [Display(Name = "День рождения")]
        public DateOnly? BirthDate { get; set; }

        [Required(ErrorMessage = "Обязательное поле")]
        [Display(Name = "Роль")]
        public int? IdRole { get; set; }

        [Required(ErrorMessage = "Обязательное поле")]
        [Display(Name = "Рабочая почта")]
        public string? WorkEmail { get; set; }

        [Required(ErrorMessage = "Обязательное поле")]
        [Display(Name = "Телефон")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Обязательное поле")]
        [Display(Name = "Номер ПК")]
        public string? PcNumber { get; set; }
    }
}