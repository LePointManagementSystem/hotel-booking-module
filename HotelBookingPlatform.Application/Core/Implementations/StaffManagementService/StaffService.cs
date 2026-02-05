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
    // Validation minimale RH
    if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
        throw new BadRequestException("FirstName and LastName are required.");

    if (string.IsNullOrWhiteSpace(request.Role))
        throw new BadRequestException("Role is required.");

    var staff = _mapper.Map<Staff>(request);

    staff.CreatedAtUtc = DateTime.UtcNow;
    staff.UpdatedAtUtc = DateTime.UtcNow;
    staff.IsActive = request.IsActive ?? true;

    // ✅ Cas 1: RH-only (pas de login)
    if (string.IsNullOrWhiteSpace(request.Email))
    {
        staff.UserId = null;
        staff.Email = null;

        await _unitOfWork.StaffRepository.CreateAsync(staff);
        await _unitOfWork.SaveChangesAsync();

        var savedNoUser = await _unitOfWork.StaffRepository.GetByIdAsync(staff.StaffId);
        return _mapper.Map<StaffResponseDto>(savedNoUser);
    }

    // ✅ Cas 2: Staff avec login (email fourni)
    var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(request.Email);
    if (user is null)
        throw new NotFoundException($"No user account found with email '{request.Email}'. Create the user first, then create staff.");

    var existingStaff = await _unitOfWork.StaffRepository.GetByUserIdAsync(user.Id);
    if (existingStaff is not null)
        throw new BadRequestException("This user already has a staff profile.");

    staff.UserId = user.Id;
    staff.Email = user.Email;

    await _unitOfWork.StaffRepository.CreateAsync(staff);
    await _unitOfWork.SaveChangesAsync();

    var saved = await _unitOfWork.StaffRepository.GetByIdAsync(staff.StaffId);
    return _mapper.Map<StaffResponseDto>(saved);
}



    public async Task<StaffResponseDto> UpdateAsync(int id, StaffCreateRequest request)
    {
        var staff = await _unitOfWork.StaffRepository.GetByIdAsync(id);

        // map basic fields
        staff.FirstName = request.FirstName;
        staff.LastName  = request.LastName;
        staff.Role      = request.Role;
        staff.PhoneNumber = request.PhoneNumber;
        staff.HotelId   = request.HotelId;

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(request.Email);
            if (user is null)
                throw new NotFoundException($"No user found with email '{request.Email}'.");

            staff.UserId = user.Id;
            staff.Email = user.Email;
        }

        staff.IsActive = request.IsActive ?? staff.IsActive;
        staff.UpdatedAtUtc = DateTime.UtcNow;

        await _unitOfWork.StaffRepository.UpdateAsync(id, staff);
        await _unitOfWork.SaveChangesAsync();

        var saved = await _unitOfWork.StaffRepository.GetByIdAsync(id);
        return _mapper.Map<StaffResponseDto>(saved);
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

        public async Task<StaffResponseDto>AttachUserAsync(int staffId, string userEmail)
        {
            var staff = await _unitOfWork.StaffRepository.GetByIdAsync(staffId);
            if (staff is null)
                throw new KeyNotFoundException($"Staff with ID {staffId} not found.");

            var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(userEmail);
            if (user is null)
                throw new KeyNotFoundException($"User with email {userEmail} not found.");

            staff.UserId = user.Id;
            staff.Email = user.Email;
            staff.UpdatedAtUtc = DateTime.UtcNow;

            await _unitOfWork.StaffRepository.UpdateAsync(staffId, staff);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<StaffResponseDto>(staff);
        }

        public async Task<StaffResponseDto> GetByUserIdAsync(string userId)
        {
            var staff = await _unitOfWork.StaffRepository.GetByUserIdAsync(userId);
            if (staff == null)
                throw new NotFoundException("Staff profile not found for this user.");
            
            if (!staff.IsActive)
                throw new BadRequestException("Your staff service is inactive. Please contact an administrator.");
            
            return _mapper.Map<StaffResponseDto>(staff);
        }

    //     public async Task<IEnumerable<StaffResponseDto>> GetAllAsync()
    // {
    //     var staffs = await _unitOfWork.StaffRepository.GetAllWithHotelAsync();
    //     return _mapper.Map<IEnumerable<StaffResponseDto>>(staffs);
    // }

    }
}
