using BussinessObjects;
using Repositories.IRepository;
using Repositories.IRepository.Appointments;
using Repositories.IRepository.Consultants;
using Repositories.IRepository.Users;
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
                "Approved",
                "Pending",
                "Cancelled"
            };
            if (!allowedStatuses.Contains(status))
            {
                throw new Exception("status must be Pending, Confirmed, Completed, Cancelled");
            }

            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null)
            {
                throw new Exception("appointment not found");
            }
            appointment.Status = status;
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
            consultant.WorkingHours = consultantRequest.WorkingHours;
            consultant.User = user;
            await _consultantRepository.InsertAsync(consultant);
            await _unitOfWork.SaveAsync();

            var consultantResponse = new ConsultantResponse
            {
                ConsultantID = consultantRequest.UserId,
                Qualifications = consultantRequest.Qualifications,
                Specialty = consultantRequest.Specialty,
                WorkingHours = consultantRequest.WorkingHours
            };
            return consultantResponse;
        }

        public async Task<List<string>> GetWorkingHour(int id)
        {
            var consultant = _consultantRepository.GetById(id);
            if (consultant == null)
            {
                return null;
            }

            return consultant.WorkingHours;
        }
    }
}
