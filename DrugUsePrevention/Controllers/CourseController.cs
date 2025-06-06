using BussinessObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs;
using Services.IService;

namespace DrugUsePrevention.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CourseController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpPost("createCourse")]
        public IActionResult CreateCourse([FromBody] Course request)
        {
            var course = _courseService.AddCourseAsync(request);
            if (course == null)
            {
                return BadRequest(new { message = "Failed to create course" });
            }
            return Ok(new { message = "Course created successfully" });
        }

        [HttpGet("getAllCourses")]
        public IActionResult GetAllCourses([FromQuery] PagingRequest request)
        {
            var courses = _courseService.GetAllCoursesAsync(request);
            if (courses == null)
            {
                return NotFound(new { message = "No courses found" });
            }
            // Assuming courses is a Task<BasePaginatedList<Course>>
            return Ok(courses.Result); // Awaiting the task to get the result
            //return Ok(new { message = "List of all courses" });
        }
    }
}
