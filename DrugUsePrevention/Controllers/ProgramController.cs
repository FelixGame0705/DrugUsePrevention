using BussinessObjects;
using Microsoft.AspNetCore.Mvc;
using Repositories.Paging;
using Services.DTOs.Common;
using Services.DTOs.NewArticle;
using Services.DTOs.Program;
using Services.IService;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DrugUsePrevention.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProgramController : ControllerBase
    {
        private readonly IProgramService _programService;

        public ProgramController(IProgramService programService)
        {
            _programService = programService;
        }

        /// <summary>
        /// Get all programs with pagination and filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PaginatedApiResponse<ProgramResponse>>> GetPrograms(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                var result = await _programService.GetAllProgramsAsync(pageIndex, pageSize, searchTerm, isActive);
                return Ok(PaginatedApiResponse<ProgramResponse>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving programs", error = ex.Message });
            }
        }

        /// <summary>
        /// Get program by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProgramResponse>> GetProgram(int id)
        {
            try
            {
                var program = await _programService.GetProgramByIdAsync(id);
                if (program == null)
                    return NotFound(new { message = $"Program with ID {id} not found" });

                return Ok(ApiResponse<ProgramResponse>.SuccessResult(program));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the program", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new program
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ProgramResponse>> CreateProgram([FromBody] ProgramCreateRequest dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Validate date logic
                if (dto.StartDate >= dto.EndDate)
                    return BadRequest(new { message = "Start date must be before end date" });

                var createdProgram = await _programService.CreateProgramAsync(dto);
                return CreatedAtAction(nameof(GetProgram), new { id = createdProgram.ProgramID }, ApiResponse<ProgramResponse>.SuccessResult(createdProgram));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the program", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing program
        /// </summary>
        [HttpPut]
        public async Task<ActionResult<ProgramResponse>> UpdateProgram( [FromBody] ProgramUpdateRequest dto)
        {
            try
            {

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Validate date logic
                if (dto.StartDate >= dto.EndDate)
                    return BadRequest(new { message = "Start date must be before end date" });

                var updatedProgram = await _programService.UpdateProgramAsync(dto);
                if (updatedProgram == null)
                    return NotFound(new { message = $"Program with ID {dto.ProgramID} not found" });

                return Ok(ApiResponse<ProgramResponse>.SuccessResult(updatedProgram));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the program", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a program
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProgram(int id)
        {
            try
            {
                var result = await _programService.DeleteProgramAsync(id);
                if (!result)
                    return NotFound(new { message = $"Program with ID {id} not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the program", error = ex.Message });
            }
        }

        
    }
}