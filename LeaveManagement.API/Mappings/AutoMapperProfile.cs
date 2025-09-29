using AutoMapper;
using LeaveManagement.API.Models;
using LeaveManagement.API.DTOs;

namespace LeaveManagement.API.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // User mappings
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.Name))
                .ForMember(dest => dest.LeaveBalances, opt => opt.MapFrom(src => src.LeaveBalances))
                .ForMember(dest => dest.ProfilePictureUrl, opt => opt.MapFrom(src => src.ProfilePictureUrl));

            CreateMap<RegisterUserDto, User>()
                .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => 2)) // Employee role
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => BCrypt.Net.BCrypt.HashPassword(src.Password)));

            // Leave mappings
            CreateMap<Leave, LeaveDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.LeaveTypeName, opt => opt.MapFrom(src => src.LeaveType.Type))
                .ForMember(dest => dest.LeaveDays, opt => opt.MapFrom(src => src.LeaveDays));

            CreateMap<ApplyLeaveDto, Leave>()
                .ForMember(dest => dest.DateOfRequest, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Pending"));

            // LeaveBalance mappings
            CreateMap<LeaveBalance, LeaveBalanceDto>()
                .ForMember(dest => dest.LeaveTypeName, opt => opt.MapFrom(src => src.LeaveType.Type));

            CreateMap<LeaveBalanceDto, LeaveBalance>();

            // LeaveType mappings
            CreateMap<LeaveType, LeaveTypeDto>();

            CreateMap<UpdateUserDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.RoleId, opt => opt.Ignore());

            //update profile picture
            CreateMap<User, ProfilePictureDto>()
                .ForMember(dest => dest.ProfilePictureUrl, opt => opt.MapFrom(src => src.ProfilePictureUrl))
                .ForMember(dest => dest.DefaultAvatarUrl, opt => opt.MapFrom(src => src.DefaultAvatarUrl));
        }
    }
}