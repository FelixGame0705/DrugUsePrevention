using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs.Common;
using Services.DTOs.CourseContent;
using Services.DTOs.Courses;
using Services.DTOs.Dashboard;
using Services.DTOs.Registration;
using Services.IService;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CoursesController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        #region ✅ API View Course - Phân trang, filter theo ngày tháng, kỹ năng, độ tuổi

        /// <summary>
        /// [TASK] API View Course - Có phân trang, filter theo ngày tháng, kỹ năng, độ tuổi
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PaginatedApiResponse<CourseListDto>>> GetCourses([FromQuery] CourseFilterDto filter)
        {
            try
            {
                var result = await _courseService.GetCoursesAsync(filter);
                return Ok(PaginatedApiResponse<CourseListDto>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Get course details by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CourseResponseDto>>> GetCourse(int id)
        {
            try
            {
                var result = await _courseService.GetCourseByIdAsync(id);
                return Ok(ApiResponse<CourseResponseDto>.SuccessResult(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        #endregion

        #region ✅ API Create Course - Cho Consultant

        /// <summary>
        /// [TASK] API Create khóa học - Cho phép tạo khóa học, cho phép up ảnh thumbnail của khóa học (Consultant)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Consultant,Manager")]
        public async Task<ActionResult<ApiResponse<CourseResponseDto>>> CreateCourse([FromBody] CreateCourseDto createDto)
        {
            try
            {
                int createdBy = GetCurrentUserId();

                var result = await _courseService.CreateCourseAsync(createDto, createdBy);
                return CreatedAtAction(nameof(GetCourse), new { id = result.CourseID },
                    ApiResponse<CourseResponseDto>.SuccessResult(result, "Tạo khóa học thành công"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResult("Có lỗi xảy ra khi tạo khóa học"));
            }
        }

        #endregion

        #region ✅ API Update/Delete Course - Cho Consultant

        /// <summary>
        /// [TASK] API Update Course - Cho Consultant
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Consultant,Manager")]
        public async Task<ActionResult<ApiResponse<CourseResponseDto>>> UpdateCourse(int id, [FromBody] UpdateCourseDto updateDto)
        {
            try
            {
                if (id != updateDto.CourseID)
                {
                    return BadRequest(ApiResponse<string>.ErrorResult("ID không khớp"));
                }

                int updatedBy = GetCurrentUserId();
                var result = await _courseService.UpdateCourseAsync(updateDto, updatedBy);
                return Ok(ApiResponse<CourseResponseDto>.SuccessResult(result, "Cập nhật khóa học thành công"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResult("Có lỗi xảy ra khi cập nhật khóa học"));
            }
        }

        /// <summary>
        /// [TASK] API Delete Course - Xóa khóa học bằng cách thay đổi trạng thái isActive = false
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Consultant,Manager")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteCourse(int id)
        {
            try
            {
                int deletedBy = GetCurrentUserId();
                await _courseService.DeleteCourseAsync(id, deletedBy);
                return Ok(ApiResponse<string>.SuccessResult("", "Xóa khóa học thành công"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResult("Có lỗi xảy ra khi xóa khóa học"));
            }
        }

        /// <summary>
        /// Toggle course status (active/inactive)
        /// </summary>
        [HttpPatch("{id}/toggle-status")]
        [Authorize(Roles = "Consultant,Manager")]
        public async Task<ActionResult<ApiResponse<string>>> ToggleCourseStatus(int id, [FromBody] bool isActive)
        {
            try
            {
                int updatedBy = GetCurrentUserId();
                await _courseService.ToggleCourseStatusAsync(id, isActive, updatedBy);
                return Ok(ApiResponse<string>.SuccessResult("", "Cập nhật trạng thái thành công"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResult("Có lỗi xảy ra"));
            }
        }

        #endregion

        #region ✅ API Manager Approval

        /// <summary>
        /// [TASK] API cho phép Manager duyệt khóa học - isAccept = true
        /// </summary>
        [HttpPatch("{id}/approve")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<ApiResponse<string>>> ApproveCourse(int id, [FromBody] bool isAccept)
        {
            try
            {
                int approvedBy = GetCurrentUserId();
                await _courseService.ApproveCourseAsync(id, isAccept, approvedBy);

                string message = isAccept ? "Duyệt khóa học thành công" : "Từ chối khóa học thành công";
                return Ok(ApiResponse<string>.SuccessResult("", message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResult("Có lỗi xảy ra"));
            }
        }

        #endregion

        #region ✅ Course Contents Management

        /// <summary>
        /// [TASK] API list ra chi tiết các bài học trong course
        /// Hiển thị danh sách học phần và các bài học của course
        /// </summary>
        [HttpGet("{courseId}/contents")]
        public async Task<ActionResult<PaginatedApiResponse<CourseContentResponseDto>>> GetCourseContents(
            int courseId,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _courseService.GetCourseContentsAsync(courseId, pageIndex, pageSize);
                return Ok(PaginatedApiResponse<CourseContentResponseDto>.SuccessResult(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Get active contents only (for learning)
        /// </summary>
        [HttpGet("{courseId}/contents/active")]
        public async Task<ActionResult<ApiResponse<List<CourseContentResponseDto>>>> GetActiveCourseContents(int courseId)
        {
            try
            {
                var result = await _courseService.GetActiveContentsAsync(courseId);
                return Ok(ApiResponse<List<CourseContentResponseDto>>.SuccessResult(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Get specific content details
        /// </summary>
        [HttpGet("contents/{contentId}")]
        public async Task<ActionResult<ApiResponse<CourseContentResponseDto>>> GetCourseContent(int contentId)
        {
            try
            {
                var result = await _courseService.GetCourseContentByIdAsync(contentId);
                return Ok(ApiResponse<CourseContentResponseDto>.SuccessResult(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// [TASK] API create CourseContent - Cho phép up khóa học lên trong api (Consultant)
        /// </summary>
        [HttpPost("contents")]
        [Authorize(Roles = "Consultant,Manager")]
        public async Task<ActionResult<ApiResponse<CourseContentResponseDto>>> CreateCourseContent([FromBody] CreateCourseContentDto createDto)
        {
            try
            {
                var result = await _courseService.CreateCourseContentAsync(createDto);
                return CreatedAtAction(nameof(GetCourseContent), new { contentId = result.ContentID },
                    ApiResponse<CourseContentResponseDto>.SuccessResult(result, "Tạo nội dung bài học thành công"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResult("Có lỗi xảy ra khi tạo nội dung bài học"));
            }
        }

        /// <summary>
        /// [TASK] API update CourseContent - Cho Consultant
        /// </summary>
        [HttpPut("contents/{contentId}")]
        [Authorize(Roles = "Consultant,Manager")]
        public async Task<ActionResult<ApiResponse<CourseContentResponseDto>>> UpdateCourseContent(int contentId, [FromBody] UpdateCourseContentDto updateDto)
        {
            try
            {
                if (contentId != updateDto.ContentID)
                {
                    return BadRequest(ApiResponse<string>.ErrorResult("ID không khớp"));
                }

                var result = await _courseService.UpdateCourseContentAsync(updateDto);
                return Ok(ApiResponse<CourseContentResponseDto>.SuccessResult(result, "Cập nhật nội dung bài học thành công"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResult("Có lỗi xảy ra khi cập nhật nội dung bài học"));
            }
        }

        /// <summary>
        /// [TASK] API delete CourseContent - Xóa bằng cách thay đổi trạng thái isActive = false
        /// </summary>
        [HttpDelete("contents/{contentId}")]
        [Authorize(Roles = "Consultant,Manager")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteCourseContent(int contentId)
        {
            try
            {
                await _courseService.DeleteCourseContentAsync(contentId);
                return Ok(ApiResponse<string>.SuccessResult("", "Xóa nội dung bài học thành công"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResult("Có lỗi xảy ra khi xóa nội dung bài học"));
            }
        }

        /// <summary>
        /// Get next order index for new content
        /// </summary>
        [HttpGet("{courseId}/contents/next-order")]
        [Authorize(Roles = "Consultant,Manager")]
        public async Task<ActionResult<ApiResponse<int>>> GetNextOrderIndex(int courseId)
        {
            try
            {
                var result = await _courseService.GetNextOrderIndexAsync(courseId);
                return Ok(ApiResponse<int>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Reorder course contents
        /// </summary>
        [HttpPatch("{courseId}/contents/reorder")]
        [Authorize(Roles = "Consultant,Manager")]
        public async Task<ActionResult<ApiResponse<string>>> ReorderCourseContents(int courseId, [FromBody] Dictionary<int, int> contentOrderMapping)
        {
            try
            {
                await _courseService.ReorderCourseContentsAsync(courseId, contentOrderMapping);
                return Ok(ApiResponse<string>.SuccessResult("", "Sắp xếp lại thứ tự thành công"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResult("Có lỗi xảy ra"));
            }
        }

        #endregion

        #region ✅ Course Registration - Member APIs

        /// <summary>
        /// [TASK] API cho phép member đăng ký khóa học - Ghi thông tin ở bảng registrationCourse
        /// </summary>
        [HttpPost("{courseId}/register")]
        [Authorize(Roles = "Member,Consultant,Manager")]
        public async Task<ActionResult<ApiResponse<CourseRegistrationResponseDto>>> RegisterForCourse(int courseId)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _courseService.RegisterForCourseAsync(courseId, userId);
                return Ok(ApiResponse<CourseRegistrationResponseDto>.SuccessResult(result, "Đăng ký khóa học thành công"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResult("Có lỗi xảy ra khi đăng ký khóa học"));
            }
        }

        /// <summary>
        /// Unregister from course
        /// </summary>
        [HttpDelete("{courseId}/unregister")]
        [Authorize(Roles = "Member,Consultant,Manager")]
        public async Task<ActionResult<ApiResponse<string>>> UnregisterFromCourse(int courseId)
        {
            try
            {
                int userId = GetCurrentUserId();
                await _courseService.UnregisterFromCourseAsync(courseId, userId);
                return Ok(ApiResponse<string>.SuccessResult("", "Hủy đăng ký khóa học thành công"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResult("Có lỗi xảy ra"));
            }
        }

        /// <summary>
        /// Check if user can register for course
        /// </summary>
        [HttpGet("{courseId}/can-register")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> CanUserRegister(int courseId)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _courseService.CanUserRegisterAsync(courseId, userId);
                return Ok(ApiResponse<bool>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Check if user is registered for course
        /// </summary>
        [HttpGet("{courseId}/is-registered")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> IsUserRegistered(int courseId)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _courseService.IsUserRegisteredAsync(courseId, userId);
                return Ok(ApiResponse<bool>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        #endregion

        #region Registration Management & Statistics

        /// <summary>
        /// Get registrations for a course (for instructors/managers)
        /// </summary>
        [HttpGet("{courseId}/registrations")]
        [Authorize(Roles = "Consultant,Manager")]
        public async Task<ActionResult<PaginatedApiResponse<RegistrationListDto>>> GetCourseRegistrations(
            int courseId,
            [FromQuery] RegistrationFilterDto filter)
        {
            try
            {
                var result = await _courseService.GetCourseRegistrationsAsync(courseId, filter);
                return Ok(PaginatedApiResponse<RegistrationListDto>.SuccessResult(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Get enrollment statistics for a course
        /// </summary>
        [HttpGet("{courseId}/enrollment-stats")]
        [Authorize(Roles = "Consultant,Manager")]
        public async Task<ActionResult<ApiResponse<CourseEnrollmentStatsDto>>> GetCourseEnrollmentStats(int courseId)
        {
            try
            {
                var result = await _courseService.GetCourseEnrollmentStatsAsync(courseId);
                return Ok(ApiResponse<CourseEnrollmentStatsDto>.SuccessResult(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get current user ID from JWT token
        /// TODO: Implement based on your authentication system
        /// </summary>
        private int GetCurrentUserId()
        {
            // Example implementation - adjust based on your JWT setup
            var userIdClaim = User.FindFirst("userId") ?? User.FindFirst("sub");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }

            throw new UnauthorizedAccessException("Không thể xác định người dùng hiện tại");
        }

        #endregion
    }

    // ==============================================
    // USER LEARNING CONTROLLER (Additional APIs)
    // ==============================================

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LearningController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public LearningController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        /// <summary>
        /// Get user's learning dashboard
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<ActionResult<ApiResponse<UserLearningDashboardDto>>> GetUserDashboard()
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _courseService.GetUserDashboardAsync(userId);
                return Ok(ApiResponse<UserLearningDashboardDto>.SuccessResult(result));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResult("Có lỗi xảy ra"));
            }
        }

        /// <summary>
        /// Get user's course registrations
        /// </summary>
        [HttpGet("my-courses")]
        public async Task<ActionResult<PaginatedApiResponse<RegistrationListDto>>> GetMyRegistrations([FromQuery] RegistrationFilterDto filter)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _courseService.GetUserRegistrationsAsync(userId, filter);
                return Ok(PaginatedApiResponse<RegistrationListDto>.SuccessResult(result));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResult("Có lỗi xảy ra"));
            }
        }

        /// <summary>
        /// Update learning progress
        /// </summary>
        [HttpPatch("progress")]
        public async Task<ActionResult<ApiResponse<CourseRegistrationResponseDto>>> UpdateProgress([FromBody] UpdateProgressDto updateDto)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _courseService.UpdateProgressAsync(updateDto, userId);
                return Ok(ApiResponse<CourseRegistrationResponseDto>.SuccessResult(result, "Cập nhật tiến độ thành công"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResult(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResult("Có lỗi xảy ra"));
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("userId") ?? User.FindFirst("sub");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }

            throw new UnauthorizedAccessException("Không thể xác định người dùng hiện tại");
        }
    }
}