using Microsoft.AspNetCore.Mvc;
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
    }
}
