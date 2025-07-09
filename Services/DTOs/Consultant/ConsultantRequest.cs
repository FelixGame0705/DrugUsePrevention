using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTOs.Consultant
{
    public class ConsultantRequest
    {
        public int UserId { get; set; }
        public string Qualifications { get; set; }
        public string Specialty { get; set; }
        [Range(2, 7, ErrorMessage = "Start Date must be between 2 and 7.")]
        public int StartDate { get; set; }
        [Range(2, 7, ErrorMessage = "End Date must be between 2 and 7.")]
        public int EndDate { get; set; }
        public TimeOnly StartHour { get; set; }
        public TimeOnly EndHour { get; set; }

        //public string WorkingHours { get; set; }
    }
}
