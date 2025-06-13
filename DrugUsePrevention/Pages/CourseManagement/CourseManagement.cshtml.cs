using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace DrugUsePrevention.Pages.CourseManagement
{
    //[Authorize] // Require authentication for course management
    public class CourseManagementModel : PageModel
    {
        private readonly ILogger<CourseManagementModel> _logger;

        public CourseManagementModel(ILogger<CourseManagementModel> logger)
        {
            _logger = logger;
        }

        // Public properties for the page
        public string UserRole { get; set; } = "";
        public bool CanManageCourses { get; set; } = false;
        public bool CanApproveCourses { get; set; } = false;
        public string UserName { get; set; } = "";
        public int UserId { get; set; } = 0;

        public void OnGet()
        {
            _logger.LogInformation("Course Management page accessed at {Time}", DateTime.UtcNow);

            // Set page metadata
            ViewData["Title"] = "Quản lý Khóa học";
            ViewData["CurrentPage"] = "CourseManagement";

            // Get user information from claims
            SetUserInformation();

            // Log user access
            _logger.LogInformation("User {UserId} ({UserName}) with role {UserRole} accessed Course Management",
                UserId, UserName, UserRole);
        }

        public IActionResult OnPost()
        {
            _logger.LogInformation("POST request to Course Management page at {Time}", DateTime.UtcNow);
            return Page();
        }

        // Action handlers for specific operations
        public JsonResult OnGetUserInfo()
        {
            SetUserInformation();

            return new JsonResult(new
            {
                userId = UserId,
                userName = UserName,
                userRole = UserRole,
                canManageCourses = CanManageCourses,
                canApproveCourses = CanApproveCourses,
                isAuthenticated = User.Identity?.IsAuthenticated ?? false
            });
        }

        public JsonResult OnGetPageSettings()
        {
            return new JsonResult(new
            {
                apiBaseUrl = "/api",
                maxFileSize = 5 * 1024 * 1024, // 5MB
                allowedImageTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" },
                defaultPageSize = 10,
                maxPageSize = 100
            });
        }

        private void SetUserInformation()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                // Get user ID
                var userIdClaim = User.FindFirst("userId") ??
                                 User.FindFirst("sub") ??
                                 User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    UserId = userId;
                }

                // Get user name
                UserName = User.FindFirst("name")?.Value ??
                          User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ??
                          User.Identity.Name ??
                          "Unknown User";

                // Get user role
                UserRole = User.FindFirst("role")?.Value ??
                          User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ??
                          "Member";

                // Set permissions based on role
                CanManageCourses = UserRole == "Consultant" || UserRole == "Manager";
                CanApproveCourses = UserRole == "Manager";

                // Set ViewData for use in the page
                ViewData["UserId"] = UserId;
                ViewData["UserName"] = UserName;
                ViewData["UserRole"] = UserRole;
                ViewData["CanManageCourses"] = CanManageCourses;
                ViewData["CanApproveCourses"] = CanApproveCourses;
            }
            else
            {
                // Redirect to login if not authenticated
                _logger.LogWarning("Unauthenticated user attempted to access Course Management");
            }
        }

        // Helper method to check if user has specific permission
        public bool HasPermission(string permission)
        {
            return permission.ToLower() switch
            {
                "manage_courses" => CanManageCourses,
                "approve_courses" => CanApproveCourses,
                "view_courses" => User.Identity?.IsAuthenticated == true,
                "register_courses" => User.Identity?.IsAuthenticated == true,
                _ => false
            };
        }

        // Method to get user-specific settings
        public Dictionary<string, object> GetUserSettings()
        {
            return new Dictionary<string, object>
            {
                ["theme"] = "light", // Could be stored in user preferences
                ["language"] = "vi-VN",
                ["timezone"] = "Asia/Ho_Chi_Minh",
                ["dateFormat"] = "dd/MM/yyyy",
                ["pageSize"] = 10,
                ["showAdvancedFilters"] = CanManageCourses
            };
        }
    }
}