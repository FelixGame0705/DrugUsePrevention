using BussinessObjects;
using Services.DTOs.Appointment;
using Services.DTOs.Consultant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IService
{
    public interface IConsultantService
    {
        Task<List<string>> GetWorkingHour(int id);
        Task<ConsultantResponse> CreateConsultant(ConsultantRequest consultantRequest);
        Task<ApppointmentResponse> ApppointmentApprove(string status, int appointmentId);
    }
}
