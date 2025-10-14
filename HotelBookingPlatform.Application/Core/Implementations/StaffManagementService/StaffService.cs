using AutoMapper;
using HotelBookingPlatform.Application.Core.Abstracts.StaffManagementService;
using HotelBookingPlatform.Domain.Abstracts;
using HotelBookingPlatform.Domain.DTOs.Staff;
using HotelBookingPlatform.Domain.Entities;

namespace HotelBookingPlatform.Application.Core.Implementations.StaffManagementService
{
    public class StaffService : IStaffService
    {
        private readonly IUnitOfWork<Staff> _unitOfWork;
        private readonly IMapper _mapper;

        public StaffService(IUnitOfWork<Staff> unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<StaffResponseDto>> GetAllAsync()
        {
            var staffs = await _unitOfWork.StaffRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<StaffResponseDto>>(staffs);
        }

        public async Task<StaffResponseDto> CreateAsync(StaffCreateRequest request)
        {
            var staff = _mapper.Map<Staff>(request);
            staff.CreatedAtUtc = DateTime.UtcNow;
            staff.UpdatedAtUtc = DateTime.UtcNow;

            await _unitOfWork.StaffRepository.CreateAsync(staff);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<StaffResponseDto>(staff);
        }

        public async Task<StaffResponseDto> UpdateAsync(int id, StaffCreateRequest request)
        {
            var staff = await _unitOfWork.StaffRepository.GetByIdAsync(id);
            _mapper.Map(request, staff);
            staff.UpdatedAtUtc = DateTime.UtcNow;

            await _unitOfWork.StaffRepository.UpdateAsync(id, staff);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<StaffResponseDto>(staff);
        }

        public async Task DeleteAsync(int id)
        {
            await _unitOfWork.StaffRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<StaffResponseDto> UpdateStatusAsync(int id, bool isActive)
        {
            var staff = await _unitOfWork.StaffRepository.GetByIdAsync(id);
            staff.IsActive = isActive;
            staff.UpdatedAtUtc = DateTime.UtcNow;

            await _unitOfWork.StaffRepository.UpdateAsync(id, staff);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<StaffResponseDto>(staff);
        }
    }
}
