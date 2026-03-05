using FranchiseService.Grpc;
using FranchiseService.Application.Features.ShiftApplications.Commands.ApplyForShift;
using FranchiseService.Application.Features.ShiftApplications.Commands.ReviewApplication;
using FranchiseService.Application.Features.ShiftApplications.Queries.GetApplicationsByShift;
using Grpc.Core;
using MediatR;

namespace FranchiseService.API.Services;

/// <summary>
/// gRPC Service quan ly don dang ky nhan ca tu Cho Ca (Shift Pool).
/// Chuyen doi giua gRPC request va MediatR command/query.
/// </summary>
public class ShiftApplicationGrpcService
    : Grpc.ShiftApplicationGrpcService.ShiftApplicationGrpcServiceBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ShiftApplicationGrpcService> _logger;

    public ShiftApplicationGrpcService(IMediator mediator, ILogger<ShiftApplicationGrpcService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Nhan vien dang ky nhan ca tu Cho Ca
    /// </summary>
    public override async Task<ApplyForShiftResponse> ApplyForShift(
        ApplyForShiftRequest request, ServerCallContext context)
    {
        try
        {
            var command = new ApplyForShiftCommand
            {
                ShiftId = Guid.Parse(request.ShiftId),
                UserId = Guid.Parse(request.UserId),
                Note = string.IsNullOrEmpty(request.Note) ? null : request.Note
            };

            var applicationId = await _mediator.Send(command);

            return new ApplyForShiftResponse
            {
                Success = true,
                ApplicationId = applicationId.ToString(),
                Message = "Dang ky thanh cong. Don dang cho Manager duyet."
            };
        }
        catch (KeyNotFoundException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
        catch (FormatException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument,
                $"Dinh dang du lieu khong hop le: {ex.Message}"));
        }
    }

    /// <summary>
    /// Manager xem danh sach ung vien kem Smart Score
    /// </summary>
    public override async Task<GetApplicationsByShiftResponse> GetApplicationsByShift(
        GetApplicationsByShiftRequest request, ServerCallContext context)
    {
        try
        {
            var query = new GetApplicationsByShiftQuery
            {
                ShiftId = Guid.Parse(request.ShiftId)
            };

            var applications = await _mediator.Send(query);

            var response = new GetApplicationsByShiftResponse();
            foreach (var app in applications)
            {
                response.Applications.Add(new ShiftApplicationItem
                {
                    Id = app.Id.ToString(),
                    ShiftId = app.ShiftId.ToString(),
                    UserId = app.UserId.ToString(),
                    Status = app.Status,
                    Note = app.Note ?? "",
                    AppliedAt = app.AppliedAt.ToString("o"),
                    UserName = app.UserName ?? "",
                    UserEmail = app.UserEmail ?? "",
                    AvatarUrl = app.AvatarUrl ?? "",
                    HourlyWage = (double)(app.HourlyWage ?? 0),
                    SmartScore = app.SmartScore,
                    WeeklyHoursWorked = app.WeeklyHoursWorked
                });
            }

            return response;
        }
        catch (FormatException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument,
                $"Dinh dang du lieu khong hop le: {ex.Message}"));
        }
    }

    /// <summary>
    /// Manager duyet hoac tu choi don dang ky
    /// </summary>
    public override async Task<ReviewApplicationResponse> ReviewApplication(
        ReviewApplicationRequest request, ServerCallContext context)
    {
        try
        {
            var command = new ReviewApplicationCommand
            {
                ApplicationId = Guid.Parse(request.ApplicationId),
                IsApproved = request.IsApproved,
                ReviewedBy = Guid.Parse(request.ReviewedBy),
                ReviewNote = string.IsNullOrEmpty(request.ReviewNote) ? null : request.ReviewNote
            };

            await _mediator.Send(command);

            string message = request.IsApproved
                ? "Da duyet don. Nhan vien da duoc tu dong phan cong vao ca."
                : "Da tu choi don dang ky.";

            return new ReviewApplicationResponse
            {
                Success = true,
                Message = message
            };
        }
        catch (KeyNotFoundException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
        catch (ArgumentException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (FormatException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument,
                $"Dinh dang du lieu khong hop le: {ex.Message}"));
        }
    }
}
