using AutoMapper;
using IdentityService.Domain.Entities;
using IdentityService.Application.Dtos.Role.Request;
using IdentityService.Application.Dtos.Role.Respone;
namespace IdentityService.Application.Mappers
{
    public class RoleProfile : Profile
    {
        public RoleProfile()
        {
            CreateMap<Role, RoleRep>();
            CreateMap<CreateRoleReq, Role>();
            CreateMap<UpdateRoleReq, Role>();
        }
    }
}
