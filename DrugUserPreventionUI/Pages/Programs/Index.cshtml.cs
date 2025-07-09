using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DrugUserPreventionUI.Models.Common;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DrugUserPreventionUI.Pages.Programs
{
    // DTOs matching your API's ProgramController
    public class ProgramResponse
    {
        public int ProgramID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string? ThumbnailURL { get; set; } // Nullable as per your update
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; }
        public int CreatedBy { get; set; }
        public bool IsActive { get; set; }
        public string CreatorName { get; set; } = string.Empty; // Assuming this might be returned by API
    }

    public class ProgramCreateRequest
    {
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Url(ErrorMessage = "Invalid Thumbnail URL")]
        public string? ThumbnailURL { get; set; }

        [Required(ErrorMessage = "Start Date is required")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "End Date is required")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(3);

        [Required(ErrorMessage = "Location is required")]
        public string Location { get; set; }

        [Required(ErrorMessage = "Created By is required")] // Added CreatedBy as per your update
        public int CreatedBy { get; set; }

        public bool IsActive { get; set; } = true; // Retained default value and removed [Required]
    }

    public class ProgramUpdateRequest
    {
        [Required]
        public int ProgramID { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Url(ErrorMessage = "Invalid Thumbnail URL")]
        public string? ThumbnailURL { get; set; }

        [Required(ErrorMessage = "Start Date is required")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End Date is required")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Location is required")]
        public string Location { get; set; }
        public bool IsActive { get; set; }
    }

    public class ProgramFilterDto : BasePaginationDto
    {
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
    }

    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private const string BASE_API_URL = "https://localhost:7045/api/Program"; // IMPORTANT: Update with your actual API URL
        public string? UserRole { get; set; } = string.Empty;
        public int? CurrentUserId { get; set; }

        [BindProperty(SupportsGet = true)]
        public ProgramFilterDto FilterModel { get; set; } = new();

        public PaginatedApiResponse<ProgramResponse>? ProgramsResponse { get; set; }
        public ProgramResponse? ProgramDetail { get; set; }

        [TempData]
        public string Message { get; set; } = string.Empty;
        [TempData]
        public string MessageType { get; set; } = "info"; // success, error, info

        [BindProperty(SupportsGet = true)]
        public string CurrentAction { get; set; } = "list"; // "list", "detail", "create", "edit"

        [BindProperty]
        public ProgramCreateRequest NewProgram { get; set; } = new();

        [BindProperty]
        public ProgramUpdateRequest EditProgram { get; set; } = new();

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
                await GetProgramDetail(id.Value);
            }
            else if (CurrentAction == "edit" && id.HasValue)
            {
                await GetProgramToEdit(id.Value);
            }
            else // list or other invalid action
            {
                await GetProgramsList();
            }
        }

        private async Task GetProgramsList()
        {
            try
            {
                var query = $"pageIndex={FilterModel.PageIndex}&pageSize={FilterModel.PageSize}";
                if (!string.IsNullOrEmpty(FilterModel.SearchTerm))
                {
                    query += $"&searchTerm={Uri.EscapeDataString(FilterModel.SearchTerm)}";
                }
                if (FilterModel.IsActive.HasValue)
                {
                    query += $"&isActive={FilterModel.IsActive.Value}";
                }

                var response = await _httpClient.GetAsync($"{BASE_API_URL}?{query}");
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                ProgramsResponse = JsonSerializer.Deserialize<PaginatedApiResponse<ProgramResponse>>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (HttpRequestException ex)
            {
                Message = $"Error fetching programs: {ex.Message}";
                MessageType = "error";
                ProgramsResponse = new PaginatedApiResponse<ProgramResponse>(); // Ensure it's not null for UI
            }
            catch (Exception ex)
            {
                Message = $"An unexpected error occurred: {ex.Message}";
                MessageType = "error";
                ProgramsResponse = new PaginatedApiResponse<ProgramResponse>();
            }
        }

        private async Task GetProgramDetail(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BASE_API_URL}/{id}");
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProgramResponse>>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (apiResponse != null && apiResponse.Success && apiResponse.Data != null)
                {
                    ProgramDetail = apiResponse.Data;
                }
                else
                {
                    Message = apiResponse?.Message ?? $"Program with ID {id} not found.";
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

        private async Task GetProgramToEdit(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BASE_API_URL}/{id}");
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProgramResponse>>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (apiResponse != null && apiResponse.Success && apiResponse.Data != null)
                {
                    // Map ProgramResponse to ProgramUpdateRequest
                    EditProgram = new ProgramUpdateRequest
                    {
                        ProgramID = apiResponse.Data.ProgramID,
                        Title = apiResponse.Data.Title,
                        Description = apiResponse.Data.Description,
                        ThumbnailURL = apiResponse.Data.ThumbnailURL,
                        StartDate = apiResponse.Data.StartDate,
                        EndDate = apiResponse.Data.EndDate,
                        Location = apiResponse.Data.Location,
                        IsActive = apiResponse.Data.IsActive
                    };
                }
                else
                {
                    Message = apiResponse?.Message ?? $"Program with ID {id} not found for editing.";
                    MessageType = "error";
                    CurrentAction = "list"; // Go back to list if not found
                }
            }
            catch (HttpRequestException ex)
            {
                Message = $"Error fetching program for edit: {ex.Message}";
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

        public async Task<IActionResult> OnPostCreateProgramAsync()
        {
            var authToken = Request.Cookies["auth_token"];
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(authToken) as JwtSecurityToken;
            var userIdClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            var userRoleClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            UserRole = userRoleClaim.Value;
            CurrentUserId = int.Parse(userIdClaim.Value);
            NewProgram.CreatedBy = (int)CurrentUserId;
            //if (!ModelState.IsValid)
            //{
            //    CurrentAction = "create"; 
            //    return Page();
            //}

            try
            {
                var jsonContent = JsonSerializer.Serialize(NewProgram);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(BASE_API_URL, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProgramResponse>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (response.IsSuccessStatusCode && apiResponse != null && apiResponse.Success)
                {
                    Message = "Program created successfully!";
                    MessageType = "success";
                    return RedirectToPage("/Programs/Index");
                }
                else
                {
                    Message = apiResponse?.Message ?? $"Error creating program: {response.StatusCode} - {responseContent}";
                    if (apiResponse?.Errors != null && apiResponse.Errors.Any())
                    {
                        Message += " " + string.Join("; ", apiResponse.Errors);
                    }
                    MessageType = "error";
                    CurrentAction = "create"; // Stay on create form with error
                    return Page();
                }
            }
            catch (Exception ex)
            {
                Message = $"An error occurred while creating the program: {ex.Message}";
                MessageType = "error";
                CurrentAction = "create"; // Stay on create form with error
                return Page();
            }
        }

        public async Task<IActionResult> OnPostUpdateProgramAsync()
        {
            //if (!ModelState.IsValid)
            //{
            //    CurrentAction = "edit"; // Stay on edit form
            //    return Page();
            //}

            try
            {
                var jsonContent = JsonSerializer.Serialize(EditProgram);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(BASE_API_URL, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProgramResponse>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (response.IsSuccessStatusCode && apiResponse != null && apiResponse.Success)
                {
                    Message = "Program updated successfully!";
                    MessageType = "success";
                    return RedirectToPage("/Programs/Index");
                }
                else
                {
                    Message = apiResponse?.Message ?? $"Error updating program: {response.StatusCode} - {responseContent}";
                    if (apiResponse?.Errors != null && apiResponse.Errors.Any())
                    {
                        Message += " " + string.Join("; ", apiResponse.Errors);
                    }
                    MessageType = "error";
                    CurrentAction = "edit"; // Stay on edit form with error
                    return Page();
                }
            }
            catch (Exception ex)
            {
                Message = $"An error occurred while updating the program: {ex.Message}";
                MessageType = "error";
                CurrentAction = "edit"; // Stay on edit form with error
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteProgramAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{BASE_API_URL}/{id}");

                if (response.IsSuccessStatusCode)
                {
                    Message = "Program deleted successfully!";
                    MessageType = "success";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Message = $"Error deleting program: {response.StatusCode} - {errorContent}";
                    MessageType = "error";
                }
            }
            catch (Exception ex)
            {
                Message = $"An error occurred while deleting the program: {ex.Message}";
                MessageType = "error";
            }

            return RedirectToPage("/Programs/Index");
        }
    }
}