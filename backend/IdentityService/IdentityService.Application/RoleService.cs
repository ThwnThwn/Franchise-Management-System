using IdentityService.Application.Abstrations;
using IdentityService.Application.Dtos.Role.Request;
using IdentityService.Application.Dtos.Role.Respone;
using IdentityService.Application.Persistences.Repositories;
using AutoMapper;
using IdentityService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application
{
    public class RoleService : IRoleService
    {
        private IRoleRepository _roleRepository;
        private readonly IMapper _mapper;
        public RoleService(IRoleRepository roleRepository, IMapper mapper)
        {
            _roleRepository = roleRepository;
            _mapper = mapper;
        }

        // CREATE
        public async Task<RoleRep> CreateAsync(CreateRoleReq request)
        {
            var existingRole = await _roleRepository.GetByNameAsync(request.Name);
            if (existingRole != null)
                throw new Exception($"Role '{request.Name}' already exists");

            var role = _mapper.Map<Role>(request);

            role.CreatedAt = DateTimeOffset.UtcNow;
            role.UpdatedAt = DateTimeOffset.UtcNow;

            await _roleRepository.AddAsync(role);

            return _mapper.Map<RoleRep>(role);
        }
        // UPDATE
        public async Task<RoleRep?> UpdateAsync(Guid id, UpdateRoleReq request)
        {
            var existingRole = await _roleRepository.GetByNameAsync(request.Name);
            if (existingRole != null)
                throw new Exception($"Role '{request.Name}' already exists");

            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
                return null;

            _mapper.Map(request, role);
            role.UpdatedAt = DateTimeOffset.UtcNow;

            await _roleRepository.UpdateAsync(role);

            return _mapper.Map<RoleRep>(role);
        }

        // DELETE
        public async Task<bool> DeleteAsync(Guid id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
                return false;

            await _roleRepository.DeleteAsync(id);
            return true;
        }

        // GET ALL
        public async Task<IReadOnlyList<RoleRep>> GetAllAsync()
        {
            var roles = await _roleRepository.GetAllAsync();

            return _mapper.Map<IReadOnlyList<RoleRep>>(roles);
        }

        // GET BY ID
        public async Task<RoleRep?> GetByIdAsync(Guid id)
        {
            var role = await _roleRepository.GetByIdAsync(id);

            return role == null ? null : _mapper.Map<RoleRep>(role);
        }

        
    }
}
