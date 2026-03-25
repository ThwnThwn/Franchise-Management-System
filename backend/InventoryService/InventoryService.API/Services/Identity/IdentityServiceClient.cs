using Grpc.Core;
using Grpc.Net.Client;
using IdentityService.Grpc.Protos.User;
using InventoryService.API.Mapper;
using InventoryService.Application.ClientInterface;
using InventoryService.Application.Constants;
using InventoryService.Application.Dtos.User;
using MediatR;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace InventoryService.API.Services.Identity
{
    public class IdentityServiceClient(UserGrpc.UserGrpcClient client) : IIdentityServiceClient
    {
        public async Task<UserRep> GetManager(GetManagerReq command)
        {
            var request = new GetManagerRequest
            {
                Id = command.StoreId.ToString()
            };
            var response = await client.GetManagerAsync(request);

            if (response == null) 
                throw new RpcException(new Status(StatusCode.NotFound, Message.User.NotFound));
            var entity = response.ToEntity();
            if(entity == null)
                throw new RpcException(new Status(StatusCode.NotFound, Message.User.NotFound));
            return entity;

        }
    }
}
