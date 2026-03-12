using FranchiseService.Grpc;
using Grpc.Net.Client;
using SalesService.Application.Interfaces.Franchise;

namespace SalesService.API.Services.Franchise;

public class FranchiseServiceClient : IFranchiseServiceClient
{
    private readonly ShiftGrpcService.ShiftGrpcServiceClient _shiftClient;
    private readonly StoreGrpcService.StoreGrpcServiceClient _storeClient;
    private readonly string _franchiseServiceUrl;

    public FranchiseServiceClient(IConfiguration configuration)
    {
        var rawUrl = configuration["FRANCHISE_SERVICE_URL"] ?? "http://localhost:5006";

        _franchiseServiceUrl = rawUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                              rawUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            ? rawUrl
            : $"http://{rawUrl}";

        var httpHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        var channel = GrpcChannel.ForAddress(_franchiseServiceUrl, new GrpcChannelOptions
        {
            HttpHandler = httpHandler
        });

        _shiftClient = new ShiftGrpcService.ShiftGrpcServiceClient(channel);
        _storeClient = new StoreGrpcService.StoreGrpcServiceClient(channel);
    }

    public async Task<UserShiftInfo?> GetUserCurrentShiftAsync(Guid userId)
    {
        try
        {
            var request = new GetUserCurrentShiftRequest { UserId = userId.ToString() };
            var response = await _shiftClient.GetUserCurrentShiftAsync(request);

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
            Console.WriteLine($"Error calling FranchiseService ({_franchiseServiceUrl}): {ex.Message}");
            return null;
        }
    }

    public async Task<List<Guid>> GetActiveStaffsAtStoreAsync(Guid storeId)
    {
        try
        {
            var request = new GetActiveStaffsAtStoreRequest { StoreId = storeId.ToString() };
            var response = await _shiftClient.GetActiveStaffsAtStoreAsync(request);

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
            var response = await _storeClient.GetStoreDetailAsync(request);

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
