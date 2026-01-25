using IdentityService.Application.Dtos.User.Request;
using IdentityService.Application.Dtos.User.Respone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.Abstrations
{
    public interface IUserService
    {
        Task<UserRep> CustomerCreateAsync(CustomerCreateUserReq request);

        Task<UserRep> AdminCreateAsync(CreateUserReq request);

        Task<UserRep?> GetByIdAsync(Guid id);

        Task<IEnumerable<UserRep>> GetAllAsync();

        Task<UserRep?> UpdateAsync(Guid id, UpdateUserReq request);

        Task<bool> DeleteAsync(Guid id);
    }
}
