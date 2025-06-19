using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;
using DrugUserPreventionUI.Models.CourseDashboard; // Note: typo in namespace
using DrugUserPreventionUI.Models.Common;

namespace DrugUserPreventionUI.Pages.CourseDashboard
{
    public class CourseDashboardModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const string BASE_API_URL = "https://localhost:7045/api/Courses";

        public CourseDashboardModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<CourseListDto> Courses { get; set; } = new List<CourseListDto>();
        public CourseResponseDto? CourseDetail { get; set; }
        public string? CurrentAction { get; set; }
        public string? Message { get; set; }
        public string? MessageType { get; set; } // success, error, info
        public PaginationInfo? PaginationInfo { get; set; }

        [BindProperty]
        public CreateCourseDto CourseForm { get; set; } = new CreateCourseDto();
        // Helper properties for user info
        public string UserName => HttpContext.Session.GetString("user_name") ?? "User";
        public string UserRole => HttpContext.Session.GetString("user_role") ?? "Member";

        // Load danh sách khóa học với phân trang và filter
        public async Task<IActionResult> OnGetAsync(string? action = null, int? id = null,
            int pageIndex = 1, int pageSize = 10, string? message = null, string? messageType = null)
        {
            CurrentAction = action?.ToLower();

            // Hiển thị message từ redirect
            if (!string.IsNullOrEmpty(message))
            {
                Message = message;
                MessageType = messageType ?? "info";
            }

            try
            {
                var client = GetAuthenticatedClient();

                // Load danh sách courses với filter
                await LoadCoursesList(client, pageIndex, pageSize);

                // Xử lý các action khác nhau
                switch (CurrentAction)
                {
                    case "detail":
                        if (id.HasValue)
                        {
                            await LoadCourseDetail(client, id.Value);
                        }
                        break;
                    case "edit":
                        if (id.HasValue)
                        {
                            await LoadCourseForEdit(client, id.Value);
                        }
                        break;
                    case "add":
                        // Form trống cho thêm mới
                        CourseForm = new CreateCourseDto();
                        break;
                }
            }
            catch (Exception ex)
            {
                Message = $"Lỗi khi tải dữ liệu: {ex.Message}";
                MessageType = "error";
            }

            return Page();
        }

        // Thêm khóa học mới
        public async Task<IActionResult> OnPostAddAsync()
        {
            // Validate on server side first
            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState
                    .SelectMany(x => x.Value.Errors)
                    .Select(x => x.ErrorMessage)
                    .ToList();

                Message = "Dữ liệu không hợp lệ: " + string.Join(", ", validationErrors);
                MessageType = "error";

                await LoadCoursesList(GetAuthenticatedClient(), 1, 10);
                CurrentAction = "add";
                return Page();
            }

