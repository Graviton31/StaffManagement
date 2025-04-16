using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StaffManagementApi.Models;
using StaffManagementApi.ModelsDto;
using System.Text.Json;

namespace StaffManagementApp.Controllers
{
    public class WorkersController : Controller
    {
        private readonly HttpClient _httpClient;

        public WorkersController(IHttpClientFactory clientFactory)
        {
            _httpClient = clientFactory.CreateClient("ApiClient");
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/Workers?pageNumber={pageNumber}&pageSize={pageSize}");
                if (response.IsSuccessStatusCode)
                {
                    var pagedResponse = await response.Content.ReadFromJsonAsync<PagedResponse<VwWorkerInfo>>();
                    return View(pagedResponse);
                }
                else
                {
                    Console.WriteLine($"Ошибка HTTP: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }

            return View(new PagedResponse<VwWorkerInfo> { Data = new List<VwWorkerInfo>() });
        }


        // GET: Workers/BirthDate
        public async Task<IActionResult> BirthDate()
        {
            var response = await _httpClient.GetAsync("api/Workers/birth_date");
            if (response.IsSuccessStatusCode)
            {
                var workersWithBirthDate = await response.Content.ReadFromJsonAsync<IEnumerable<WorkerBirthDateDto>>();
                return View(workersWithBirthDate);
            }
            return View(new List<WorkerBirthDateDto>());
        }


        public async Task<IActionResult> Details(int id)
        {
            var response = await _httpClient.GetAsync($"api/Workers/{id}");
            if (response.IsSuccessStatusCode)
            {
                var worker = await response.Content.ReadFromJsonAsync<VwWorkerDetail>();
                return View(worker);
            }
            return NotFound();
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/Workers/dto/{id}");
                response.EnsureSuccessStatusCode();

                // Десериализуем ответ API в WorkerDto
                var workerDto = await response.Content.ReadFromJsonAsync<WorkerDto>();

                await LoadRolesAsync();
                return View(workerDto);
            }
            catch
            {
                return NotFound();
            }
        }

        private async Task LoadRolesAsync()
        {
            var rolesResponse = await _httpClient.GetAsync("api/Roles");
            if (rolesResponse.IsSuccessStatusCode)
            {
                var roles = await rolesResponse.Content.ReadFromJsonAsync<List<Role>>();
                ViewBag.Roles = new SelectList(roles, "IdRole", "Name"); 
            }
        }

        // POST: Workers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdWorker,Surname,Name,Patronymic,BirthDate,IdRole,WorkEmail,Phone,PcNumber")] WorkerDto workerDto)
        {
            if (id != workerDto.IdWorker)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var response = await _httpClient.PutAsJsonAsync($"api/Workers/update/{id}", workerDto);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Details", new { id = workerDto.IdWorker });
                }
                ModelState.AddModelError("", "Ошибка при обновлении. Проверьте данные.");
            }

            // Повторная загрузка ролей при ошибке
            var rolesResponse = await _httpClient.GetAsync("api/Roles");
            if (rolesResponse.IsSuccessStatusCode)
            {
                var roles = await rolesResponse.Content.ReadFromJsonAsync<List<Role>>();
                ViewBag.Roles = new SelectList(roles, "IdRole", "Name", workerDto.IdRole);
            }
            return RedirectToAction("Details", new { id = workerDto.IdWorker});
        }
    }
}
