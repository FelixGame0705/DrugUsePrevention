using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs;
using Services.DTOs.Category;
using Services.DTOs.Common;
using Services.DTOs.NewArticle;
using Services.DTOs.Tags;
using Services.IService;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsArticlesController : ControllerBase
    {
        private readonly INewsArticleService _newsArticleService;

        public NewsArticlesController(INewsArticleService newsArticleService)
        {
            _newsArticleService = newsArticleService;
        }

        #region ✅ API View NewsArticles - PUBLIC ACCESS (Guest có thể xem)

        /// <summary>
        /// Get all active news articles with pagination and filtering
        /// PUBLIC: Guest, Member đều có thể xem danh sách bài viết
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedApiResponse<NewsArticleDto>>> GetNewsArticles(
            [FromQuery] NewsArticleFilterDto filter
        )
        {
            try
            {
                var result = await _newsArticleService.GetAllActiveNewsArticlesAsync(filter);
                return Ok(PaginatedApiResponse<NewsArticleDto>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Get all news articles for admin (including inactive)
        /// ADMIN: Staff, Manager, Admin có thể xem tất cả bài viết
        /// </summary>
        [HttpGet("admin")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<
            ActionResult<PaginatedApiResponse<NewsArticleDto>>
        > GetAllNewsArticlesForAdmin([FromQuery] NewsArticleFilterDto filter)
        {
            try
            {
                var result = await _newsArticleService.GetAllNewsArticlesAsync(filter);
                return Ok(PaginatedApiResponse<NewsArticleDto>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Get news article details by ID
        /// PUBLIC: Guest có thể xem chi tiết bài viết
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<NewsArticleDto>>> GetNewsArticle(int id)
        {
            try
            {
                var result = await _newsArticleService.GetNewsArticleByIdAsync(id);
                return Ok(ApiResponse<NewsArticleDto>.SuccessResult(result));
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
        /// Get news articles by category
        /// </summary>
        [HttpGet("category/{categoryId}")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedApiResponse<NewsArticleDto>>> GetNewsByCategory(
            short categoryId,
            [FromQuery] PagingRequest pagingRequest
        )
        {
            try
            {
                var result = await _newsArticleService.GetNewsArticlesByCategoryAsync(
                    categoryId,
                    pagingRequest
                );
                return Ok(PaginatedApiResponse<NewsArticleDto>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Get news articles by source
        /// </summary>
        [HttpGet("source")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedApiResponse<NewsArticleDto>>> GetNewsBySource(
            [FromQuery] string newsSource,
            [FromQuery] PagingRequest pagingRequest
        )
        {
            try
            {
                var result = await _newsArticleService.GetNewsArticlesBySourceAsync(
                    newsSource,
                    pagingRequest
                );
                return Ok(PaginatedApiResponse<NewsArticleDto>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Search news articles
        /// </summary>
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedApiResponse<NewsArticleDto>>> SearchNewsArticles(
            [FromQuery] string searchKeyword,
            [FromQuery] PagingRequest pagingRequest
        )
        {
            try
            {
                var result = await _newsArticleService.SearchNewsArticlesAsync(
                    searchKeyword,
                    pagingRequest
                );
                return Ok(PaginatedApiResponse<NewsArticleDto>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        #endregion

        #region ✅ API Create NewsArticle - Cho Staff, Manager, Admin

        /// <summary>
        /// Create news article
        /// Staff: Quản lý nội dung
        /// Manager: Quản lý tổng thể
        /// Admin: Toàn quyền
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<ActionResult<ApiResponse<NewsArticleDto>>> CreateNewsArticle(
            [FromBody] CreateNewsArticleDto createDto
        )
        {
            try
            {
                short createdBy = GetCurrentUserId();
                var result = await _newsArticleService.AddNewsArticleAsync(createDto, createdBy);
                return CreatedAtAction(
                    nameof(GetNewsArticle),
                    new { id = result.NewsArticleID },
                    ApiResponse<NewsArticleDto>.SuccessResult(result, "Tạo bài viết thành công")
                );
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
                return StatusCode(
                    500,
                    ApiResponse<string>.ErrorResult("Có lỗi xảy ra khi tạo bài viết")
                );
            }
        }

        #endregion

        #region ✅ API Update/Delete NewsArticle - Cho Staff, Manager, Admin

        /// <summary>
        /// Update news article
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<ActionResult<ApiResponse<NewsArticleDto>>> UpdateNewsArticle(
            int id,
            [FromBody] UpdateNewsArticleDto updateDto
        )
        {
            try
            {
                if (id != updateDto.NewsArticleID)
                {
                    return BadRequest(ApiResponse<string>.ErrorResult("ID không khớp"));
                }

                short updatedBy = GetCurrentUserId();
                var result = await _newsArticleService.UpdateNewsArticleAsync(updateDto, updatedBy);
                return Ok(
                    ApiResponse<NewsArticleDto>.SuccessResult(
                        result,
                        "Cập nhật bài viết thành công"
                    )
                );
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
        /// Delete news article (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteNewsArticle(int id)
        {
            try
            {
                await _newsArticleService.DeleteNewsArticleAsync(id);
                return Ok(ApiResponse<string>.SuccessResult("", "Xóa bài viết thành công"));
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

        /// <summary>
        /// Toggle news article status
        /// </summary>
        [HttpPatch("{id}/toggle-status")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<ActionResult<ApiResponse<string>>> ToggleNewsStatus(
            int id,
            [FromBody] string isActive
        )
        {
            try
            {
                short updatedBy = GetCurrentUserId();
                await _newsArticleService.UpdateNewsArticleStatusAsync(id, isActive, updatedBy);
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

        #region Statistics

        /// <summary>
        /// Get news article statistics
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<ActionResult<ApiResponse<NewsArticleStatsDto>>> GetNewsArticleStats()
        {
            try
            {
                var result = await _newsArticleService.GetNewsArticleStatsAsync();
                return Ok(ApiResponse<NewsArticleStatsDto>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        #endregion

        #region Helper Methods

        private short GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && short.TryParse(userIdClaim.Value, out short userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("Không thể xác định người dùng hiện tại");
        }

        #endregion
    }

    // ==============================================
    // CATEGORIES CONTROLLER
    // ==============================================

    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        #region ✅ API View Categories - PUBLIC ACCESS

        /// <summary>
        /// Get all active categories
        /// PUBLIC: Guest có thể xem danh mục
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedApiResponse<CategoryDTO>>> GetCategories(
            [FromQuery] PagingRequest pagingRequest
        )
        {
            try
            {
                var result = await _categoryService.GetAllActiveCategoriesAsync(pagingRequest);
                return Ok(PaginatedApiResponse<CategoryDTO>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Get all categories for admin (including inactive)
        /// </summary>
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PaginatedApiResponse<CategoryDTO>>> GetAllCategoriesForAdmin(
            [FromQuery] PagingRequest pagingRequest
        )
        {
            try
            {
                var result = await _categoryService.GetAllCategoriesAsync(pagingRequest);
                return Ok(PaginatedApiResponse<CategoryDTO>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Get category by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<CategoryDTO>>> GetCategory(short id)
        {
            try
            {
                var result = await _categoryService.GetCategoryByIdAsync(id);
                return Ok(ApiResponse<CategoryDTO>.SuccessResult(result));
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
        /// Search categories
        /// </summary>
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedApiResponse<CategoryDTO>>> SearchCategories(
            [FromQuery] string searchKeyword,
            [FromQuery] PagingRequest pagingRequest
        )
        {
            try
            {
                var result = await _categoryService.SearchCategoriesAsync(
                    searchKeyword,
                    pagingRequest
                );
                return Ok(PaginatedApiResponse<CategoryDTO>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        #endregion

        #region ✅ API Manage Categories - Admin Only

        /// <summary>
        /// Create category
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<CategoryDTO>>> CreateCategory(
            [FromBody] CreateCategoryDto createDto
        )
        {
            try
            {
                var result = await _categoryService.AddCategoryAsync(createDto);
                return CreatedAtAction(
                    nameof(GetCategory),
                    new { id = result.CategoryID },
                    ApiResponse<CategoryDTO>.SuccessResult(result, "Tạo danh mục thành công")
                );
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
        /// Update category
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<CategoryDTO>>> UpdateCategory(
            short id,
            [FromBody] UpdateCategoryDto updateDto
        )
        {
            try
            {
                if (id != updateDto.CategoryID)
                {
                    return BadRequest(ApiResponse<string>.ErrorResult("ID không khớp"));
                }

                var result = await _categoryService.UpdateCategoryAsync(updateDto);
                return Ok(
                    ApiResponse<CategoryDTO>.SuccessResult(result, "Cập nhật danh mục thành công")
                );
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
        /// Delete category
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteCategory(short id)
        {
            try
            {
                await _categoryService.DeleteCategoryAsync(id);
                return Ok(ApiResponse<string>.SuccessResult("", "Xóa danh mục thành công"));
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
        /// Get category statistics
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<CategoryStatsDto>>> GetCategoryStats()
        {
            try
            {
                var result = await _categoryService.GetCategoryStatsAsync();
                return Ok(ApiResponse<CategoryStatsDto>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        #endregion
    }

    // ==============================================
    // TAGS CONTROLLER
    // ==============================================

    [ApiController]
    [Route("api/[controller]")]
    public class TagsController : ControllerBase
    {
        private readonly ITagService _tagService;

        public TagsController(ITagService tagService)
        {
            _tagService = tagService;
        }

        #region ✅ API View Tags - PUBLIC ACCESS

        /// <summary>
        /// Get all tags
        /// PUBLIC: Guest có thể xem tags
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedApiResponse<TagDTO>>> GetTags(
            [FromQuery] PagingRequest pagingRequest
        )
        {
            try
            {
                var result = await _tagService.GetAllTagsAsync(pagingRequest);
                return Ok(PaginatedApiResponse<TagDTO>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Get tag by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<TagDTO>>> GetTag(int id)
        {
            try
            {
                var result = await _tagService.GetTagByIdAsync(id);
                return Ok(ApiResponse<TagDTO>.SuccessResult(result));
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
        /// Search tags
        /// </summary>
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedApiResponse<TagDTO>>> SearchTags(
            [FromQuery] string searchKeyword,
            [FromQuery] PagingRequest pagingRequest
        )
        {
            try
            {
                var result = await _tagService.SearchTagsAsync(searchKeyword, pagingRequest);
                return Ok(PaginatedApiResponse<TagDTO>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Get top tags by usage
        /// </summary>
        [HttpGet("top")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<TagDTO>>>> GetTopTags(
            [FromQuery] int count = 10
        )
        {
            try
            {
                var result = await _tagService.GetTopTagsAsync(count);
                return Ok(ApiResponse<List<TagDTO>>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Get tags by news article
        /// </summary>
        [HttpGet("news/{newsId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<TagDTO>>>> GetTagsByNewsArticle(int newsId)
        {
            try
            {
                var result = await _tagService.GetTagsByNewsArticleAsync(newsId);
                return Ok(ApiResponse<List<TagDTO>>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult(ex.Message));
            }
        }

        #endregion

        #region ✅ API Manage Tags - Staff, Manager, Admin

        /// <summary>
        /// Create tag
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<ActionResult<ApiResponse<TagDTO>>> CreateTag(
            [FromBody] CreateTagDto createDto
        )
        {
            try
            {
                var result = await _tagService.AddTagAsync(createDto);
                return CreatedAtAction(
                    nameof(GetTag),
                    new { id = result.TagID },
                    ApiResponse<TagDTO>.SuccessResult(result, "Tạo tag thành công")
                );
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
        /// Update tag
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<ActionResult<ApiResponse<TagDTO>>> UpdateTag(
            int id,
            [FromBody] UpdateTagDto updateDto
        )
        {
            try
            {
                if (id != updateDto.TagID)
                {
                    return BadRequest(ApiResponse<string>.ErrorResult("ID không khớp"));
                }

                var result = await _tagService.UpdateTagAsync(updateDto);
                return Ok(ApiResponse<TagDTO>.SuccessResult(result, "Cập nhật tag thành công"));
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
        /// Delete tag
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteTag(int id)
        {
            try
            {
                await _tagService.DeleteTagAsync(id);
                return Ok(ApiResponse<string>.SuccessResult("", "Xóa tag thành công"));
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

        #endregion
    }
}
