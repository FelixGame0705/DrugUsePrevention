using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessObjects
{
    /// <summary>
    /// Tạo, quản lý thông tin khóa học
    /// </summary>
    public class Course
    {
        [Key]
        public int CourseID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string TargetGroup { get; set; }
        public string AgeGroup { get; set; }
        public string ContentURL { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual User Creator { get; set; }
        public virtual ICollection<CourseRegistration> Registrations { get; set; }
    }
}
