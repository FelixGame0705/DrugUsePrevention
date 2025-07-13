using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;
using DrugUserPreventionUI.Models.Common;
using DrugUserPreventionUI.Models.Tags;
using DrugUserPreventionUI.Pages;
using DrugUserPreventionUI.Pages.AdminDashboard;
using DrugUserPreventionUI.Models.NewsArticles;
using DrugUserPreventionUI.Models.Categories;

namespace DrugUsePrevention.Pages.Staff
{
    public class StaffDashboardModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const string BASE_API_URL = "https://localhost:7045/api";

        public StaffDashboardModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Dashboard Properties
        public string? CurrentSection { get; set; } = "dashboard";
        public string? Message { get; set; }
        public string? MessageType { get; set; }

        // User Info Properties for Razor view access
        public string CurrentUserRole => GetUserRole();
        public string CurrentUserDisplayName => GetDisplayName();
        public int CurrentUserId => GetCurrentUserId();

        // Statistics Properties
        public DashboardStatsDto DashboardStats { get; set; } = new DashboardStatsDto();

        // Members Properties
        public List<UserResponse> Members { get; set; } = new List<UserResponse>();
        public PaginationInfo? MembersPagination { get; set; }

        // News Properties
        public List<NewsArticleDto> NewsArticles { get; set; } = new List<NewsArticleDto>();
        public PaginationInfo? NewsPagination { get; set; }

        // Categories Properties
        public List<CategoryDTO> Categories { get; set; } = new List<CategoryDTO>();
        public PaginationInfo? CategoriesPagination { get; set; }

        // Tags Properties
        public List<TagDTO> Tags { get; set; } = new List<TagDTO>();
        public PaginationInfo? TagsPagination { get; set; }

        // Form Properties
        [BindProperty]
        public CreateMemberDto MemberForm { get; set; } = new CreateMemberDto();

        [BindProperty]
        public CreateCategoryDto CategoryForm { get; set; } = new CreateCategoryDto();

        [BindProperty]
        public CreateTagDto TagForm { get; set; } = new CreateTagDto();

        [BindProperty]
        public UpdateProfileDto ProfileForm { get; set; } = new UpdateProfileDto();

        [BindProperty]
        public ChangePasswordDto ChangePasswordForm { get; set; } = new ChangePasswordDto();

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

        public string GetDisplayName()
        {
            return GetLoginModel().GetDisplayName();
        }

        private int GetCurrentUserId()
        {
            var user = GetCurrentUser();
            return user?.UserID ?? 0;
        }

        // Role-based permission checks - using properties for better Razor access
        public bool UserCanAccessStaffDashboard()
        {
            var role = CurrentUserRole;
            return role == "Staff" || role == "Manager" || role == "Admin";
        }

        public bool UserCanManageMembers()
        {
            var role = CurrentUserRole;
            return role == "Staff" || role == "Manager" || role == "Admin";
        }

        public bool UserCanManageNews()
        {
            var role = CurrentUserRole;
            return role == "Staff" || role == "Manager" || role == "Admin";
        }

        public bool UserCanManageCategories()
        {
            var role = CurrentUserRole;
            return role == "Staff" || role == "Manager" || role == "Admin";
        }

        public bool UserCanManageTags()
        {
            var role = CurrentUserRole;
            return role == "Staff" || role == "Manager" || role == "Admin";
        }

