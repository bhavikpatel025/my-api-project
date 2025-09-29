using AutoMapper;
using Microsoft.EntityFrameworkCore;
using LeaveManagement.API.Data;
using LeaveManagement.API.Models;
using LeaveManagement.API.DTOs;

namespace LeaveManagement.API.Services
{
    public interface IUserService
    {
        Task<LoginResponseDto> LoginAsync(LoginDto loginDto);
        Task<UserDto> RegisterUserAsync(RegisterUserDto registerDto);
        Task<List<UserDto>> GetAllUsersAsync();
        Task<UserDto> GetUserByIdAsync(int id);
        Task<bool> UpdateUserLeaveBalanceAsync(int userId, List<LeaveBalanceDto> balances);
        Task<UserDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto);
        Task<bool> DeleteUserAsync(int id);
        Task<ProfilePictureDto> UploadProfilePictureAsync(int userId, IFormFile file);
        Task<bool> DeleteProfilePictureAsync(int userId);
        Task<ProfilePictureDto> GetProfilePictureAsync(int userId);

    }

    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IJwtService _jwtService;
        private readonly IFileService _fileService;

        public UserService(ApplicationDbContext context, IMapper mapper, IJwtService jwtService, IFileService fileService)
        {
            _context = context;
            _mapper = mapper;
            _jwtService = jwtService;
            _fileService = fileService;

        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.EmailAddress == loginDto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
            {
                return null;
            }

            var token = _jwtService.GenerateToken(user);

            return new LoginResponseDto
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.EmailAddress,
                Role = user.Role.Name,
                Token = token
            };
        }

        public async Task<UserDto> RegisterUserAsync(RegisterUserDto registerDto)
        {
            // Check if user already exists
            if (await _context.Users.AnyAsync(u => u.EmailAddress == registerDto.EmailAddress))
            {
                throw new InvalidOperationException("User with this email already exists");
            }

            // Map DTO → User
            var user = _mapper.Map<User>(registerDto);

            // --- make sure we only have one LeaveBalance per type ---
            if (user.LeaveBalances != null && user.LeaveBalances.Any())
            {
                user.LeaveBalances = user.LeaveBalances
                    .GroupBy(lb => lb.LeaveTypeId)
                    .Select(g =>
                    {
                        var first = g.First();
                        first.User = user; // link back to user so EF tracks it
                        return first;
                    })
                    .ToList();
            }

            // Add the user and their balances in one go
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // ******** DELETE THIS BLOCK (it’s now redundant) ********
            /*
            // Add leave balances manually (this was causing duplicates)
            if (registerDto.LeaveBalances.Any())
            {
                foreach (var balance in registerDto.LeaveBalances)
                {
                    var leaveBalance = new LeaveBalance
                    {
                        UserId = user.Id,
                        LeaveTypeId = balance.LeaveTypeId,
                        Balance = balance.Balance
                    };
                    _context.LeaveBalances.Add(leaveBalance);
                }
                await _context.SaveChangesAsync();
            }
            */

            // Return DTO with balances
            return await GetUserByIdAsync(user.Id);
        }


        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.LeaveBalances)
                    .ThenInclude(lb => lb.LeaveType)
                .Where(u => u.RoleId == 2) // Only employees
                .ToListAsync();

            return _mapper.Map<List<UserDto>>(users);
        }

        public async Task<UserDto> GetUserByIdAsync(int id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.LeaveBalances)
                    .ThenInclude(lb => lb.LeaveType)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return null;

            return _mapper.Map<UserDto>(user);
        }

        public async Task<bool> UpdateUserLeaveBalanceAsync(int userId, List<LeaveBalanceDto> balances)
        {
            var existingBalances = await _context.LeaveBalances
                .Where(lb => lb.UserId == userId)
                .ToListAsync();

            _context.LeaveBalances.RemoveRange(existingBalances);

            foreach (var balance in balances)
            {
                var leaveBalance = new LeaveBalance
                {
                    UserId = userId,
                    LeaveTypeId = balance.LeaveTypeId,
                    Balance = balance.Balance
                };
                _context.LeaveBalances.Add(leaveBalance);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        //update 
        public async Task<UserDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            // Load user without old balances tracked
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null || user.RoleId == 1)
                return null;

            // Check email uniqueness
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.EmailAddress == updateUserDto.EmailAddress && u.Id != id);

            if (existingUser != null)
                throw new InvalidOperationException("Email address is already in use by another user");

            // Update user fields
            _mapper.Map(updateUserDto, user);
            await _context.SaveChangesAsync();

            // Replace leave balances
            var existingBalances = await _context.LeaveBalances
                .Where(lb => lb.UserId == user.Id)
                .ToListAsync();

            _context.LeaveBalances.RemoveRange(existingBalances);

            var newBalances = updateUserDto.LeaveBalances.Select(b => new LeaveBalance
            {
                UserId = user.Id,
                LeaveTypeId = b.LeaveTypeId,
                Balance = b.Balance
            }).ToList();

            await _context.LeaveBalances.AddRangeAsync(newBalances);
            await _context.SaveChangesAsync();

            //  Always reload a fresh User entity with includes so AutoMapper sees the real data:
            var updatedUser = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.LeaveBalances)
                    .ThenInclude(lb => lb.LeaveType)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            return _mapper.Map<UserDto>(updatedUser);
        }


        //delete
        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users
                .Include(u => u.Leaves)
                .Include(u => u.LeaveBalances)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null || user.RoleId == 1) // Don't allow deleting admin user
            {
                return false;
            }

            // Check if user has any approved leaves
            var hasApprovedLeaves = user.Leaves.Any(l => l.Status == "Approved");
            if (hasApprovedLeaves)
            {
                throw new InvalidOperationException("Cannot delete user with approved leave records");
            }

            // Remove related data
            _context.LeaveBalances.RemoveRange(user.LeaveBalances);
            _context.Leaves.RemoveRange(user.Leaves);
            _context.Users.Remove(user);

            await _context.SaveChangesAsync();
            return true;
        }

        //profile picture upload
        public async Task<ProfilePictureDto> UploadProfilePictureAsync(int userId, IFormFile file)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            // Delete old profile picture if exists
            if (!string.IsNullOrEmpty(user.ProfilePicture))
            {
                await _fileService.DeleteProfilePictureAsync(user.ProfilePicture);
            }

            // Upload new profile picture
            var filePath = await _fileService.UploadProfilePictureAsync(file, userId);

            // Update user profile
            user.ProfilePicture = filePath;
            await _context.SaveChangesAsync();

            return _mapper.Map<ProfilePictureDto>(user);
        }

        public async Task<bool> DeleteProfilePictureAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || string.IsNullOrEmpty(user.ProfilePicture))
                return false;

            // Delete physical file
            await _fileService.DeleteProfilePictureAsync(user.ProfilePicture);

            // Update user record
            user.ProfilePicture = null;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<ProfilePictureDto> GetProfilePictureAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return null;

            return _mapper.Map<ProfilePictureDto>(user);
        }
    }
}