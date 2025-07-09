using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs.Appointment;
using Services.DTOs.Consultant;
using Services.IService;
using Services.Service;
using System.Security.Claims;

namespace DrugUsePrevention.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsultantController : ControllerBase
    {
        private IConsultantService _consultantService;
        public ConsultantController(IConsultantService consultantService)
        {
            _consultantService = consultantService;
        }
        [HttpGet("WorkingHour/{id}")]
        public async Task<IActionResult> GetWorkingHours([FromRoute]int id)
        {
            try
            {
                var result = await _consultantService.GetWorkingHour(id);
                if (result == null )
                {
                    return BadRequest(new { message = "Id invalid" });
                }
                return Ok(new { message = "Success", data = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { message = "An error occurred" });
            }
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateConsultant([FromBody] ConsultantRequest consultantRequest)
        {
            try
            {
                var result = await _consultantService.CreateConsultant(consultantRequest);
                if (result == null)
                {
                    return BadRequest(new { message = "Id invalid" });
                }
                return Ok(new { message = "Success", data = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { message = "An error occurred" });
            }
        }
        [HttpPut("appointment/{appointmentId}")]
        public async Task<IActionResult> AppointmentApprove([FromBody] string status, [FromRoute]int appointmentId)
        {
            try
            {
                var result = await _consultantService.ApppointmentApprove(status, appointmentId);
                return Ok(new { message = "Success", data = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllConSultant()
        {
            try
            {
                var result = await _consultantService.GetAllConSultant();

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ConsultantResponse>> GetConsultant(int id)
        {
            try
            {
                var consult = await _consultantService.GetConsultantById(id);
                if (consult == null)
                    return NotFound(new { message = $"consult with ID {id} not found" });

                return Ok(consult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the consult", error = ex.Message });
            }
        }
    }
}
