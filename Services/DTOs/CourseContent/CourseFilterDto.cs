using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTOs.CourseContent
{
    public class CourseFilterDto
    {
        public string? SearchKeyword { get; set; }
        public string? TargetGroup { get; set; }
        public string? AgeGroup { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsAccept { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
