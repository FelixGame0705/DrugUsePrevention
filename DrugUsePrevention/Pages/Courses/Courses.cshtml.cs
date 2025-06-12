using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DrugUsePrevention.Pages.Courses
{
    public class CoursesModel : PageModel
    {
        private readonly ILogger<CoursesModel> _logger;

        public CoursesModel(ILogger<CoursesModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            _logger.LogInformation("Courses page accessed");
        }
    }
}