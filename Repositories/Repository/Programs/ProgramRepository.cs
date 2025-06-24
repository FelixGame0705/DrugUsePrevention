using BussinessObjects;
using Repositories.IRepository.Consultants;
using Repositories.IRepository.Programs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Repository.Programs
{
    public class ProgramRepository : GenericRepository<Program>, IProgramRepository
    {
        public ProgramRepository(DrugUsePreventionDBContext context)
            : base(context) { }
    }
}
