using System.Security.Claims;
using BussinessObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs.Appointment;
using Services.DTOs.User;
using Services.IService;
using Services.MailUtils;
<<<<<<< HEAD
using Services.Service;
using System.Security.Claims;
=======
>>>>>>> origin/Bao2

namespace DrugUsePrevention.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private IAppointmentService _appointmentService;

        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

<<<<<<< HEAD
        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] AppointmentRequest appointmentRequest)
=======
        [HttpPost("create")]
        public async Task<IActionResult> CreateAppointment(
            [FromForm] AppointmentRequest appointmentRequest
        )
>>>>>>> origin/Bao2
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var result = await _appointmentService.CreateAppointment(
                    appointmentRequest,
                    userId
                );
                if (result == null)
                {
                    return BadRequest(new { message = "Please Login" });
                }
                return Ok(result );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAppointment()
        {
            try
            {
                var result = await _appointmentService.GetAllAppointment();
<<<<<<< HEAD
                
                return Ok( result );
=======

                return Ok(new { message = "Success", data = result });
>>>>>>> origin/Bao2
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApppointmentResponse>> GetAppointment(int id)
        {
            try
            {
                var appointment = await _appointmentService.GetAppointmentById(id);
                if (appointment == null)
                    return NotFound(new { message = $"Appointment with ID {id} not found" });

                return Ok(appointment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the program", error = ex.Message });
            }
        }
    }
}
