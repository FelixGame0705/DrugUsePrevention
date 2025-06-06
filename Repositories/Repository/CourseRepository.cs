using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BussinessObjects;
using Repositories.IRepository;

namespace Repositories.Repository
{
    public class CourseRepository : GenericRepository<Course>, ICourseRepository
    {
        public CourseRepository(DrugUsePreventionDBContext context)
            : base(context) { }
    }
}
