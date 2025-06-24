using Repositories.IRepository.Consultants;
using Repositories.IRepository.Users;
using Repositories.IRepository;
using Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repositories.IRepository.Programs;
using BussinessObjects;
using Microsoft.EntityFrameworkCore;
using Repositories.Paging;
using Services.DTOs.Program;
using Repositories.Migrations;

namespace Services.Service
{
    public class ProgramService : IProgramService
    {
        private readonly IProgramRepository _programRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        public ProgramService(IProgramRepository programRepository, IUserRepository userRepository, IUnitOfWork unitOfWork, IUserRepository userRepository1)
        {
            _programRepository = programRepository;
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
        }

        public async Task<BasePaginatedList<ProgramResponse>> GetAllProgramsAsync(int pageIndex = 1, int pageSize = 10, string? searchTerm = null, bool? isActive = null)
        {
            var query = _programRepository.Entities
                .Include(p => p.Creator)
                .Include(p => p.Participants)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Title.Contains(searchTerm) ||
                                       p.Description.Contains(searchTerm) ||
                                       p.Location.Contains(searchTerm));
            }

            if (isActive.HasValue)
            {
                query = query.Where(p => p.IsActive == isActive.Value);
            }

            // Order by start date descending
            query = query.OrderByDescending(p => p.StartDate);

            var paginatedResult = await _programRepository.GetPagging(query, pageIndex, pageSize);
            var responses = new List<ProgramResponse>();
            foreach (var item in paginatedResult.Items)
            {
                var response = new ProgramResponse
                {
                    ProgramID = item.ProgramID,
                    Title = item.Title,
                    Description = item.Description,
                    ThumbnailURL = item.ThumbnailURL,
                    StartDate = item.StartDate,
                    EndDate = item.EndDate,
                    Location = item.Location,
                    CreatedBy = item.CreatedBy,
                    IsActive = item.IsActive,
                };
                responses.Add(response);
            }


            return new BasePaginatedList<ProgramResponse>(responses, paginatedResult.TotalItems, pageIndex, pageSize);
        }

        public async Task<ProgramResponse?> GetProgramByIdAsync(int id)
        {
            var program = await _programRepository.Entities
                .Include(p => p.Creator)
                .Include(p => p.Participants)
                .FirstOrDefaultAsync(p => p.ProgramID == id);
            if (program == null)
            {
                return null;
            }
            var response = new ProgramResponse
            {
                ProgramID = program.ProgramID,
                Title = program.Title,
                Description = program.Description,
                ThumbnailURL = program.ThumbnailURL,
                StartDate = program.StartDate,
                EndDate = program.EndDate,
                Location = program.Location,
                CreatedBy = program.CreatedBy,
                IsActive = program.IsActive,
            };
            return response;
        }

        public async Task<ProgramResponse> CreateProgramAsync(ProgramCreateRequest dto)
        {
            if (dto.ThumbnailURL == null)
            {
                dto.ThumbnailURL="N/A";
            }
            var user = await _userRepository.GetByIdAsync(dto.CreatedBy);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            var program = new Program
            {
                Title = dto.Title,
                Description = dto.Description,
                ThumbnailURL = dto.ThumbnailURL,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Location = dto.Location,
                CreatedBy = dto.CreatedBy,
                IsActive = dto.IsActive,
                Creator = user
            };
            await _programRepository.InsertAsync(program);
            await _programRepository.SaveAsync();

            return await GetProgramByIdAsync(program.ProgramID) ?? throw new InvalidOperationException("Failed to retrieve created program");
        }

        public async Task<ProgramResponse?> UpdateProgramAsync(ProgramUpdateRequest dto)
        {
            if (dto.ThumbnailURL == null)
            {
                dto.ThumbnailURL = "N/A";
            }
            var existingProgram = await _programRepository.GetByIdAsync(dto.ProgramID);
            if (existingProgram == null)
                return null;

            existingProgram.Title = dto.Title;
            existingProgram.Description = dto.Description;
            existingProgram.ThumbnailURL = dto.ThumbnailURL;
            existingProgram.StartDate = dto.StartDate;
            existingProgram.EndDate = dto.EndDate;
            existingProgram.Location = dto.Location;
            existingProgram.IsActive = dto.IsActive;

            await _programRepository.UpdateAsync(existingProgram);

            return await GetProgramByIdAsync(dto.ProgramID);
        }

        public async Task<bool> DeleteProgramAsync(int id)
        {
            var program = await _programRepository.GetByIdAsync(id);
            if (program == null)
                return false;
            program.IsActive = false;
            await _programRepository.SaveAsync();
            return true;
        }
    }
}
