using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DrugUsePrevention.Pages.Home
{
    public class HomeModel : PageModel
    {
        private readonly ILogger<HomeModel> _logger;

        public HomeModel(ILogger<HomeModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            // Logic xử lý khi trang được load
            _logger.LogInformation("Trang chủ được truy cập");
        }

        public IActionResult OnPost()
        {
            // Logic xử lý POST request
            return Page();
        }
    }
}