            try
            {
                // ✅ SỬA: Sử dụng GetAuthenticatedClient() thay vì _httpClientFactory.CreateClient()
                var client = GetAuthenticatedClient();

                // Debug: Check if token exists
                var token = HttpContext.Request.Cookies["auth_token"];
                Console.WriteLine($"=== ADD COURSE DEBUG ===");
                Console.WriteLine($"Token exists: {!string.IsNullOrEmpty(token)}");
                if (!string.IsNullOrEmpty(token))
                {
                    Console.WriteLine($"Token preview: {token[..Math.Min(50, token.Length)]}...");
                }
                Console.WriteLine($"Authorization header: {client.DefaultRequestHeaders.Authorization?.ToString()}");

                // Debug: Log request data
                var json = JsonSerializer.Serialize(CourseForm, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                Console.WriteLine($"Sending request to {BASE_API_URL}:");
                Console.WriteLine($"Request JSON: {json}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(BASE_API_URL, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                // Debug: Log response
                Console.WriteLine($"Response Status: {response.StatusCode}");
                Console.WriteLine($"Response Content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<CourseResponseDto>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (apiResponse?.Success == true)
                    {
                        return RedirectToPage("/CourseDashboard/CourseDashboard", new { message = "Thêm khóa học thành công!", messageType = "success" });
                    }
                    else
                    {
                        Message = $"API Error: {apiResponse?.Message ?? "Không thể tạo khóa học"}";
                        if (apiResponse?.Errors?.Any() == true)
                        {
                            Message += "\nChi tiết: " + string.Join(", ", apiResponse.Errors);
                        }
                        MessageType = "error";
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // ✅ THÊM: Xử lý riêng lỗi Unauthorized
                    Console.WriteLine("Unauthorized - Redirecting to login");
                    return RedirectToPage("/Login", new { message = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", messageType = "warning" });
                }
                else
                {
                    // Parse error response for better debugging
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        Message = $"HTTP {(int)response.StatusCode} - {errorResponse?.Message ?? response.ReasonPhrase}";
                        if (errorResponse?.Errors?.Any() == true)
                        {
                            Message += "\nLỗi chi tiết: " + string.Join(", ", errorResponse.Errors);
                        }

                        // Add raw response for debugging
                        Message += $"\nRaw Response: {responseContent}";
                    }
                    catch
                    {
                        Message = $"HTTP {(int)response.StatusCode} - {response.ReasonPhrase}\nRaw Response: {responseContent}";
                    }
                    MessageType = "error";
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP Request Exception: {ex.Message}");
                Message = $"Lỗi kết nối: {ex.Message}";
                MessageType = "error";
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine($"Timeout Exception: {ex.Message}");
                Message = $"Timeout: {ex.Message}";
                MessageType = "error";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Exception: {ex.Message}");
                Message = $"Lỗi không mong muốn: {ex.Message}\nStackTrace: {ex.StackTrace}";
                MessageType = "error";
            }

            await LoadCoursesList(GetAuthenticatedClient(), 1, 10);
            CurrentAction = "add";
            return Page();
        }

        // Cập nhật khóa học
        public async Task<IActionResult> OnPostUpdateAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                await LoadCoursesList(GetAuthenticatedClient(), 1, 10); // ✅ SỬA: Sử dụng GetAuthenticatedClient()
                CurrentAction = "edit";
                return Page();
            }

            try
            {
                // ✅ SỬA: Sử dụng GetAuthenticatedClient()
                var client = GetAuthenticatedClient();

                // Tạo UpdateCourseDto từ CreateCourseDto
                var updateDto = new UpdateCourseDto
                {
                    CourseID = id,
                    Title = CourseForm.Title,
                    Description = CourseForm.Description,
                    TargetGroup = CourseForm.TargetGroup,
                    AgeGroup = CourseForm.AgeGroup,
                    ContentURL = CourseForm.ContentURL,
                    ThumbnailURL = CourseForm.ThumbnailURL
                };

                var json = JsonSerializer.Serialize(updateDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync($"{BASE_API_URL}/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/CourseDashboard/CourseDashboard", new { message = "Cập nhật khóa học thành công!", messageType = "success" });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Login", new { message = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", messageType = "warning" });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Message = $"Lỗi khi cập nhật: {errorContent}";
                    MessageType = "error";
                }
            }
            catch (Exception ex)
            {
                Message = $"Lỗi: {ex.Message}";
                MessageType = "error";
            }

            await LoadCoursesList(GetAuthenticatedClient(), 1, 10); // ✅ SỬA: Sử dụng GetAuthenticatedClient()
            CurrentAction = "edit";
            return Page();
        }

