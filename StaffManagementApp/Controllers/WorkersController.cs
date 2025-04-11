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

        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Workers");
                if (response.IsSuccessStatusCode)
                {
                    var vwWorkerInfo = await response.Content.ReadFromJsonAsync<IEnumerable<VwWorkerInfo>>();
                    return View(vwWorkerInfo);
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

            return View(new List<VwWorkerInfo>());
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
