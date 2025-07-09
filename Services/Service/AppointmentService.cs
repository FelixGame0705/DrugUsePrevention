using BussinessObjects;
using Microsoft.EntityFrameworkCore;
using Repositories.IRepository;
using Repositories.IRepository.Appointments;
using Repositories.IRepository.Consultants;
using Repositories.IRepository.Users;
using Services.DTOs.Appointment;
using Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Service
{
    public class AppointmentService : IAppointmentService
    {
        private IAppointmentRepository _apointmentRepository;
        private IUserRepository _userRepository;
        private IConsultantRepository _consultantRepository;
        private IUnitOfWork _unitOfWork;
        public AppointmentService(IAppointmentRepository appointmentRepository, IUnitOfWork unitOfWork, IUserRepository userRepository, IConsultantRepository consultantRepository)
        {
            _apointmentRepository = appointmentRepository;
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _consultantRepository = consultantRepository;
        }
        public async Task<ApppointmentResponse> CreateAppointment(AppointmentRequest request, int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return null;
            }

            var appointment = new Appointment();
            appointment.User = user;
            if (request.ConsultantID != null)
            {
                var consultant = await _consultantRepository.GetByIdAsync(request.ConsultantID);
                appointment.Consultant = consultant;
            }
            appointment.ScheduledAt = request.ScheduledAt;
            appointment.Status = "Pending";
            appointment.Notes = request.Notes;
            appointment.CreatedAt = DateTime.Now;
            _apointmentRepository.Insert(appointment);
            await _unitOfWork.SaveAsync();

            var appointmentResponse = new ApppointmentResponse();
            appointmentResponse.AppointmentID = appointment.AppointmentID;
            appointmentResponse.ConsultantID = appointment.ConsultantID;
            appointmentResponse.ScheduledAt = appointment.ScheduledAt;
            appointmentResponse.Status = appointment.Status;
            appointmentResponse.Notes = appointment.Notes;
            appointmentResponse.CreatedAt = appointment.CreatedAt;
            return appointmentResponse;
        }

        public async Task<List<ApppointmentResponse>> GetAllAppointment()
        {
            var appointments = await _apointmentRepository.Entities
                .Include(a => a.User)
                .Include(a => a.Consultant)       
                .ThenInclude(c => c.User)
                .ToListAsync();

            List<ApppointmentResponse> apppointmentResponses = new List<ApppointmentResponse>();
            foreach (var appointment in appointments)
            {
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
                apppointmentResponses.Add(appointmentResponse);
            }
            return apppointmentResponses;
        }

        public async Task<ApppointmentResponse?> GetAppointmentById(int id)
        {
            var appointment = await _apointmentRepository.Entities
                .Include(p => p.User)
                .Include(p => p.Consultant)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(p => p.AppointmentID == id);
            if (appointment == null)
            {
                return null;
            }
            var response = new ApppointmentResponse
            {
                AppointmentID = appointment.AppointmentID,
                UserID = appointment.UserID,
                Username = appointment.User.Username,
                ConsultantID = appointment.ConsultantID,
                ConsultantName = appointment.Consultant.User.Username,
                ScheduledAt = appointment.ScheduledAt,
                Status = appointment.Status,
                Notes = appointment.Notes,
                CreatedAt = appointment.CreatedAt,
            };
            return response;
        }
    }
}
