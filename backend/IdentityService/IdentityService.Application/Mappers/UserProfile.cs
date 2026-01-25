using IdentityService.Application.Dtos.User.Request;
using IdentityService.Application.Dtos.User.Respone;
using AutoMapper;
using IdentityService.Domain.Entities;
using IdentityService.Application.Dtos.Role.Respone;
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
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<User, UserRep>()
                .ForMember(dest => dest.Role,
                    opt => opt.MapFrom(src => src.Role));

            CreateMap<Role, RoleUserRep>();
        }
    }
}