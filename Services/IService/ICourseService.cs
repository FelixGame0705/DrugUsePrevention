using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BussinessObjects;
using Repositories.Paging;
using Services.DTOs;

namespace Services.IService
{
    public interface ICourseService
    {
        // Define methods for course-related operations
        Task<BasePaginatedList<Course>> GetAllCoursesAsync(PagingRequest request);
        Task<Course> GetCourseByIdAsync(int id);
        Task AddCourseAsync(Course course);
        Task UpdateCourseAsync(Course course);
        Task DeleteCourseAsync(int id);
    }
}
