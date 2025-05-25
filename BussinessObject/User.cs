using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessObjects
{
    public class User
    {
        [Key]
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Phone { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; } // Male, Female, Other
        public string Role { get; set; } = "Guest";
        public DateTime CreatedAt { get; set; }
        public virtual Consultant? ConsultantProfile { get; set; }

        public virtual ICollection<Course> CreatedCourses { get; set; }
        public virtual ICollection<CourseRegistration> CourseRegistrations { get; set; }
        public virtual ICollection<UserSurveyResponse> SurveyResponses { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }
        public virtual ICollection<ProgramParticipation> Participations { get; set; }
    }
}
