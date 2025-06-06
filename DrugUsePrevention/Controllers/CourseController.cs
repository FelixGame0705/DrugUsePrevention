using BussinessObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories.Paging;
using Services.DTOs;
using Services.DTOs.Course;
using Services.DTOs.CourseContent;
using Services.DTOs.File;
using Services.IService;
using System.Security.Claims;

namespace DrugUsePrevention.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication for all endpoints
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly IFileUploadService _fileUploadService; // NEW
        private readonly ILogger<CoursesController> _logger;

        public CoursesController(
            ICourseService courseService,
            IFileUploadService fileUploadService, // NEW
            ILogger<CoursesController> logger)
        {
            _courseService = courseService;
            _fileUploadService = fileUploadService; // NEW
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách khóa học với phân trang và lọc
        /// </summary>
        /// <param name="filter">Bộ lọc tìm kiếm</param>
        /// <returns>Danh sách khóa học</returns>
        [HttpGet]
        [AllowAnonymous] // Allow guests to view courses
        public async Task<ActionResult<BasePaginatedList<CourseListDto>>> GetCourses([FromQuery] CourseFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Getting courses with filter: {@Filter}", filter);

                var result = await _courseService.GetCoursesAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting courses");
                return StatusCode(500, "Đã xảy ra lỗi hệ thống");
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết khóa học
        /// </summary>
        /// <param name="id">ID khóa học</param>
        /// <returns>Thông tin chi tiết khóa học</returns>
        [HttpGet("{id}")]
        [AllowAnonymous] // Allow guests to view course details
        public async Task<ActionResult<CourseResponseDto>> GetCourse(int id)
        {
            try
            {
                _logger.LogInformation("Getting course with ID: {CourseId}", id);

                var result = await _courseService.GetCourseByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Course not found: {CourseId}", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting course {CourseId}", id);
                return StatusCode(500, "Đã xảy ra lỗi hệ thống");
            }
        }

        /// <summary>
        /// Tạo khóa học mới
        /// </summary>
        /// <param name="createDto">Thông tin khóa học mới</param>
        /// <returns>Thông tin khóa học đã tạo</returns>
        [HttpPost]
        [Authorize(Roles = "Staff,Manager,Admin")] // Only staff and above can create courses
        public async Task<ActionResult<CourseResponseDto>> CreateCourse([FromBody] CreateCourseDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                _logger.LogInformation("Creating course by user {UserId}: {@CreateDto}", userId, createDto);

                var result = await _courseService.CreateCourseAsync(createDto, userId);

                return CreatedAtAction(nameof(GetCourse), new { id = result.CourseID }, result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid argument while creating course: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation while creating course: {Message}", ex.Message);
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating course");
                return StatusCode(500, "Đã xảy ra lỗi hệ thống");
            }
        }

        /// <summary>
        /// Cập nhật thông tin khóa học
        /// </summary>
        /// <param name="id">ID khóa học</param>
        /// <param name="updateDto">Thông tin cập nhật</param>
        /// <returns>Thông tin khóa học đã cập nhật</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<ActionResult<CourseResponseDto>> UpdateCourse(int id, [FromBody] UpdateCourseDto updateDto)
        {
            try
            {
                if (id != updateDto.CourseID)
                {
                    return BadRequest("ID khóa học không khớp");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                _logger.LogInformation("Updating course {CourseId} by user {UserId}: {@UpdateDto}", id, userId, updateDto);

                var result = await _courseService.UpdateCourseAsync(updateDto, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Course not found for update: {CourseId}", id);
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation while updating course {CourseId}: {Message}", id, ex.Message);
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating course {CourseId}", id);
                return StatusCode(500, "Đã xảy ra lỗi hệ thống");
            }
        }

        /// <summary>
        /// Xóa khóa học
        /// </summary>
        /// <param name="id">ID khóa học</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager,Admin")] // Only managers and admins can delete
        public async Task<IActionResult> DeleteCourse(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("Deleting course {CourseId} by user {UserId}", id, userId);

                await _courseService.DeleteCourseAsync(id, userId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Course not found for deletion: {CourseId}", id);
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Cannot delete course {CourseId}: {Message}", id, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting course {CourseId}", id);
                return StatusCode(500, "Đã xảy ra lỗi hệ thống");
            }
        }

        /// <summary>
        /// Kích hoạt/tạm dừng khóa học
        /// </summary>
        /// <param name="id">ID khóa học</param>
        /// <param name="request">Trạng thái kích hoạt</param>
        /// <returns>Kết quả thay đổi trạng thái</returns>
        [HttpPatch("{id}/toggle-status")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> ToggleCourseStatus(int id, [FromBody] ToggleStatusRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("Toggling course {CourseId} status to {IsActive} by user {UserId}", id, request.IsActive, userId);

                await _courseService.ToggleCourseStatusAsync(id, request.IsActive, userId);

                string message = request.IsActive ? "Đã kích hoạt khóa học thành công" : "Đã tạm dừng khóa học thành công";
                return Ok(new { Message = message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Course not found for status toggle: {CourseId}", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while toggling course {CourseId} status", id);
                return StatusCode(500, "Đã xảy ra lỗi hệ thống");
            }
        }

        /// <summary>
        /// Phê duyệt/từ chối khóa học
        /// </summary>
        /// <param name="id">ID khóa học</param>
        /// <param name="request">Trạng thái phê duyệt</param>
        /// <returns>Kết quả phê duyệt</returns>
        [HttpPatch("{id}/approve")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> ApproveCourse(int id, [FromBody] ApprovalRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("Approving course {CourseId} with status {IsAccept} by user {UserId}", id, request.IsAccept, userId);

                await _courseService.ApproveCourseAsync(id, request.IsAccept, userId);

                string message = request.IsAccept ? "Đã phê duyệt khóa học thành công" : "Đã từ chối khóa học";
                return Ok(new { Message = message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Course not found for approval: {CourseId}", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while approving course {CourseId}", id);
                return StatusCode(500, "Đã xảy ra lỗi hệ thống");
            }
        }

        #region Course Content Management

        /// <summary>
        /// Lấy danh sách nội dung bài học đang hoạt động trong khóa học
        /// </summary>
        /// <param name="courseId">ID khóa học</param>
        /// <returns>Danh sách nội dung bài học đang hoạt động</returns>
        [HttpGet("{courseId}/contents/active")]
        [AllowAnonymous] // Allow guests to view active course contents
        public async Task<ActionResult<List<CourseContentResponseDto>>> GetActiveCourseContents(int courseId)
        {
            try
            {
                _logger.LogInformation("Getting active contents for course {CourseId}", courseId);

                var result = await _courseService.GetActiveContentsAsync(courseId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Course not found for active contents: {CourseId}", courseId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting active contents for course {CourseId}", courseId);
                return StatusCode(500, "Đã xảy ra lỗi hệ thống");
            }
        }

        /// <summary>
        /// Lấy số thứ tự tiếp theo cho nội dung mới
        /// </summary>
        /// <param name="courseId">ID khóa học</param>
        /// <returns>Số thứ tự tiếp theo</returns>
        [HttpGet("{courseId}/contents/next-order")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<ActionResult<int>> GetNextOrderIndex(int courseId)
        {
            try
            {
                _logger.LogInformation("Getting next order index for course {CourseId}", courseId);

                var nextOrder = await _courseService.GetNextOrderIndexAsync(courseId);
                return Ok(nextOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting next order index for course {CourseId}", courseId);
                return StatusCode(500, "Đã xảy ra lỗi hệ thống");
            }
        }

        /// <summary>
        /// Lấy danh sách nội dung bài học trong khóa học
        /// </summary>
        /// <param name="courseId">ID khóa học</param>
        /// <param name="pageIndex">Trang hiện tại</param>
        /// <param name="pageSize">Số bài học trên mỗi trang</param>
        /// <returns>Danh sách nội dung bài học</returns>
        [HttpGet("{courseId}/contents")]
        [AllowAnonymous] // Allow guests to view course contents
        public async Task<ActionResult<BasePaginatedList<CourseContentResponseDto>>> GetCourseContents(
            int courseId,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Getting contents for course {CourseId}, page {PageIndex}, size {PageSize}",
                    courseId, pageIndex, pageSize);

                var result = await _courseService.GetCourseContentsAsync(courseId, pageIndex, pageSize);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Course not found for contents: {CourseId}", courseId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting contents for course {CourseId}", courseId);
                return StatusCode(500, "Đã xảy ra lỗi hệ thống");
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết nội dung bài học
        /// </summary>
        /// <param name="courseId">ID khóa học</param>
        /// <param name="contentId">ID nội dung bài học</param>
        /// <returns>Thông tin chi tiết nội dung bài học</returns>
        [HttpGet("{courseId}/contents/{contentId}")]
        [AllowAnonymous]
        public async Task<ActionResult<CourseContentResponseDto>> GetCourseContent(int courseId, int contentId)
        {
            try
            {
                _logger.LogInformation("Getting content {ContentId} for course {CourseId}", contentId, courseId);

                var result = await _courseService.GetCourseContentByIdAsync(contentId);

                // Verify content belongs to the specified course
                if (result.CourseID != courseId)
                {
                    return BadRequest("Nội dung không thuộc khóa học này");
                }

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Content not found: {ContentId} for course {CourseId}", contentId, courseId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting content {ContentId} for course {CourseId}", contentId, courseId);
                return StatusCode(500, "Đã xảy ra lỗi hệ thống");
            }
        }

        /// <summary>
        /// Tạo nội dung bài học mới
        /// </summary>
        /// <param name="courseId">ID khóa học</param>
        /// <param name="createDto">Thông tin nội dung bài học mới</param>
        /// <returns>Thông tin nội dung bài học đã tạo</returns>
        [HttpPost("{courseId}/contents")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<ActionResult<CourseContentResponseDto>> CreateCourseContent(
            int courseId,
            [FromBody] CreateCourseContentDto createDto)
        {
            try
            {
                if (courseId != createDto.CourseID)
                {
                    return BadRequest("ID khóa học không khớp");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                _logger.LogInformation("Creating content for course {CourseId} by user {UserId}: {@CreateDto}",
                    courseId, userId, createDto);

                var result = await _courseService.CreateCourseContentAsync(createDto);

                return CreatedAtAction(nameof(GetCourseContent),
                    new { courseId = courseId, contentId = result.ContentID }, result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Course not found for content creation: {CourseId}", courseId);
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation while creating content for course {CourseId}: {Message}", courseId, ex.Message);
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating content for course {CourseId}", courseId);
                return StatusCode(500, "Đã xảy ra lỗi hệ thống");
            }
        }

        /// <summary>
        /// Cập nhật nội dung bài học
        /// </summary>
        /// <param name="courseId">ID khóa học</param>
        /// <param name="contentId">ID nội dung bài học</param>
        /// <param name="updateDto">Thông tin cập nhật</param>
        /// <returns>Thông tin nội dung bài học đã cập nhật</returns>
        [HttpPut("{courseId}/contents/{contentId}")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<ActionResult<CourseContentResponseDto>> UpdateCourseContent(
            int courseId,
            int contentId,
            [FromBody] UpdateCourseContentDto updateDto)
        {
            try
            {
                if (contentId != updateDto.ContentID)
                {
                    return BadRequest("ID nội dung không khớp");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                _logger.LogInformation("Updating content {ContentId} for course {CourseId} by user {UserId}: {@UpdateDto}",
                    contentId, courseId, userId, updateDto);

                var result = await _courseService.UpdateCourseContentAsync(updateDto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Content not found for update: {ContentId}", contentId);
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation while updating content {ContentId}: {Message}", contentId, ex.Message);
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating content {ContentId} for course {CourseId}",
                    contentId, courseId);
                return StatusCode(500, "Đã xảy ra lỗi hệ thống");
            }
        }

        /// <summary>
        /// Xóa nội dung bài học
        /// </summary>
        /// <param name="courseId">ID khóa học</param>
        /// <param name="contentId">ID nội dung bài học</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("{courseId}/contents/{contentId}")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<IActionResult> DeleteCourseContent(int courseId, int contentId)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("Deleting content {ContentId} for course {CourseId} by user {UserId}",
                    contentId, courseId, userId);

                await _courseService.DeleteCourseContentAsync(contentId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Content not found for deletion: {ContentId}", contentId);
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Cannot delete content {ContentId}: {Message}", contentId, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting content {ContentId} for course {CourseId}",
                    contentId, courseId);
                return StatusCode(500, "Đã xảy ra lỗi hệ thống");
            }
        }

        /// <summary>
        /// Sắp xếp lại thứ tự nội dung bài học
        /// </summary>
        /// <param name="courseId">ID khóa học</param>
        /// <param name="orderMapping">Mapping giữa ContentID và OrderIndex mới</param>
        /// <returns>Kết quả sắp xếp lại</returns>
        [HttpPatch("{courseId}/contents/reorder")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<IActionResult> ReorderCourseContents(
            int courseId,
            [FromBody] Dictionary<int, int> orderMapping)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("Reordering contents for course {CourseId} by user {UserId}: {@OrderMapping}",
                    courseId, userId, orderMapping);

                await _courseService.ReorderCourseContentsAsync(courseId, orderMapping);
                return Ok(new { Message = "Sắp xếp lại thứ tự nội dung thành công" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Course not found for reordering: {CourseId}", courseId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while reordering contents for course {CourseId}", courseId);
                return StatusCode(500, "Đã xảy ra lỗi hệ thống");
            }
        }

        #endregion

        #region Course Registration Management

        /// <summary>
        /// Đăng ký khóa học
        /// </summary>
        /// <param name="courseId">ID khóa học</param>
        /// <param name="createDto">Thông tin đăng ký</param>
        /// <returns>Thông tin đăng ký đã tạo</returns>
        [HttpPost("{courseId}/register")]
        [Authorize(Roles = "Member,Staff,Consultant,Manager,Admin")]
        public async Task<ActionResult<CourseRegistrationResponseDto>> RegisterForCourse(
            int courseId,
            [FromBody] CreateRegistrationDto createDto)
        {
            try
            {
                if (courseId != createDto.CourseID)
                {
                    return BadRequest("ID khóa học không khớp");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                _logger.LogInformation("User {UserId} registering for course {CourseId}", userId, courseId);

                var result = await _courseService.RegisterForCourseAsync(courseId, userId);
                return CreatedAtAction(nameof(GetRegistration),
                    new { courseId = courseId, userId = userId }, result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Course not found for registration: {CourseId}", courseId);
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid registration attempt: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid user for registration: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while registering for course {CourseId}", courseId);
                return StatusCode(500, "Đã xảy ra lỗi hệ thống");
            }
        }

        /// <summary>
        /// Hủy đăng ký khóa học
        /// </summary>
        /// <param name="courseId">ID khóa học</param>
        /// <returns>Kết quả hủy đăng ký</returns>
        [HttpDelete("{courseId}/unregister")]
        [Authorize(Roles = "Member,Staff,Consultant,Manager,Admin")]
        public async Task<IActionResult> UnregisterFromCourse(int courseId)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("User {UserId} unregistering from course {CourseId}", userId, courseId);

                await _courseService.UnregisterFromCourseAsync(courseId, userId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Registration not found for unregistering: {CourseId}", courseId);
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Cannot unregister from course {CourseId}: {Message}", courseId, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while unregistering from course {CourseId}", courseId);
                return StatusCode(500, "Đã xảy ra lỗi hệ thống");
            }
        }

        /// <summary>
        /// Lấy thông tin đăng ký của user hiện tại
        /// </summary>
        /// <param name="courseId">ID khóa học</param>
        /// <param name="userId">ID người dùng (optional, default current user)</param>
        /// <returns>Thông tin đăng ký</returns>
        [HttpGet("{courseId}/registration")]
        [Authorize]
        public async Task<ActionResult<CourseRegistrationResponseDto>> GetRegistration(int courseId, [FromQuery] int? userId = null)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var targetUserId = userId ?? currentUserId;

                // Check permission: only user themselves or staff+ can view registration
                if (targetUserId != currentUserId && !User.IsInRole("Staff") && !User.IsInRole("Manager") && !User.IsInRole("Admin"))
                {
                    return Forbid("Bạn không có quyền xem đăng ký này");
                }

                _logger.LogInformation("Getting registration for user {UserId} in course {CourseId}", targetUserId, courseId);

                var result = await _courseService.GetRegistrationAsync(courseId, targetUserId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Registration not found: user {UserId} course {CourseId}", userId, courseId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting registration for course {CourseId}", courseId);
                return StatusCode(500, "Đã xảy ra lỗi hệ thống");
            }
        }

        /// <summary>
        /// Kiểm tra user đã đăng ký khóa học chưa
        /// </summary>
        /// <param name="courseId">ID khóa học</param>
        /// <returns>True nếu đã đăng ký</returns>
        [HttpGet("{courseId}/is-registered")]
        [Authorize]
        public async Task<ActionResult<bool>> IsUserRegistered(int courseId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var isRegistered = await _courseService.IsUserRegisteredAsync(courseId, userId);
                return Ok(isRegistered);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking registration status for course {CourseId}", courseId);
                return StatusCode(500, "Đã xảy ra lỗi hệ thống");
            }
        }

        /// <summary>
        /// Kiểm tra user có thể đăng ký khóa học không
        /// </summary>
        /// <param name="courseId">ID khóa học</param>
        /// <returns>True nếu có thể đăng ký</returns>
        [HttpGet("{courseId}/can-register")]
        [Authorize]
        public async Task<ActionResult<bool>> CanUserRegister(int courseId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var canRegister = await _courseService.CanUserRegisterAsync(courseId, userId);
                return Ok(canRegister);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking registration eligibility for course {CourseId}", courseId);
                return StatusCode(500, "Đã xảy ra lỗi hệ thống");
            }
        }

        /// <summary>
        /// Lấy danh sách đăng ký của một khóa học (cho Manager/Admin)
        /// </summary>
        /// <param name="courseId">ID khóa học</param>
        /// <param name="filter">Bộ lọc</param>
        /// <returns>Danh sách đăng ký</returns>
        [HttpGet("{courseId}/registrations")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<ActionResult<BasePaginatedList<RegistrationListDto>>> GetCourseRegistrations(
            int courseId,
            [FromQuery] RegistrationFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Getting registrations for course {CourseId} with filter: {@Filter}", courseId, filter);

                var result = await _courseService.GetCourseRegistrationsAsync(courseId, filter);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Course not found for registrations: {CourseId}", courseId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting registrations for course {CourseId}", courseId);
                return StatusCode(500, "Đã xảy ra lỗi hệ thống");
            }
        }

        /// <summary>
        /// Lấy thống kê đăng ký của khóa học (cho Manager/Admin)
        /// </summary>
        /// <param name="courseId">ID khóa học</param>
        /// <returns>Thống kê đăng ký</returns>
        [HttpGet("{courseId}/enrollment-stats")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<ActionResult<CourseEnrollmentStatsDto>> GetCourseEnrollmentStats(int courseId)
        {
            try
            {
                _logger.LogInformation("Getting enrollment stats for course {CourseId}", courseId);

                var result = await _courseService.GetCourseEnrollmentStatsAsync(courseId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Course not found for enrollment stats: {CourseId}", courseId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting enrollment stats for course {CourseId}", courseId);
                return StatusCode(500, "Đã xảy ra lỗi hệ thống");
            }
        }

        /// <summary>
        /// Cập nhật tiến độ học
        /// </summary>
        /// <param name="courseId">ID khóa học</param>
        /// <param name="updateDto">Thông tin cập nhật tiến độ</param>
        /// <returns>Thông tin đăng ký đã cập nhật</returns>
        [HttpPatch("{courseId}/progress")]
        [Authorize]
        public async Task<ActionResult<CourseRegistrationResponseDto>> UpdateProgress(
            int courseId,
            [FromBody] UpdateProgressDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                _logger.LogInformation("User {UserId} updating progress for course {CourseId}: {@UpdateDto}",
                    userId, courseId, updateDto);

                var result = await _courseService.UpdateProgressAsync(updateDto, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Registration not found for progress update: {Message}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized progress update attempt: {Message}", ex.Message);
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating progress for course {CourseId}", courseId);
                return StatusCode(500, "Đã xảy ra lỗi hệ thống");
            }
        }

        #endregion

        #region File Upload Management

        /// <summary>
        /// Upload thumbnail cho khóa học
        /// </summary>
        /// <param name="courseId">ID khóa học</param>
        /// <param name="file">File thumbnail</param>
        /// <returns>Thông tin file đã upload</returns>
        [HttpPost("{courseId}/upload-thumbnail")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<ActionResult<FileUploadResponseDto>> UploadThumbnail(
            int courseId,
            [FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("File thumbnail không được để trống");
                }

                var userId = GetCurrentUserId();
                _logger.LogInformation("User {UserId} uploading thumbnail for course {CourseId}", userId, courseId);

                var result = await _fileUploadService.UploadThumbnailAsync(file, courseId, userId);

                // Update course thumbnail URL
                var course = await _courseService.GetCourseByIdAsync(courseId);
                // You might want to add an UpdateThumbnailAsync method to CourseService

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Course not found for thumbnail upload: {CourseId}", courseId);
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid file for thumbnail upload: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading thumbnail for course {CourseId}", courseId);
                return StatusCode(500, "Đã xảy ra lỗi hệ thống");
            }
        }

        /// <summary>
        /// Upload file cho course content
        /// </summary>
        /// <param name="courseId">ID khóa học</param>
        /// <param name="contentId">ID nội dung</param>
        /// <param name="file">File nội dung</param>
        /// <param name="fileType">Loại file (video, audio, document)</param>
        /// <returns>Thông tin file đã upload</returns>
        [HttpPost("{courseId}/contents/{contentId}/upload-file")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<ActionResult<FileUploadResponseDto>> UploadContentFile(
            int courseId,
            int contentId,
            [FromForm] IFormFile file,
            [FromForm] string fileType)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("File nội dung không được để trống");
                }

                if (string.IsNullOrEmpty(fileType))
                {
                    return BadRequest("Loại file không được để trống");
                }

                var userId = GetCurrentUserId();
                _logger.LogInformation("User {UserId} uploading {FileType} file for content {ContentId} in course {CourseId}",
                    userId, fileType, contentId, courseId);

                var result = await _fileUploadService.UploadContentFileAsync(file, contentId, fileType, userId);

                // You might want to update the CourseContent with the file URL

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid file for content upload: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading file for content {ContentId}", contentId);
                return StatusCode(500, "Đã xảy ra lỗi hệ thống");
            }
        }

        /// <summary>
        /// Upload multiple files
        /// </summary>
        /// <param name="courseId">ID khóa học</param>
        /// <param name="files">Danh sách files</param>
        /// <param name="fileType">Loại file</param>
        /// <returns>Danh sách thông tin files đã upload</returns>
        [HttpPost("{courseId}/upload-multiple")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<ActionResult<List<FileUploadResponseDto>>> UploadMultipleFiles(
            int courseId,
            [FromForm] List<IFormFile> files,
            [FromForm] string fileType)
        {
            try
            {
                if (files == null || !files.Any())
                {
                    return BadRequest("Danh sách file không được để trống");
                }

                var userId = GetCurrentUserId();
                _logger.LogInformation("User {UserId} uploading {FileCount} files for course {CourseId}",
                    userId, files.Count, courseId);

                var results = await _fileUploadService.UploadMultipleFilesAsync(files, "Course", courseId, fileType, userId);

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading multiple files for course {CourseId}", courseId);
                return StatusCode(500, "Đã xảy ra lỗi hệ thống");
            }
        }

        /// <summary>
        /// Lấy thông tin file
        /// </summary>
        /// <param name="fileId">ID file</param>
        /// <returns>Thông tin file</returns>
        [HttpGet("files/{fileId}")]
        [AllowAnonymous]
        public async Task<ActionResult<FileUploadResponseDto>> GetFileInfo(int fileId)
        {
            try
            {
                var result = await _fileUploadService.GetFileInfoAsync(fileId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting file info {FileId}", fileId);
                return StatusCode(500, "Đã xảy ra lỗi hệ thống");
            }
        }

        /// <summary>
        /// Xóa file
        /// </summary>
        /// <param name="fileId">ID file</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("files/{fileId}")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<IActionResult> DeleteFile(int fileId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _fileUploadService.DeleteFileAsync(fileId, userId);

                if (success)
                {
                    return NoContent();
                }
                else
                {
                    return NotFound("File không tồn tại");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting file {FileId}", fileId);
                return StatusCode(500, "Đã xảy ra lỗi hệ thống");
            }
        }

        /// <summary>
        /// Lấy danh sách files của entity
        /// </summary>
        /// <param name="courseId">ID khóa học</param>
        /// <returns>Danh sách files</returns>
        [HttpGet("{courseId}/files")]
        [Authorize]
        public async Task<ActionResult<List<FileUploadResponseDto>>> GetCourseFiles(int courseId)
        {
            try
            {
                var results = await _fileUploadService.GetEntityFilesAsync("Course", courseId);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting files for course {CourseId}", courseId);
                return StatusCode(500, "Đã xảy ra lỗi hệ thống");
            }
        }

        #endregion

        #region User Dashboard & Learning Management

        /// <summary>
        /// Lấy dashboard học tập của user hiện tại
        /// </summary>
        /// <returns>Dashboard học tập</returns>
        [HttpGet("~/api/users/me/dashboard")]
        [Authorize]
        public async Task<ActionResult<UserLearningDashboardDto>> GetMyDashboard()
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("Getting learning dashboard for user {UserId}", userId);

                var result = await _courseService.GetUserDashboardAsync(userId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("User not found for dashboard: {UserId}", GetCurrentUserId());
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user dashboard");
                return StatusCode(500, "Đã xảy ra lỗi hệ thống");
            }
        }

        /// <summary>
        /// Lấy danh sách khóa học đã đăng ký của user hiện tại
        /// </summary>
        /// <param name="filter">Bộ lọc</param>
        /// <returns>Danh sách khóa học đã đăng ký</returns>
        [HttpGet("~/api/users/me/registrations")]
        [Authorize]
        public async Task<ActionResult<BasePaginatedList<RegistrationListDto>>> GetMyRegistrations([FromQuery] RegistrationFilterDto filter)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("Getting registrations for user {UserId} with filter: {@Filter}", userId, filter);

                var result = await _courseService.GetUserRegistrationsAsync(userId, filter);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("User not found for registrations: {UserId}", GetCurrentUserId());
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user registrations");
                return StatusCode(500, "Đã xảy ra lỗi hệ thống");
            }
        }

        /// <summary>
        /// Lấy danh sách khóa học đã đăng ký của user cụ thể (cho Manager/Admin)
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="filter">Bộ lọc</param>
        /// <returns>Danh sách khóa học đã đăng ký</returns>
        [HttpGet("~/api/users/{userId}/registrations")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<ActionResult<BasePaginatedList<RegistrationListDto>>> GetUserRegistrations(
            int userId,
            [FromQuery] RegistrationFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Getting registrations for user {UserId} with filter: {@Filter}", userId, filter);

                var result = await _courseService.GetUserRegistrationsAsync(userId, filter);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("User not found for registrations: {UserId}", userId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user {UserId} registrations", userId);
                return StatusCode(500, "Đã xảy ra lỗi hệ thống");
            }
        }

        #endregion

        #region Private Methods & Request Models

        /// <summary>
        /// Lấy ID người dùng hiện tại từ JWT token
        /// </summary>
        /// <returns>User ID</returns>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("Không thể xác định người dùng hiện tại");
        }

        /// <summary>
        /// Lấy role của người dùng hiện tại
        /// </summary>
        /// <returns>User role</returns>
        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "";
        }

        #endregion
    }

    // Request models for simple operations
    public class ToggleStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class ApprovalRequest
    {
        public bool IsAccept { get; set; }
    }
}
