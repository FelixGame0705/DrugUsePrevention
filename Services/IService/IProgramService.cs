using Repositories.Paging;
using Services.DTOs.Program;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IService
{
    public interface IProgramService
    {
        Task<BasePaginatedList<ProgramResponse>> GetAllProgramsAsync(int pageIndex = 1, int pageSize = 10, string? searchTerm = null, bool? isActive = null);
        Task<ProgramResponse?> GetProgramByIdAsync(int id);
        Task<ProgramResponse> CreateProgramAsync(ProgramCreateRequest dto);
        Task<ProgramResponse?> UpdateProgramAsync(ProgramUpdateRequest dto);
        Task<bool> DeleteProgramAsync(int id);
    }
}
