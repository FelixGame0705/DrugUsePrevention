using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using System.Text;
using DrugUserPreventionUI.Models.Common;

namespace DrugUserPreventionUI.Pages.Appointment
{
    public class ApppointmentResponse
    {
        public int AppointmentID { get; set; }
        public int? UserID { get; set; }
        public string Username { get; set; }
        public int? ConsultantID { get; set; }
        public string ConsultantName { get; set; }
        public DateTime ScheduledAt { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private const string BASE_API_URL = "https://localhost:7045/api/Appointment"; // IMPORTANT: Update with your actual API URL
        private const string CONSULTANT_API_URL = "https://localhost:7045/api/Consultant"; // IMPORTANT: Update with your actual API URL
        public string? UserRole { get; set; } = string.Empty;
        public int? CurrentUserId { get; set; }
        public List<ApppointmentResponse>? ApppointmentResponse { get; set; }
        public ApppointmentResponse? ApppointmentDetail { get; set; }

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
                await GetAppointmentDetail(id.Value);
            }
            else // list or other invalid action
            {
                await GetAppointments();
            }
        }

        private async Task GetAppointments()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BASE_API_URL}");
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                ApppointmentResponse = JsonSerializer.Deserialize<List<ApppointmentResponse>>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (ApppointmentResponse != null && ApppointmentResponse.Count > 0)
                {
                    if (UserRole == "Consultant")
                    {
                        ApppointmentResponse = ApppointmentResponse.Where(a => a.ConsultantID == CurrentUserId).ToList();
                    }
                    else if (UserRole == "Member" || UserRole == "Guest")
                    {
                        ApppointmentResponse = ApppointmentResponse.Where(a => a.UserID == CurrentUserId).ToList();
                    }
                    else
                    {
                        ApppointmentResponse = null;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Message = $"Error fetching programs: {ex.Message}";
                MessageType = "error";
                ApppointmentResponse = new List<ApppointmentResponse>(); // Ensure it's not null for UI
            }
            catch (Exception ex)
            {
                Message = $"An unexpected error occurred: {ex.Message}";
                MessageType = "error";
                ApppointmentResponse = new List<ApppointmentResponse>();
            }
        }

        private async Task GetAppointmentDetail(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BASE_API_URL}/{id}");
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApppointmentResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (apiResponse != null)
                {
                    ApppointmentDetail = apiResponse;
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

        public async Task<IActionResult> OnPostUpdate(int id, string status)
        {
            try
            {
                var jsonContent = JsonSerializer.Serialize(status);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var newResponse = await _httpClient.PutAsync($"{CONSULTANT_API_URL}/appointment/{id}", content);

                if (newResponse.IsSuccessStatusCode)
                {
                    Message = "Program updated successfully!";
                    MessageType = "success";
                    return RedirectToPage("/Appointment/Index");
                }
                else
                {
                    Message = $"Error updating program: {newResponse.StatusCode}";
                    MessageType = "error";
                    CurrentAction = "list";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                Message = $"An error occurred while updating the Appointment: {ex.Message}";
                MessageType = "error";
                CurrentAction = "list";
                return Page();
            }
        }


    }
}
