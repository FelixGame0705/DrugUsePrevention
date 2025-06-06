using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BussinessObjects;
using Microsoft.EntityFrameworkCore;
using Repositories.IRepository;
using Repositories.Paging;
using Services.DTOs;
using Services.IService;

namespace Services.Service
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;

        public CourseService(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public async Task AddCourseAsync(Course course)
        {
            if (course == null)
            {
                throw new ArgumentNullException(nameof(course), "Course cannot be null");
            }
            await _courseRepository.InsertAsync(course);
            await _courseRepository.SaveAsync();
        }

        public Task DeleteCourseAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<BasePaginatedList<Course>> GetAllCoursesAsync(PagingRequest request)
        {
            return await _courseRepository.GetPagging(
                _courseRepository.Entities.Include(c => c.Creator),
                request.index,
                request.pageSize
            );
        }

        public Task<Course> GetCourseByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateCourseAsync(Course course)
        {
            throw new NotImplementedException();
        }
    }
}
