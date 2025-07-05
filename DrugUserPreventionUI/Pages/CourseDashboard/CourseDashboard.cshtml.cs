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

        // ✅ UPDATED: Remove Session-based user info, now use JWT token
        // Helper methods to get user info from JWT token
        private LoginModel GetLoginModel()
        {
            var loginModel = new LoginModel(_httpClientFactory);
            loginModel.PageContext = PageContext;
            return loginModel;
        }

        private UserInfoDto? GetCurrentUser()
        {
            return GetLoginModel().GetCurrentUser();
        }

        private bool IsAuthenticated()
        {
            return GetLoginModel().IsAuthenticated();
        }

        private string GetUserRole()
        {
            return GetLoginModel().GetUserRole();
        }

        private string GetDisplayName()
        {
            return GetLoginModel().GetDisplayName();
        }

        // Load danh sách khóa học với phân trang và filter
        public async Task<IActionResult> OnGetAsync(string? action = null, int? id = null,
            int pageIndex = 1, int pageSize = 10, string? message = null, string? messageType = null)
        {
            // ✅ ADDED: Check authentication first
            if (!IsAuthenticated())
            {
                return RedirectToPage("/Login", new { message = "Vui lòng đăng nhập để truy cập trang này.", messageType = "warning" });
            }

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
            // ✅ ADDED: Check authentication first
            if (!IsAuthenticated())
            {
                return RedirectToPage("/Login", new { message = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", messageType = "warning" });
            }

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
                var client = GetAuthenticatedClient();

                // Debug: Check if token exists
                var token = HttpContext.Request.Cookies["auth_token"];
                var currentUser = GetCurrentUser();
                Console.WriteLine($"=== ADD COURSE DEBUG ===");
                Console.WriteLine($"Token exists: {!string.IsNullOrEmpty(token)}");
                Console.WriteLine($"Current user: {currentUser?.UserName} ({currentUser?.Role})");
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
                    try
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
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"JSON Parse Error: {ex.Message}");
                        Console.WriteLine($"Raw response: {responseContent}");
                        Message = $"Lỗi parse JSON từ server";
                        MessageType = "error";
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
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
            // ✅ ADDED: Check authentication first
            if (!IsAuthenticated())
            {
                return RedirectToPage("/Login", new { message = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", messageType = "warning" });
            }

            if (!ModelState.IsValid)
            {
                await LoadCoursesList(GetAuthenticatedClient(), 1, 10);
                CurrentAction = "edit";
                return Page();
            }

            try
            {
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

            await LoadCoursesList(GetAuthenticatedClient(), 1, 10);
            CurrentAction = "edit";
            return Page();
        }

        // Xóa khóa học
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            // ✅ ADDED: Check authentication first
            if (!IsAuthenticated())
            {
                return RedirectToPage("/Login", new { message = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", messageType = "warning" });
            }

            try
            {
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
            // ✅ ADDED: Check authentication first
            if (!IsAuthenticated())
            {
                return RedirectToPage("/Login", new { message = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", messageType = "warning" });
            }

            try
            {
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
            // ✅ ADDED: Check authentication first
            if (!IsAuthenticated())
            {
                return RedirectToPage("/Login", new { message = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", messageType = "warning" });
            }

            try
            {
                var client = GetAuthenticatedClient();
                var json = JsonSerializer.Serialize(isAccept);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PatchAsync($"{BASE_API_URL}/{id}/approve", content);

                if (response.IsSuccessStatusCode)
                {
                    var approvalText = isAccept ? "duyệt" : "hủy duyệt";
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

        // ✅ UPDATED: Helper method to configure authenticated HTTP client using JWT token
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

        // Helper methods
        private async Task LoadCoursesList(HttpClient client, int pageIndex, int pageSize)
        {
            // Tạo query string cho phân trang
            var queryString = $"?pageIndex={pageIndex}&pageSize={pageSize}";

            try
            {
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
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP Request Exception in LoadCoursesList: {ex.Message}");
                Message = $"Lỗi khi tải danh sách khóa học: {ex.Message}";
                MessageType = "error";
            }
        }

        private async Task LoadCourseDetail(HttpClient client, int id)
        {
            try
            {
                var response = await client.GetAsync($"{BASE_API_URL}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<CourseResponseDto>>();
                    CourseDetail = apiResponse?.Data;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Response.Redirect("/Login?message=Phiên đăng nhập đã hết hạn&messageType=warning");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading course detail: {ex.Message}");
                Message = $"Lỗi khi tải chi tiết khóa học: {ex.Message}";
                MessageType = "error";
            }
        }

        private async Task LoadCourseForEdit(HttpClient client, int id)
        {
            try
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
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Response.Redirect("/Login?message=Phiên đăng nhập đã hết hạn&messageType=warning");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading course for edit: {ex.Message}");
                Message = $"Lỗi khi tải thông tin khóa học để chỉnh sửa: {ex.Message}";
                MessageType = "error";
            }
        }
    }
}