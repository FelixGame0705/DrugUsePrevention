using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;

namespace DrugUserPreventionUI.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const string AUTH_API_URL = "https://localhost:7045"; // Base URL của API (bỏ dấu / cuối)

        public LoginModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public UserLoginRequest LoginForm { get; set; } = new UserLoginRequest();

        public string? Message { get; set; }
        public string? MessageType { get; set; }

        public IActionResult OnGet(string? message = null, string? messageType = null)
        {
            // Hiển thị message từ redirect (logout, etc.)
            if (!string.IsNullOrEmpty(message))
            {
                Message = message;
                MessageType = messageType ?? "info";
            }

            // Check if already logged in
            var token = HttpContext.Request.Cookies["auth_token"];
            if (!string.IsNullOrEmpty(token))
            {
                // Redirect to dashboard if already logged in
                return RedirectToPage("/CourseDashboard/CourseDashboard");
            }

            return Page();
        }
        //login
        // ✅ UPDATE: OnPostAsync method trong LoginModel.cs
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient();

                var json = JsonSerializer.Serialize(LoginForm, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var loginUrl = $"{AUTH_API_URL}/login";

                Console.WriteLine($"=== LOGIN DEBUG ===");
                Console.WriteLine($"URL: {loginUrl}");
                Console.WriteLine($"Payload: {json}");

                var response = await client.PostAsync(loginUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Response: {response.StatusCode} - {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var loginResponse = JsonSerializer.Deserialize<TokenResponseDto>(responseContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.AccessToken))
                        {
                            // ✅ Store token in cookie
                            var cookieOptions = new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = Request.IsHttps,
                                SameSite = SameSiteMode.Lax,
                                Expires = DateTime.UtcNow.AddHours(8)
                            };

                            Response.Cookies.Append("auth_token", loginResponse.AccessToken, cookieOptions);

                            // ✅ CRITICAL: Store user info in session with error handling
                            try
                            {
                                Console.WriteLine("=== STORING SESSION DATA ===");

                                var userName = loginResponse.UserName ?? loginResponse.FullName ?? "User";
                                var userRole = loginResponse.Role ?? "Member";
                                var userId = loginResponse.UserId.ToString();
                                var userEmail = loginResponse.Email ?? "";

                                HttpContext.Session.SetString("user_name", userName);
                                HttpContext.Session.SetString("user_role", userRole);
                                HttpContext.Session.SetString("user_id", userId);
                                HttpContext.Session.SetString("user_email", userEmail);

                                // ✅ FORCE session save
                                await HttpContext.Session.CommitAsync();

                                Console.WriteLine($"Session stored - Name: {userName}, Role: {userRole}");

                                // Verify session was saved
                                var testName = HttpContext.Session.GetString("user_name");
                                Console.WriteLine($"Session verification - Retrieved name: {testName}");
                            }
                            catch (Exception sessionEx)
                            {
                                Console.WriteLine($"Session error: {sessionEx.Message}");
                                // Continue anyway - session is not critical for basic functionality
                            }

                            // ✅ FIXED: Use correct redirect path
                            return RedirectToPage("/CourseDashboard", new
                            {
                                message = $"Xin chào {loginResponse.UserName ?? "User"}! Đăng nhập thành công.",
                                messageType = "success"
                            });
                        }
                        else
                        {
                            Message = "Đăng nhập thất bại: Không nhận được token từ server";
                            MessageType = "error";
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"JSON Parse Error: {ex.Message}");
                        Message = $"Lỗi parse JSON: {ex.Message}. Raw response: {responseContent}";
                        MessageType = "error";
                    }
                }
                else
                {
                    Console.WriteLine($"Login failed - Status: {response.StatusCode}");

                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        Message = "Tên đăng nhập hoặc mật khẩu không chính xác";
                    }
                    else
                    {
                        Message = $"Lỗi đăng nhập ({(int)response.StatusCode}): {response.ReasonPhrase}";
                        if (!string.IsNullOrWhiteSpace(responseContent))
                        {
                            Message += $". Chi tiết: {responseContent}";
                        }
                    }
                    MessageType = "error";
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP Request Exception: {ex.Message}");
                Message = $"Lỗi kết nối đến server: {ex.Message}";
                MessageType = "error";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Exception: {ex.Message}");
                Message = $"Lỗi không mong muốn: {ex.Message}";
                MessageType = "error";
            }

            return Page();
        }

        // Logout
        public IActionResult OnPostLogout()
        {
            try
            {
                // Clear authentication
                Response.Cookies.Delete("auth_token");

                // Clear session if available
                try
                {
                    HttpContext.Session.Clear();
                }
                catch (InvalidOperationException)
                {
                    // Session not configured, ignore
                }

                return RedirectToPage("/Login", new { message = "Đăng xuất thành công!", messageType = "success" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logout error: {ex.Message}");
                return RedirectToPage("/Login", new { message = "Đã đăng xuất", messageType = "info" });
            }
        }
    }

    // DTOs matching the API controller
    public class UserLoginRequest
    {
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; } = string.Empty;
    }

    public class TokenResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        // Additional properties that might be included
        public string? RefreshToken { get; set; }
        public DateTime? Expiration { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? FullName { get; set; }
    }

    // Additional DTOs for other auth operations
    public class RegisterUserRequest
    {
        [Required(ErrorMessage = "Tên đầy đủ là bắt buộc")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [MinLength(8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự")]
        public string Password { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        public string Gender { get; set; } = string.Empty; // Male, Female, Other

        [Required(ErrorMessage = "Vai trò là bắt buộc")]
        public string Role { get; set; } = "Member"; // Default role
    }

    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "Token là bắt buộc")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [MinLength(8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự")]
        public string NewPassword { get; set; } = string.Empty;
    }
}