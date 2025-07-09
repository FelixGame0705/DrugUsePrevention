using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using System.Text;
using System.Globalization;
using System.Net.Http.Headers;

namespace DrugUserPreventionUI.Pages.Consultant
{
    public class ConsultantResponse
    {
        public int ConsultantID { get; set; }
        public string ConsultantName { get; set; } = string.Empty;
        public string Qualifications { get; set; }
        public string Specialty { get; set; }
        public string WorkingHours { get; set; }
    }
    public class ApppointmentResponse
    {
        //public int? ConsultantID { get; set; }
        //public DateTime ScheduledAt { get; set; }
        //public string Notes { get; set; }

        public int? AppointmentID { get; set; } 
        public int? UserID { get; set; }
        public int? ConsultantID { get; set; }
        public DateTime ScheduledAt { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; } 
    }
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private const string BASE_API_URL = "https://localhost:7045/api/Consultant"; // IMPORTANT: Update with your actual API URL
        private const string Appointment_API_URL = "https://localhost:7045/api/Appointment"; // IMPORTANT: Update with your actual API URL
        public string? UserRole { get; set; } = string.Empty;
        public int? CurrentUserId { get; set; }
        public List<ConsultantResponse>? ConsultantResponse { get; set; }
        public ConsultantResponse? ConsultantDetail { get; set; }
        [BindProperty]
        public ApppointmentResponse? ApppointmentDetail { get; set; }
        public List<DateTime> AvailableAppointmentSlots { get; set; } = new List<DateTime>();
        public List<ApppointmentResponse>? ExistingAppointments { get; set; } = new List<ApppointmentResponse>();

        [TempData]
        public string Message { get; set; } = string.Empty;
        [TempData]
        public string MessageType { get; set; } = "info"; // success, error, info

        [BindProperty(SupportsGet = true)]
        public string CurrentAction { get; set; } = "list"; // "list", "detail", "create", "edit"

        public IndexModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task OnGetAsync(int? id, string? action)
        {
            var authToken = Request.Cookies["auth_token"];
            if (authToken != null)
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(authToken) as JwtSecurityToken;
                var userIdClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                var userRoleClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
                UserRole = userRoleClaim.Value;
                CurrentUserId = int.Parse(userIdClaim.Value);
            }

            if (!string.IsNullOrEmpty(action))
            {
                CurrentAction = action;
            }

