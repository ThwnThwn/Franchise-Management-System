using IdentityService.Application.Dtos.Role.Request;
using IdentityService.Application.Dtos.Role.Respone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.Abstrations
{
    public interface IRoleService
    {
        Task<RoleRep> CreateAsync(CreateRoleReq request);

        Task<RoleRep?> GetByIdAsync(Guid id);

        Task<IReadOnlyList<RoleRep>> GetAllAsync();

        Task<RoleRep?> UpdateAsync(Guid id, UpdateRoleReq request);

        Task<bool> DeleteAsync(Guid id);
    }
}
