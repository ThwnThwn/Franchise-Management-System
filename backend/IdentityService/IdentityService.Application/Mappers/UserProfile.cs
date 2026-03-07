using AutoMapper;
using IdentityService.Application.Dtos.Role.Respone;
using IdentityService.Application.Dtos.User.Request;
using IdentityService.Application.Dtos.User.Respone;
using IdentityService.Application.Features.Users.Commands.AdminCreateUser;
using IdentityService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.Mappers
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<CreateUserReq, User>();

            CreateMap<CustomerCreateUserReq, User>();

            CreateMap<UpdateUserReq, User>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<AdminCreateUserReq, User>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<ChangePasswordReq, User>();

            CreateMap<User, UserRep>()
                //.ForMember(dest => dest.CreatedAt,
                //    opt => opt.MapFrom(src => src.CreatedAt))
                //.ForMember(dest => dest.UpdatedAt,
                //    opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(dest => dest.Role,
                    opt => opt.MapFrom(src => src.Role));

            CreateMap<Role, RoleUserRep>();
        }
    }
}