        // Xóa khóa học
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                // ✅ SỬA: Sử dụng GetAuthenticatedClient()
                var client = GetAuthenticatedClient();
                var response = await client.DeleteAsync($"{BASE_API_URL}/{id}");

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/CourseDashboard/CourseDashboard", new { message = "Xóa khóa học thành công!", messageType = "success" });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Login", new { message = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", messageType = "warning" });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return RedirectToPage("/CourseDashboard/CourseDashboard", new { message = $"Lỗi khi xóa: {errorContent}", messageType = "error" });
                }
            }
            catch (Exception ex)
            {
                return RedirectToPage("/CourseDashboard/CourseDashboard", new { message = $"Lỗi: {ex.Message}", messageType = "error" });
            }
        }

        // Cập nhật trạng thái khóa học (Active/Inactive)
        public async Task<IActionResult> OnPostUpdateStatusAsync(int id, bool isActive)
        {
            try
            {
                // ✅ SỬA: Sử dụng GetAuthenticatedClient()
                var client = GetAuthenticatedClient();
                var json = JsonSerializer.Serialize(isActive);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PatchAsync($"{BASE_API_URL}/{id}/toggle-status", content);

                if (response.IsSuccessStatusCode)
                {
                    var statusText = isActive ? "kích hoạt" : "vô hiệu hóa";
                    return RedirectToPage("/CourseDashboard/CourseDashboard", new { message = $"Đã {statusText} khóa học thành công!", messageType = "success" });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Login", new { message = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", messageType = "warning" });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return RedirectToPage("/CourseDashboard/CourseDashboard", new { message = $"Lỗi khi cập nhật trạng thái: {errorContent}", messageType = "error" });
                }
            }
            catch (Exception ex)
            {
                return RedirectToPage("/CourseDashboard/CourseDashboard", new { message = $"Lỗi: {ex.Message}", messageType = "error" });
            }
        }

        // Duyệt khóa học (Accept/Reject)
        public async Task<IActionResult> OnPostApproveAsync(int id, bool isAccept)
        {
            try
            {
                // ✅ SỬA: Sử dụng GetAuthenticatedClient()
                var client = GetAuthenticatedClient();
                var json = JsonSerializer.Serialize(isAccept);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PatchAsync($"{BASE_API_URL}/{id}/approve", content);

                if (response.IsSuccessStatusCode)
                {
                    var approvalText = isAccept ? "duyệt" : "từ chối";
                    return RedirectToPage("/CourseDashboard/CourseDashboard", new { message = $"Đã {approvalText} khóa học thành công!", messageType = "success" });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Login", new { message = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", messageType = "warning" });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return RedirectToPage("/CourseDashboard/CourseDashboard", new { message = $"Lỗi khi duyệt khóa học: {errorContent}", messageType = "error" });
                }
            }
            catch (Exception ex)
            {
                return RedirectToPage("/CourseDashboard/CourseDashboard", new { message = $"Lỗi: {ex.Message}", messageType = "error" });
            }
        }

        // Helper method to configure authenticated HTTP client
        private HttpClient GetAuthenticatedClient()
        {
            var client = _httpClientFactory.CreateClient();

            var token = HttpContext.Request.Cookies["auth_token"];
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        // Helper method to check authentication
        private bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(HttpContext.Request.Cookies["auth_token"]);
        }

        // Helper methods
        private async Task LoadCoursesList(HttpClient client, int pageIndex, int pageSize)
        {
            // Tạo query string cho phân trang
            var queryString = $"?pageIndex={pageIndex}&pageSize={pageSize}";

            var response = await client.GetAsync($"{BASE_API_URL}{queryString}");

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Token expired or invalid, redirect to login
                Response.Redirect("/Login?message=Phiên đăng nhập đã hết hạn&messageType=warning");
                return;
            }

            response.EnsureSuccessStatusCode();

            var apiResponse = await response.Content.ReadFromJsonAsync<PaginatedApiResponse<CourseListDto>>();
            if (apiResponse?.Data != null)
            {
                Courses.AddRange(apiResponse.Data);
                PaginationInfo = apiResponse.Pagination;

                // Set ViewData for pagination
                ViewData["CurrentPage"] = PaginationInfo.CurrentPage;
                ViewData["TotalPages"] = PaginationInfo.TotalPages;
                ViewData["TotalItems"] = PaginationInfo.TotalItems;
                ViewData["HasPreviousPage"] = PaginationInfo.HasPreviousPage;
                ViewData["HasNextPage"] = PaginationInfo.HasNextPage;
            }
        }

        private async Task LoadCourseDetail(HttpClient client, int id)
        {
            var response = await client.GetAsync($"{BASE_API_URL}/{id}");
            if (response.IsSuccessStatusCode)
            {
                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<CourseResponseDto>>();
                CourseDetail = apiResponse?.Data;
            }
        }

        private async Task LoadCourseForEdit(HttpClient client, int id)
        {
            var response = await client.GetAsync($"{BASE_API_URL}/{id}");
            if (response.IsSuccessStatusCode)
            {
                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<CourseResponseDto>>();
                if (apiResponse?.Data != null)
                {
                    var course = apiResponse.Data;
                    CourseForm = new CreateCourseDto
                    {
                        Title = course.Title,
                        Description = course.Description ?? string.Empty,
                        TargetGroup = course.TargetGroup ?? string.Empty,
                        AgeGroup = course.AgeGroup ?? string.Empty,
                        ContentURL = course.ContentURL,
                        ThumbnailURL = course.ThumbnailURL
                    };
                }
            }
        }
    }
}