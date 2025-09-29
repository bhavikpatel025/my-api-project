using AutoMapper;
using Microsoft.EntityFrameworkCore;
using LeaveManagement.API.Data;
using LeaveManagement.API.Models;
using LeaveManagement.API.DTOs;

namespace LeaveManagement.API.Services
{
    public interface ILeaveService
    {
        Task<LeaveDto> ApplyLeaveAsync(int userId, ApplyLeaveDto applyLeaveDto);
        Task<List<LeaveDto>> GetUserLeavesAsync(int userId);
        Task<List<LeaveDto>> GetAllLeavesAsync(int? userId = null);
        Task<bool> CancelLeaveAsync(int userId, int leaveId);
        Task<bool> UpdateLeaveStatusAsync(UpdateLeaveStatusDto updateLeaveStatusDto);
        Task<List<LeaveTypeDto>> GetLeaveTypesAsync();
    }

    public class LeaveService : ILeaveService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public LeaveService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<LeaveDto> ApplyLeaveAsync(int userId, ApplyLeaveDto applyLeaveDto)
        {
            // Validate dates
            if (applyLeaveDto.StartDate < DateTime.Today)
            {
                throw new InvalidOperationException("Start date cannot be in the past");
            }

            if (applyLeaveDto.EndDate < applyLeaveDto.StartDate)
            {
                throw new InvalidOperationException("End date cannot be before start date");
            }

            // Check leave balance
            var leaveBalance = await _context.LeaveBalances
                .FirstOrDefaultAsync(lb => lb.UserId == userId && lb.LeaveTypeId == applyLeaveDto.LeaveTypeId);

            if (leaveBalance == null)
            {
                throw new InvalidOperationException("Leave balance not found for this leave type");
            }

            var leaveDays = (applyLeaveDto.EndDate - applyLeaveDto.StartDate).Days + 1;
            if (leaveBalance.Balance < leaveDays)
            {
                throw new InvalidOperationException($"Insufficient leave balance. Available: {leaveBalance.Balance}, Requested: {leaveDays}");
            }

            var leave = _mapper.Map<Leave>(applyLeaveDto);
            leave.UserId = userId;

            _context.Leaves.Add(leave);
            await _context.SaveChangesAsync();

            // Load related data for response
            await _context.Entry(leave)
                .Reference(l => l.User)
                .LoadAsync();
            await _context.Entry(leave)
                .Reference(l => l.LeaveType)
                .LoadAsync();

            return _mapper.Map<LeaveDto>(leave);
        }

        public async Task<List<LeaveDto>> GetUserLeavesAsync(int userId)
        {
            var leaves = await _context.Leaves
                .Include(l => l.User)
                .Include(l => l.LeaveType)
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.DateOfRequest)
                .ToListAsync();

            return _mapper.Map<List<LeaveDto>>(leaves);
        }

        public async Task<List<LeaveDto>> GetAllLeavesAsync(int? userId = null)
        {
            var query = _context.Leaves
                .Include(l => l.User)
                .Include(l => l.LeaveType)
                .AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(l => l.UserId == userId.Value);
            }

            var leaves = await query
                .OrderByDescending(l => l.DateOfRequest)
                .ToListAsync();

            return _mapper.Map<List<LeaveDto>>(leaves);
        }

        public async Task<bool> CancelLeaveAsync(int userId, int leaveId)
        {
            var leave = await _context.Leaves
                .FirstOrDefaultAsync(l => l.Id == leaveId && l.UserId == userId);

            if (leave == null)
                return false;

            if (leave.Status != "Pending")
            {
                throw new InvalidOperationException("Only pending leaves can be cancelled");
            }

            // Check if cancellation is within 3 days
            var daysDifference = (leave.StartDate - DateTime.Today).Days;
            if (daysDifference < 3)
            {
                throw new InvalidOperationException("Leave cannot be cancelled within 3 days of start date");
            }

            leave.Status = "Cancelled";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateLeaveStatusAsync(UpdateLeaveStatusDto updateLeaveStatusDto)
        {
            var leave = await _context.Leaves
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.Id == updateLeaveStatusDto.LeaveId);

            if (leave == null)
                return false;

            if (leave.Status != "Pending")
            {
                throw new InvalidOperationException("Only pending leaves can be approved/rejected");
            }

            leave.Status = updateLeaveStatusDto.Status;

            // If approved, deduct from leave balance
            if (updateLeaveStatusDto.Status == "Approved")
            {
                var leaveBalance = await _context.LeaveBalances
                    .FirstOrDefaultAsync(lb => lb.UserId == leave.UserId && lb.LeaveTypeId == leave.LeaveTypeId);

                if (leaveBalance != null)
                {
                    var leaveDays = (leave.EndDate - leave.StartDate).Days + 1;
                    leaveBalance.Balance -= leaveDays;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<LeaveTypeDto>> GetLeaveTypesAsync()
        {
            var leaveTypes = await _context.LeaveTypes.ToListAsync();
            return _mapper.Map<List<LeaveTypeDto>>(leaveTypes);
        }
    }
}