using FranchiseService.Grpc;
using Grpc.Net.Client;
using SalesService.Application.Interfaces.Franchise;

namespace SalesService.API.Services.Franchise;

public class FranchiseServiceClient(
     ShiftGrpcService.ShiftGrpcServiceClient shiftClient,
     StoreGrpcService.StoreGrpcServiceClient storeClient) : IFranchiseServiceClient
{

    public async Task<UserShiftInfo?> GetUserCurrentShiftAsync(Guid userId)
    {
        try
        {
            var request = new GetUserCurrentShiftRequest { UserId = userId.ToString() };
            var response = await shiftClient.GetUserCurrentShiftAsync(request);

            if (response == null) return null;

            return new UserShiftInfo
            {
                HasShiftToday = response.HasShiftToday,
                ShiftId = response.ShiftId,
                StoreId = response.StoreId,
                StartTime = response.StartTime,
                EndTime = response.EndTime,
                CanCheckIn = response.CanCheckIn,
                CanCheckOut = response.CanCheckOut,
                CheckInAt = response.CheckInAt,
                CheckOutAt = response.CheckOutAt
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling FranchiseService: {ex.Message}");
            return null;
        }
    }

    public async Task<List<Guid>> GetActiveStaffsAtStoreAsync(Guid storeId)
    {
        try
        {
            var request = new GetActiveStaffsAtStoreRequest { StoreId = storeId.ToString() };
            var response = await shiftClient.GetActiveStaffsAtStoreAsync(request);

            if (response?.Staffs == null) return new List<Guid>();

            return response.Staffs
                .Select(s => Guid.TryParse(s.UserId, out var id) ? id : Guid.Empty)
                .Where(id => id != Guid.Empty)
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling GetActiveStaffsAtStore at FranchiseService: {ex.Message}");
            return new List<Guid>();
        }
    }

    public async Task<StoreDetailInfo?> GetStoreDetailAsync(Guid storeId)
    {
        try
        {
            var request = new GetStoreDetailRequest { Id = storeId.ToString() };
            var response = await storeClient.GetStoreDetailAsync(request);

            if (response == null) return null;

            return new StoreDetailInfo
            {
                Id = Guid.Parse(response.Id),
                Name = response.Name,
                Address = response.Address,
                Latitude = response.Latitude,
                Longitude = response.Longitude,
                IsOpenNow = response.IsOpenNow
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling GetStoreDetail at FranchiseService: {ex.Message}");
            return null;
        }
    }
}