        // GET Handler
        public async Task<IActionResult> OnGetAsync(string? section = null, int? id = null,
            int pageIndex = 1, int pageSize = 10, string? message = null, string? messageType = null)
        {
            // Check authentication
            if (!IsAuthenticated())
            {
                return RedirectToPage("/Login", new { message = "Vui lòng đăng nhập để truy cập trang này.", messageType = "warning" });
            }

            // Check permissions
            if (!UserCanAccessStaffDashboard())
            {
                return RedirectToPage("/Index", new { message = "Bạn không có quyền truy cập trang quản lý.", messageType = "error" });
            }

            CurrentSection = section?.ToLower() ?? "dashboard";

            if (!string.IsNullOrEmpty(message))
            {
                Message = message;
                MessageType = messageType ?? "info";
            }

            try
            {
                var client = GetAuthenticatedClient();

                // Always load dashboard stats
                await LoadDashboardStats(client);

                // Load data based on current section
                switch (CurrentSection)
                {
                    case "members":
                        await LoadMembers(client, pageIndex, pageSize);
                        break;
                    case "news":
                        await LoadNews(client, pageIndex, pageSize);
                        break;
                    case "categories":
                        await LoadCategories(client, pageIndex, pageSize);
                        break;
                    case "tags":
                        await LoadTags(client, pageIndex, pageSize);
                        break;
                    case "profile":
                        await LoadCurrentUserProfile(client);
                        break;
                    default: // dashboard
                        // Stats already loaded
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

        // POST Handlers for Members
        public async Task<IActionResult> OnPostCreateMemberAsync()
        {
            if (!IsAuthenticated())
            {
                return RedirectToPage("/Login", new { message = "Phiên đăng nhập đã hết hạn.", messageType = "warning" });
            }

            if (!UserCanManageMembers())
            {
                return RedirectToPage("/Staff/StaffDashboard",
                    new { section = "members", message = "Bạn không có quyền tạo Member mới.", messageType = "error" });
            }

            if (!ModelState.IsValid)
            {
                await LoadMembers(GetAuthenticatedClient(), 1, 10);
                CurrentSection = "members";
                Message = "Dữ liệu không hợp lệ.";
                MessageType = "error";
                return Page();
            }

            try
            {
                var client = GetAuthenticatedClient();
                var createRequest = new CreateUserRequest
                {
                    FullName = MemberForm.FullName,
                    Username = MemberForm.Username,
                    Email = MemberForm.Email,
                    Password = MemberForm.Password,
                    Phone = MemberForm.Phone,
                    Role = "Member"
                };

                var json = JsonSerializer.Serialize(createRequest, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{BASE_API_URL}/Admin/create", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/Staff/StaffDashboard",
                        new { section = "members", message = "Tạo Member thành công!", messageType = "success" });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Message = $"Lỗi khi tạo Member: {errorContent}";
                    MessageType = "error";
                }
            }
            catch (Exception ex)
            {
                Message = $"Lỗi: {ex.Message}";
                MessageType = "error";
            }

            await LoadMembers(GetAuthenticatedClient(), 1, 10);
            CurrentSection = "members";
            return Page();
        }

        public async Task<IActionResult> OnPostBanMemberAsync(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToPage("/Login", new { message = "Phiên đăng nhập đã hết hạn.", messageType = "warning" });
            }

            if (!UserCanManageMembers())
            {
                return RedirectToPage("/Staff/StaffDashboard",
                    new { section = "members", message = "Bạn không có quyền ban Member.", messageType = "error" });
            }

            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.PostAsync($"{BASE_API_URL}/Admin/ban/{id}", null);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/Staff/StaffDashboard",
                        new { section = "members", message = "Ban Member thành công!", messageType = "success" });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return RedirectToPage("/Staff/StaffDashboard",
                        new { section = "members", message = $"Lỗi khi ban Member: {errorContent}", messageType = "error" });
                }
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Staff/StaffDashboard",
                    new { section = "members", message = $"Lỗi: {ex.Message}", messageType = "error" });
            }
        }

        public async Task<IActionResult> OnPostUnbanMemberAsync(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToPage("/Login", new { message = "Phiên đăng nhập đã hết hạn.", messageType = "warning" });
            }

            if (!UserCanManageMembers())
            {
                return RedirectToPage("/Staff/StaffDashboard",
                    new { section = "members", message = "Bạn không có quyền unban Member.", messageType = "error" });
            }

            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.PostAsync($"{BASE_API_URL}/Admin/unban/{id}", null);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/Staff/StaffDashboard",
                        new { section = "members", message = "Unban Member thành công!", messageType = "success" });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return RedirectToPage("/Staff/StaffDashboard",
                        new { section = "members", message = $"Lỗi khi unban Member: {errorContent}", messageType = "error" });
                }
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Staff/StaffDashboard",
                    new { section = "members", message = $"Lỗi: {ex.Message}", messageType = "error" });
            }
        }

        // POST Handlers for News
        public async Task<IActionResult> OnPostToggleNewsStatusAsync(int id, string status)
        {
            if (!IsAuthenticated())
            {
                return RedirectToPage("/Login", new { message = "Phiên đăng nhập đã hết hạn.", messageType = "warning" });
            }

            if (!UserCanManageNews())
            {
                return RedirectToPage("/Staff/StaffDashboard",
                    new { section = "news", message = "Bạn không có quyền thay đổi trạng thái News.", messageType = "error" });
            }

            try
            {
                var client = GetAuthenticatedClient();
                var json = JsonSerializer.Serialize(status);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PatchAsync($"{BASE_API_URL}/NewsArticles/{id}/toggle-status", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/Staff/StaffDashboard",
                        new { section = "news", message = "Cập nhật trạng thái News thành công!", messageType = "success" });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return RedirectToPage("/Staff/StaffDashboard",
                        new { section = "news", message = $"Lỗi khi cập nhật trạng thái: {errorContent}", messageType = "error" });
                }
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Staff/StaffDashboard",
                    new { section = "news", message = $"Lỗi: {ex.Message}", messageType = "error" });
            }
        }

        public async Task<IActionResult> OnPostDeleteNewsAsync(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToPage("/Login", new { message = "Phiên đăng nhập đã hết hạn.", messageType = "warning" });
            }

            if (!UserCanManageNews())
            {
                return RedirectToPage("/Staff/StaffDashboard",
                    new { section = "news", message = "Bạn không có quyền xóa News.", messageType = "error" });
            }

            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.DeleteAsync($"{BASE_API_URL}/NewsArticles/{id}");

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/Staff/StaffDashboard",
                        new { section = "news", message = "Xóa News thành công!", messageType = "success" });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return RedirectToPage("/Staff/StaffDashboard",
                        new { section = "news", message = $"Lỗi khi xóa News: {errorContent}", messageType = "error" });
                }
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Staff/StaffDashboard",
                    new { section = "news", message = $"Lỗi: {ex.Message}", messageType = "error" });
            }
        }

        // POST Handlers for Categories
        public async Task<IActionResult> OnPostCreateCategoryAsync()
        {
            if (!IsAuthenticated())
            {
                return RedirectToPage("/Login", new { message = "Phiên đăng nhập đã hết hạn.", messageType = "warning" });
            }

            if (!UserCanManageCategories())
            {
                return RedirectToPage("/Staff/StaffDashboard",
                    new { section = "categories", message = "Bạn không có quyền tạo Category.", messageType = "error" });
            }

            if (!ModelState.IsValid)
            {
                await LoadCategories(GetAuthenticatedClient(), 1, 10);
                CurrentSection = "categories";
                Message = "Dữ liệu không hợp lệ.";
                MessageType = "error";
                return Page();
            }

            try
            {
                var client = GetAuthenticatedClient();
                var json = JsonSerializer.Serialize(CategoryForm, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{BASE_API_URL}/Categories", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/Staff/StaffDashboard",
                        new { section = "categories", message = "Tạo Category thành công!", messageType = "success" });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Message = $"Lỗi khi tạo Category: {errorContent}";
                    MessageType = "error";
                }
            }
            catch (Exception ex)
            {
                Message = $"Lỗi: {ex.Message}";
                MessageType = "error";
            }

            await LoadCategories(GetAuthenticatedClient(), 1, 10);
            CurrentSection = "categories";
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteCategoryAsync(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToPage("/Login", new { message = "Phiên đăng nhập đã hết hạn.", messageType = "warning" });
            }

            if (!UserCanManageCategories())
            {
                return RedirectToPage("/Staff/StaffDashboard",
                    new { section = "categories", message = "Bạn không có quyền xóa Category.", messageType = "error" });
            }

            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.DeleteAsync($"{BASE_API_URL}/Categories/{id}");

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/Staff/StaffDashboard",
                        new { section = "categories", message = "Xóa Category thành công!", messageType = "success" });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return RedirectToPage("/Staff/StaffDashboard",
                        new { section = "categories", message = $"Lỗi khi xóa Category: {errorContent}", messageType = "error" });
                }
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Staff/StaffDashboard",
                    new { section = "categories", message = $"Lỗi: {ex.Message}", messageType = "error" });
            }
        }

        // POST Handlers for Tags
        public async Task<IActionResult> OnPostCreateTagAsync()
        {
            if (!IsAuthenticated())
            {
                return RedirectToPage("/Login", new { message = "Phiên đăng nhập đã hết hạn.", messageType = "warning" });
            }

            if (!UserCanManageTags())
            {
                return RedirectToPage("/Staff/StaffDashboard",
                    new { section = "tags", message = "Bạn không có quyền tạo Tag.", messageType = "error" });
            }

            if (!ModelState.IsValid)
            {
                await LoadTags(GetAuthenticatedClient(), 1, 10);
                CurrentSection = "tags";
                Message = "Dữ liệu không hợp lệ.";
                MessageType = "error";
                return Page();
            }

            try
            {
                var client = GetAuthenticatedClient();
                var json = JsonSerializer.Serialize(TagForm, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{BASE_API_URL}/Tags", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/Staff/StaffDashboard",
                        new { section = "tags", message = "Tạo Tag thành công!", messageType = "success" });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Message = $"Lỗi khi tạo Tag: {errorContent}";
                    MessageType = "error";
                }
            }
            catch (Exception ex)
            {
                Message = $"Lỗi: {ex.Message}";
                MessageType = "error";
            }

            await LoadTags(GetAuthenticatedClient(), 1, 10);
            CurrentSection = "tags";
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteTagAsync(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToPage("/Login", new { message = "Phiên đăng nhập đã hết hạn.", messageType = "warning" });
            }

            if (!UserCanManageTags())
            {
                return RedirectToPage("/Staff/StaffDashboard",
                    new { section = "tags", message = "Bạn không có quyền xóa Tag.", messageType = "error" });
            }

            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.DeleteAsync($"{BASE_API_URL}/Tags/{id}");

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/Staff/StaffDashboard",
                        new { section = "tags", message = "Xóa Tag thành công!", messageType = "success" });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return RedirectToPage("/Staff/StaffDashboard",
                        new { section = "tags", message = $"Lỗi khi xóa Tag: {errorContent}", messageType = "error" });
                }
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Staff/StaffDashboard",
                    new { section = "tags", message = $"Lỗi: {ex.Message}", messageType = "error" });
            }
        }

        // POST Handlers for Profile
        public async Task<IActionResult> OnPostUpdateProfileAsync()
        {
            if (!IsAuthenticated())
            {
                return RedirectToPage("/Login", new { message = "Phiên đăng nhập đã hết hạn.", messageType = "warning" });
            }

            if (!ModelState.IsValid)
            {
                CurrentSection = "profile";
                Message = "Dữ liệu không hợp lệ.";
                MessageType = "error";
                return Page();
            }

            try
            {
                var client = GetAuthenticatedClient();
                var updateRequest = new UpdateUserRequest
                {
                    UserID = CurrentUserId,
                    FullName = ProfileForm.FullName,
                    Username = ProfileForm.Username,
                    Email = ProfileForm.Email,
                    Phone = ProfileForm.Phone
                };

                var json = JsonSerializer.Serialize(updateRequest, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PutAsync($"{BASE_API_URL}/Admin/update", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/Staff/StaffDashboard",
                        new { section = "profile", message = "Cập nhật Profile thành công!", messageType = "success" });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Message = $"Lỗi khi cập nhật Profile: {errorContent}";
                    MessageType = "error";
                }
            }
            catch (Exception ex)
            {
                Message = $"Lỗi: {ex.Message}";
                MessageType = "error";
            }

            CurrentSection = "profile";
            return Page();
        }

        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            if (!IsAuthenticated())
            {
                return RedirectToPage("/Login", new { message = "Phiên đăng nhập đã hết hạn.", messageType = "warning" });
            }

            if (!ModelState.IsValid)
            {
                CurrentSection = "profile";
                Message = "Dữ liệu không hợp lệ.";
                MessageType = "error";
                return Page();
            }

            if (ChangePasswordForm.NewPassword != ChangePasswordForm.ConfirmPassword)
            {
                CurrentSection = "profile";
                Message = "Mật khẩu xác nhận không khớp.";
                MessageType = "error";
                return Page();
            }

            try
            {
                var client = GetAuthenticatedClient();
                var changePasswordRequest = new ChangePasswordRequest
                {
                    UserID = CurrentUserId,
                    OldPassword = ChangePasswordForm.OldPassword,
                    NewPassword = ChangePasswordForm.NewPassword
                };

                var json = JsonSerializer.Serialize(changePasswordRequest, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{BASE_API_URL}/Admin/change-password", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/Staff/StaffDashboard",
                        new { section = "profile", message = "Đổi mật khẩu thành công!", messageType = "success" });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Message = $"Lỗi khi đổi mật khẩu: {errorContent}";
                    MessageType = "error";
                }
            }
            catch (Exception ex)
            {
                Message = $"Lỗi: {ex.Message}";
                MessageType = "error";
            }

            CurrentSection = "profile";
            return Page();
        }

        // Helper Methods
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

        private async Task LoadDashboardStats(HttpClient client)
        {
            try
            {
                // Load user statistics
                var userStatsResponse = await client.GetAsync($"{BASE_API_URL}/Admin/statistics");
                if (userStatsResponse.IsSuccessStatusCode)
                {
                    var userStatsJson = await userStatsResponse.Content.ReadAsStringAsync();
                    var userStatsResult = JsonSerializer.Deserialize<AdminStatsResponse>(userStatsJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    if (userStatsResult?.Data != null)
                    {
                        DashboardStats.TotalMembers = userStatsResult.Data.TotalUsers;
                        DashboardStats.UsersByRole = userStatsResult.Data.UsersByRole;
                        DashboardStats.UsersByStatus = userStatsResult.Data.UsersByStatus;
                    }
                }

                // Load news statistics
                var newsStatsResponse = await client.GetAsync($"{BASE_API_URL}/NewsArticles/stats");
                if (newsStatsResponse.IsSuccessStatusCode)
                {
                    var newsStatsJson = await newsStatsResponse.Content.ReadAsStringAsync();
                    var newsStatsResult = JsonSerializer.Deserialize<NewsStatsResponse>(newsStatsJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    if (newsStatsResult?.Data != null)
                    {
                        DashboardStats.TotalNews = newsStatsResult.Data.TotalNewsArticles;
                    }
                }

                // Load category statistics
                var categoryStatsResponse = await client.GetAsync($"{BASE_API_URL}/Categories/stats");
                if (categoryStatsResponse.IsSuccessStatusCode)
                {
                    var categoryStatsJson = await categoryStatsResponse.Content.ReadAsStringAsync();
                    var categoryStatsResult = JsonSerializer.Deserialize<CategoryStatsResponse>(categoryStatsJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    if (categoryStatsResult?.Data != null)
                    {
                        DashboardStats.TotalCategories = categoryStatsResult.Data.TotalCategories;
                    }
                }

                // Load tags count
                var tagsResponse = await client.GetAsync($"{BASE_API_URL}/Tags");
                if (tagsResponse.IsSuccessStatusCode)
                {
                    var tagsJson = await tagsResponse.Content.ReadAsStringAsync();
                    var tagsResult = JsonSerializer.Deserialize<PaginatedApiResponse<TagDTO>>(tagsJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    if (tagsResult?.Pagination != null)
                    {
                        DashboardStats.TotalTags = tagsResult.Pagination.TotalItems;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail the page load
                Console.WriteLine($"Error loading dashboard stats: {ex.Message}");
            }
        }

        private async Task LoadMembers(HttpClient client, int pageIndex, int pageSize)
        {
            try
            {
                var response = await client.GetAsync($"{BASE_API_URL}/Admin/role/Member");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<AdminDto>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    if (result?.Data != null)
                    {
                        Members = result.Data.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                Message = $"Lỗi khi tải danh sách Members: {ex.Message}";
                MessageType = "error";
            }
        }

        private async Task LoadNews(HttpClient client, int pageIndex, int pageSize)
        {
            try
            {
                var queryString = $"?pageIndex={pageIndex}&pageSize={pageSize}";
                var response = await client.GetAsync($"{BASE_API_URL}/NewsArticles/admin{queryString}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<PaginatedApiResponse<NewsArticleDto>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    if (result?.Data != null)
                    {
                        NewsArticles = result.Data.ToList();
                        NewsPagination = result.Pagination;
                    }
                }
            }
            catch (Exception ex)
            {
                Message = $"Lỗi khi tải danh sách News: {ex.Message}";
                MessageType = "error";
            }
        }

        private async Task LoadCategories(HttpClient client, int pageIndex, int pageSize)
        {
            try
            {
                var queryString = $"?pageIndex={pageIndex}&pageSize={pageSize}";
                var response = await client.GetAsync($"{BASE_API_URL}/Categories/admin{queryString}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<PaginatedApiResponse<CategoryDTO>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    if (result?.Data != null)
                    {
                        Categories = result.Data.ToList();
                        CategoriesPagination = result.Pagination;
                    }
                }
            }
            catch (Exception ex)
            {
                Message = $"Lỗi khi tải danh sách Categories: {ex.Message}";
                MessageType = "error";
            }
        }

        private async Task LoadTags(HttpClient client, int pageIndex, int pageSize)
        {
            try
            {
                var queryString = $"?pageIndex={pageIndex}&pageSize={pageSize}";
                var response = await client.GetAsync($"{BASE_API_URL}/Tags{queryString}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<PaginatedApiResponse<TagDTO>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    if (result?.Data != null)
                    {
                        Tags = result.Data.ToList();
                        TagsPagination = result.Pagination;
                    }
                }
            }
            catch (Exception ex)
            {
                Message = $"Lỗi khi tải danh sách Tags: {ex.Message}";
                MessageType = "error";
            }
        }

        private async Task LoadCurrentUserProfile(HttpClient client)
        {
            try
            {
                var currentUser = GetCurrentUser();
                if (currentUser != null)
                {
                    ProfileForm = new UpdateProfileDto
                    {
                        FullName = currentUser.FullName,
                        Username = currentUser.Username,
                        Email = currentUser.Email,
                        Phone = currentUser.Phone ?? ""
                    };
                }
            }
            catch (Exception ex)
            {
                Message = $"Lỗi khi tải thông tin Profile: {ex.Message}";
                MessageType = "error";
            }
        }
    }

    // DTO Classes
    public class DashboardStatsDto
    {
        public int TotalMembers { get; set; }
        public int TotalNews { get; set; }
        public int TotalCategories { get; set; }
        public int TotalTags { get; set; }
        public Dictionary<string, int>? UsersByRole { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int>? UsersByStatus { get; set; } = new Dictionary<string, int>();
    }

    public class CreateMemberDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Phone { get; set; }
    }

    public class CreateCategoryDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class CreateTagDto
    {
        public string TagName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class UpdateProfileDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
    }

    public class ChangePasswordDto
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    // Additional classes for API calls (should match your existing models)
    public class CreateUserRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Role { get; set; } = string.Empty;
    }

    public class UpdateUserRequest
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
    }

    public class ChangePasswordRequest
    {
        public int UserID { get; set; }
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    // Response classes (these should match your existing model structure)
    public class AdminStatsResponse
    {
        public UserStatisticsResponse? Data { get; set; }
    }

    public class NewsStatsResponse
    {
        public NewsStatsDto? Data { get; set; }
    }

    public class CategoryStatsResponse
    {
        public CategoryStatsDto? Data { get; set; }
    }

    public class UserStatisticsResponse
    {
        public int TotalUsers { get; set; }
        public Dictionary<string, int>? UsersByRole { get; set; }
        public Dictionary<string, int>? UsersByStatus { get; set; }
    }

    public class NewsStatsDto
    {
        public int TotalNewsArticles { get; set; }
    }

    public class CategoryStatsDto
    {
        public int TotalCategories { get; set; }
    }

    public class AdminDto
    {
        public List<UserResponse>? Data { get; set; }
    }

    // Additional DTO classes that match your API structure
    public class NewsArticleDto
    {
        public int NewsArticleID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? AuthorName { get; set; }
        public string? CategoryName { get; set; }
        public string IsActive { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CategoryDTO
    {
        public short CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string IsActive { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class TagDTO
    {
        public int TagID { get; set; }
        public string TagName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? UsageCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UserResponse
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Role { get; set; } = string.Empty;
    }

    // Additional classes to match your project structure
    public class PaginatedApiResponse<T>
    {
        public List<T>? Data { get; set; }
        public PaginationInfo? Pagination { get; set; }
    }

    public class PaginationInfo
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }

    public class UserInfoDto
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
    }

    // LoginModel placeholder - REPLACE WITH YOUR ACTUAL LoginModel CLASS
    // This is just a placeholder to make the code compile
    // You should use your existing LoginModel from DrugUserPreventionUI.Pages
    public class LoginModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public PageContext? PageContext { get; set; }

        public LoginModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public UserInfoDto? GetCurrentUser()
        {
            // TODO: Replace with your actual JWT token parsing logic
            // This should extract user info from JWT token in cookies/headers
            return new UserInfoDto
            {
                UserID = 1,
                FullName = "Staff User",
                Username = "staff",
                Email = "staff@example.com",
                Phone = ""
            };
        }

        public bool IsAuthenticated()
        {
            // TODO: Replace with your actual authentication check
            // This should check if JWT token exists and is valid
            return true;
        }

        public string GetUserRole()
        {
            // TODO: Replace with your actual role extraction logic
            // This should extract role from JWT token claims
            return "Staff";
        }

        public string GetDisplayName()
        {
            // TODO: Replace with your actual display name logic
            return GetCurrentUser()?.FullName ?? "Staff User";
        }
    }
}