            if (CurrentAction == "detail" && id.HasValue)
            {
                await GetConsultantDetail(id.Value);
                if (ConsultantDetail != null)
                {
                    await GetExistingConsultantAppointments(id.Value);
                    GenerateAvailableAppointmentSlots(ConsultantDetail.WorkingHours);
                }
            }
            else // list or other invalid action
            {
                await GetConsultants();
            }
        }

        public async Task<IActionResult> OnPostCreateAppointmentAsync()
        {
            var authToken = Request.Cookies["auth_token"];
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(authToken) as JwtSecurityToken;
            var userIdClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            var userRoleClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            UserRole = userRoleClaim.Value;
            CurrentUserId = int.Parse(userIdClaim.Value);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            try 
            {
                var jsonContent = JsonSerializer.Serialize(ApppointmentDetail);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(Appointment_API_URL, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApppointmentResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (response.IsSuccessStatusCode)
                {
                    Message = "Appointment created successfully!";
                    MessageType = "success";
                    return RedirectToPage("/Consultant/Index");
                }
                else
                {
                    Message =  $"Error creating appointment";
                    MessageType = "error";
                    CurrentAction = "detail"; 
                    return Page();
                }
            }
            catch (Exception ex)
            {
                Message = $"An error occurred while creating the appointment: {ex.Message}";
                MessageType = "error";
                CurrentAction = "detail"; 
                return Page();
            }
        }

        private async Task GetConsultants()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BASE_API_URL}");
                response.EnsureSuccessStatusCode();
                var jsonResponse = await response.Content.ReadAsStringAsync();
                ConsultantResponse = JsonSerializer.Deserialize<List<ConsultantResponse>>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (HttpRequestException ex)
            {
                Message = $"Error fetching programs: {ex.Message}";
                MessageType = "error";
                ConsultantResponse = new List<ConsultantResponse>(); // Ensure it's not null for UI
            }
            catch (Exception ex)
            {
                Message = $"An unexpected error occurred: {ex.Message}";
                MessageType = "error";
                ConsultantResponse = new List<ConsultantResponse>();
            }
        }

        private async Task GetConsultantDetail(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BASE_API_URL}/{id}");
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ConsultantResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (apiResponse != null)
                {
                    ConsultantDetail = apiResponse;
                }
                else
                {
                    Message = $"Program with ID {id} not found.";
                    MessageType = "error";
                    CurrentAction = "list"; // Go back to list if not found
                }
            }
            catch (HttpRequestException ex)
            {
                Message = $"Error fetching program details: {ex.Message}";
                MessageType = "error";
                CurrentAction = "list";
            }
            catch (Exception ex)
            {
                Message = $"An unexpected error occurred: {ex.Message}";
                MessageType = "error";
                CurrentAction = "list";
            }
        }

        private async Task GetExistingConsultantAppointments(int consultantId)
        {
            try
            {
                // Assuming your API supports filtering appointments by ConsultantID, adjust the URL accordingly.
                // Example: https://localhost:7045/api/Appointment?consultantId={consultantId}
                var response = await _httpClient.GetAsync($"{Appointment_API_URL}");
                response.EnsureSuccessStatusCode();
                var jsonResponse = await response.Content.ReadAsStringAsync();
                ExistingAppointments = JsonSerializer.Deserialize<List<ApppointmentResponse>>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                ExistingAppointments = ExistingAppointments.Where(a => a.ConsultantID == consultantId).ToList();
            }
            catch (HttpRequestException ex)
            {
                Message = $"Error fetching existing appointments: {ex.Message}";
                MessageType = "error";
                ExistingAppointments = new List<ApppointmentResponse>();
            }
            catch (Exception ex)
            {
                Message = $"An unexpected error occurred while fetching existing appointments: {ex.Message}";
                MessageType = "error";
                ExistingAppointments = new List<ApppointmentResponse>();
            }
        }

        private void GenerateAvailableAppointmentSlots(string workingHoursString)
        {
            AvailableAppointmentSlots.Clear();
            if (string.IsNullOrWhiteSpace(workingHoursString))
            {
                return;
            }

            var parts = workingHoursString.Split(',');
            if (parts.Length != 2) return; // Invalid format

            var dayRangeOrList = parts[0].Trim();
            var timeRange = parts[1].Trim();

            TimeSpan startTime;
            TimeSpan endTime;

            var timeParts = timeRange.Split('-');
            if (timeParts.Length != 2 || !TimeSpan.TryParse(timeParts[0].Trim(), out startTime) ||
                !TimeSpan.TryParse(timeParts[1].Trim(), out endTime))
            {
                return; // Invalid time format
            }

            var workingDays = new List<DayOfWeek>();
            var culture = new CultureInfo("vi-VN");

            if (dayRangeOrList.Contains('-'))
            {
                var rangeParts = dayRangeOrList.Split('-');
                if (rangeParts.Length == 2)
                {
                    int startDayNumeric = GetDayOfWeekNumeric(rangeParts[0]);
                    int endDayNumeric = GetDayOfWeekNumeric(rangeParts[1]);

                    for (int i = startDayNumeric; i <= endDayNumeric; i++)
                    {
                        workingDays.Add((DayOfWeek)((i % 7)));
                    }
                }
            }
            else
            {
                var specificDays = dayRangeOrList.Split(',');
                foreach (var dayStr in specificDays)
                {
                    int dayNumeric = GetDayOfWeekNumeric(dayStr.Trim());
                    if (dayNumeric != -1)
                    {
                        workingDays.Add((DayOfWeek)((dayNumeric % 7)));
                    }
                }
            }

            // Temporarily store all potential slots before filtering
            var potentialSlots = new List<DateTime>();

            for (int i = 0; i < 7; i++)
            {
                DateTime currentDate = DateTime.Today.AddDays(i);
                DayOfWeek currentDayOfWeek = currentDate.DayOfWeek;

                if (workingDays.Contains(currentDayOfWeek))
                {
                    for (TimeSpan time = startTime; time < endTime; time = time.Add(TimeSpan.FromMinutes(30)))
                    {
                        DateTime slot = currentDate.Date.Add(time);

                        if (slot > DateTime.Now)
                        {
                            potentialSlots.Add(slot);
                        }
                    }
                }
            }

            // Filter out already booked slots
            // You might want to consider the status of the appointment here (e.g., only filter if Status is "Confirmed" or "Pending")
            AvailableAppointmentSlots = potentialSlots
                .Where(slot => !ExistingAppointments.Any(bookedApp =>
                    bookedApp.ScheduledAt.Year == slot.Year &&
                    bookedApp.ScheduledAt.Month == slot.Month &&
                    bookedApp.ScheduledAt.Day == slot.Day &&
                    bookedApp.ScheduledAt.Hour == slot.Hour &&
                    bookedApp.ScheduledAt.Minute == slot.Minute &&
                    (bookedApp.Status == "Pending" || bookedApp.Status == "Confirmed") // Adjust status as per your logic
                ))
                .OrderBy(s => s)
                .ToList();
        }


        // Helper to convert "T2", "T3", ..., "T7", "CN" to DayOfWeek numeric values
        // Assuming T2 is Monday (1), T3 is Tuesday (2), ..., T7 is Saturday (6), CN is Sunday (0)
        private int GetDayOfWeekNumeric(string dayCode)
        {
            return dayCode.ToUpperInvariant() switch
            {
                "T2" => 1, // Monday
                "T3" => 2, // Tuesday
                "T4" => 3, // Wednesday
                "T5" => 4, // Thursday
                "T6" => 5, // Friday
                "T7" => 6, // Saturday
                "CN" => 0, // Sunday
                _ => -1,   // Invalid
            };
        }
    }
}
