using Grpc.Core;
using Grpc.Net.Client;
using IdentityService.Grpc.Protos.User;
using FranchiseService.Application.ClientInterfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace FranchiseService.Infrastructure.Clients.Identity
{
    // gRPC Client giao tiep voi IdentityService
    // Dung de validate user va lay thong tin nhan vien (username, email)
    public class IdentityServiceClient(UserGrpc.UserGrpcClient client, IHttpContextAccessor httpContextAccessor) : IIdentityServiceClient
    {

        // Lay thong tin user theo ID tu IdentityService
        // Tra ve null neu khong tim thay (thay vi throw)
        public async Task<UserInfo?> GetUserByIdAsync(Guid userId)
        {
            try
            {
                var request = new GetUserByIdRequest { Id = userId.ToString() };
                var response = await client.GetByIdAsync(request);

                if (response == null) return null;

                return MapToUserInfo(response);
            }
            catch (RpcException ex) when (
                ex.StatusCode == StatusCode.NotFound ||
                ex.StatusCode == StatusCode.Internal && ex.Status.Detail.Contains("NotFound"))
            {
                // IdentityService co the tra NotFound truc tiep
                // hoac boc trong Internal -> can bat ca 2 truong hop
                return null;
            }
        }

        // Lay danh sach tat ca user (co the filter theo role)
        // Dung cho GetAvailableStaffs de lay danh sach nhan vien cua store
        // IdentityService yeu cau x-user-id trong gRPC metadata de xac thuc
        public async Task<List<UserInfo>> GetAllUsersAsync(string? roleSlug = null)
        {
            try
            {
                var request = new GetAllUserWithFilterRequest
                {
                    RoleSlug = roleSlug ?? "",
                    Page = 1,
                    PageSize = 1000 // Lay het, FE se phan trang o tang ung dung
                };

                // Tao gRPC metadata headers
                // IdentityService.GetAll BAT BUOC phai co x-user-id
                var metadata = CreateAuthMetadata();

                var response = await client.GetAllAsync(request, metadata);

                if (response == null || response.Users == null)
                    return new List<UserInfo>();

                return response.Users.Select(MapToUserInfo).ToList();
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
            {
                return new List<UserInfo>();
            }
        }

        // Lay thong tin cua nhieu user cung luc thong qua 1 request gRPC
        public async Task<List<UserInfo>> GetUsersByIdsAsync(IEnumerable<Guid> userIds)
        {
            var idsList = userIds.ToList();
            if (idsList.Count == 0) return new List<UserInfo>();

            try
            {
                var request = new GetUserByIdsRequest();
                request.Ids.AddRange(idsList.Select(id => id.ToString()));

                var response = await client.GetByIdsAsync(request);

                if (response == null || response.Users == null)
                    return new List<UserInfo>();

                return response.Users.Select(MapToUserInfo).ToList();
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
            {
                return new List<UserInfo>();
            }
        }

        public async Task UpdateUserAsync(UserInfo user, CancellationToken ct)
        {
            var request = new UpdateUserRequest
            {
                Id = user.Id.ToString(),
                Username = user.Username,
                Email = user.Email,
                Phone = user.Phone ?? "",
                FirstName = user.FirstName,
                LastName = user.LastName ?? "",
                CurrentHourlyWage = user.CurrentHourlyWage.HasValue ? (double)user.CurrentHourlyWage.Value : 0,
                HomeStoreId = user.HomeStoreId.HasValue ? user.HomeStoreId.Value.ToString() : "",
                RoleSlug = user.RoleSlug ?? ""
            };
            await _client.UpdateAsync(
                request,
                cancellationToken: ct
            );
        }

        // PRIVATE METHODS

        // Tao gRPC Metadata chua x-user-id tu HTTP request hien tai
        // IdentityService dung header nay de xac thuc va filter data
        private Metadata CreateAuthMetadata()
        {
            var metadata = new Metadata();

            // Lay x-user-id tu HTTP request hien tai (forwarded tu API Gateway / FE)
            var httpContext = httpContextAccessor.HttpContext;
            string? userId = null;

            if (httpContext != null)
            {
                // Thu lay tu header x-user-id (truyen tu API Gateway hoac Postman)
                userId = httpContext.Request.Headers["x-user-id"].FirstOrDefault();

                // Neu khong co header, thu lay tu claim (JWT auth)
                if (string.IsNullOrEmpty(userId))
                {
                    userId = httpContext.User?.FindFirst("sub")?.Value
                          ?? httpContext.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                }
            }

            // Fallback: dung 1 service account ID de goi internal
            if (string.IsNullOrEmpty(userId))
            {
                userId = "00000000-0000-0000-0000-000000000000";
            }

            metadata.Add("x-user-id", userId);
            return metadata;
        }

        // Map tu gRPC UserResponse sang UserInfo DTO
        // Dung chung cho ca GetById va GetAll
        private static UserInfo MapToUserInfo(UserResponse response)
        {
            return new UserInfo
            {
                Id = Guid.Parse(response.Id),
                Username = response.Username,
                Email = response.Email,
                Phone = response.Phone,
                FirstName = response.FirstName,
                LastName = response.LastName,
                AvatarUrl = response.AvatarUrl,
                CurrentHourlyWage = response.HasCurrentHourlyWage ? (decimal)response.CurrentHourlyWage : null,
                HomeStoreId = !string.IsNullOrEmpty(response.HomeStoreId) ? Guid.Parse(response.HomeStoreId) : null,
                RoleSlug = response.Role?.Slug
            };
        }
    }
}