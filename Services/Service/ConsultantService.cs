using BussinessObjects;
using Microsoft.EntityFrameworkCore;
using Repositories.IRepository;
using Repositories.IRepository.Appointments;
using Repositories.IRepository.Consultants;
using Repositories.IRepository.Users;
using Repositories.Repository.Appointments;
using Services.DTOs.Appointment;
using Services.DTOs.Consultant;
using Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Service
{
    public class ConsultantService : IConsultantService
    {
        private readonly IConsultantRepository _consultantRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAppointmentRepository _appointmentRepository;
        public ConsultantService(IConsultantRepository consultantRepository, IUserRepository userRepository, IUnitOfWork unitOfWork, IAppointmentRepository appointmentRepository)
        {
            _consultantRepository = consultantRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _appointmentRepository = appointmentRepository;
        }

        public async Task<ApppointmentResponse> ApppointmentApprove(string status, int appointmentId)
        {
            var allowedStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Confirmed",
                "Completed",
                "Pending",
                "Cancelled"
            };
            if (!allowedStatuses.Contains(status))
            {
                throw new Exception("status must be Pending, Confirmed, Completed, Cancelled");
            }

            var appointment = await _appointmentRepository.Entities
                .Include(p => p.User)
                .Include(p => p.Consultant)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(p => p.AppointmentID == appointmentId);
            if (appointment == null)
            {
                throw new Exception("appointment not found");
            }
            appointment.Status = status;
            await _unitOfWork.SaveAsync();

            var appointmentResponse = new ApppointmentResponse();
            appointmentResponse.AppointmentID = appointment.AppointmentID;
            appointmentResponse.UserID = appointment.UserID;
            appointmentResponse.Username = appointment.User.Username;
            appointmentResponse.ConsultantID = appointment.ConsultantID;
            appointmentResponse.ConsultantName = appointment.Consultant.User.Username;
            appointmentResponse.ScheduledAt = appointment.ScheduledAt;
            appointmentResponse.Status = appointment.Status;
            appointmentResponse.Notes = appointment.Notes;
            appointmentResponse.CreatedAt = appointment.CreatedAt;
            return appointmentResponse;
        }

        public async Task<ConsultantResponse> CreateConsultant(ConsultantRequest consultantRequest)
        {
            if (consultantRequest == null)
            {
                return null;
            }
            var user = await _userRepository.GetByIdAsync(consultantRequest.UserId);
            if (user == null)
            {
                return null;
            }

            var consultant = new Consultant();
            consultant.Specialty = consultantRequest.Specialty;
            consultant.Qualifications = consultantRequest.Qualifications;
            string GetDayAbbreviation(int dayNumber)
            {
                return dayNumber switch
                {
                    2 => "t2", // Tuesday
                    3 => "t3", // Wednesday
                    4 => "t4", // Thursday
                    5 => "t5", // Friday
                    6 => "t6", // Saturday
                    7 => "t7", // Sunday
                    _ => ""
                };
            }

            string startDay = GetDayAbbreviation(consultantRequest.StartDate);
            string endDay = GetDayAbbreviation(consultantRequest.EndDate);

            string formattedStartHour = consultantRequest.StartHour.ToString("HH:mm"); 
            string formattedEndHour = consultantRequest.EndHour.ToString("HH:mm");   

            consultant.WorkingHours = $"{startDay}-{endDay},{formattedStartHour}-{formattedEndHour}";

            consultant.User = user;
            await _consultantRepository.InsertAsync(consultant);
            await _unitOfWork.SaveAsync();

            var consultantResponse = new ConsultantResponse
            {
                ConsultantID = consultant.ConsultantID, // Use consultant.ConsultantID which is the generated ID
                ConsultantName = user.Username,
                Qualifications = consultantRequest.Qualifications,
                Specialty = consultantRequest.Specialty,
                WorkingHours = consultant.WorkingHours // Use the newly constructed WorkingHours
            };
            return consultantResponse;
        }

        public async Task<string> GetWorkingHour(int id)
        {
            var consultant = _consultantRepository.GetById(id);
            if (consultant == null)
            {
                return null;
            }

            return consultant.WorkingHours;
        }

        public async Task<List<ConsultantResponse>> GetAllConSultant()
        {
            var consultants = await _consultantRepository.Entities
                .Include(a => a.User)
                .ToListAsync();

            List<ConsultantResponse> consultantResponses = new List<ConsultantResponse>();
            foreach (var appointment in consultants)
            {
                var appointmentResponse = new ConsultantResponse();
                appointmentResponse.ConsultantID = appointment.ConsultantID;
                appointmentResponse.ConsultantName = appointment.User.FullName;
                appointmentResponse.Qualifications = appointment.Qualifications;
                appointmentResponse.Specialty = appointment.Specialty;
                appointmentResponse.WorkingHours = appointment.WorkingHours;
                consultantResponses.Add(appointmentResponse);
            }
            return consultantResponses;
        }

        public async Task<ConsultantResponse?> GetConsultantById(int id)
        {
            var appointment = await _consultantRepository.Entities
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.ConsultantID == id);
            if (appointment == null)
            {
                return null;
            }
            var response = new ConsultantResponse
            {
                ConsultantID = appointment.ConsultantID,
                ConsultantName = appointment.User.FullName,
                Qualifications = appointment.Qualifications,
                Specialty = appointment.Specialty,
                WorkingHours = appointment.WorkingHours
            };
            return response;
        }
    }
}
