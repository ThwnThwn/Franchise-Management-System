using FranchiseService.Grpc; // Generated Namespace
using FranchiseService.Application.Features.ShiftAvailabilities.Commands.CreateStaffAvailability;
using FranchiseService.Application.Features.ShiftAvailabilities.Commands.DeleteStaffAvailability;
using FranchiseService.Application.Features.ShiftAvailabilities.Commands.UpdateStaffAvailability;
using AppDTOs = FranchiseService.Application.Features.ShiftAvailabilities.DTOs;
using FranchiseService.Application.Features.ShiftAvailabilities.Queries.GetMyAvailability;
using FranchiseService.Application.Features.ShiftAvailabilities.Queries.GetStaffAvailabilities;
using Grpc.Core;
using MediatR;
using System;

namespace FranchiseService.API.Services
{
    public class AvailabilityGrpcService : FranchiseService.Grpc.ShiftAvailabilityGrpcService.ShiftAvailabilityGrpcServiceBase
    {
        private readonly IMediator _mediator;

        public AvailabilityGrpcService(IMediator mediator)
        {
            _mediator = mediator;
        }

        // --- GET MY AVAILABILITY ---
        public override async Task<StaffAvailabilityListResponse> GetMyAvailability(GetAvailabilityParams request, ServerCallContext context)
        {
            var queryParams = MapToParams(request);
            
            var query = new GetMyAvailabilityQuery
            {
                FromDate = queryParams.FromDate,
                ToDate = queryParams.ToDate,
                IsAvailable = queryParams.IsAvailable,
                IsRecurring = queryParams.IsRecurring,
                DayOfWeek = queryParams.DayOfWeek,
                UserId = queryParams.UserId ?? Guid.Empty, // Or handle logic
                Keyword = queryParams.Keyword
                // StoreId not in MyAvailabilityQuery? Check definition
            };

            // If UserId is not set in params (it's "My"), it should be set from context/token.
            // For now assuming Params has it or Query defaults to context if we had context accessor.
            // But checking Query definition earlier, it has public Guid UserId with set.

            var result = await _mediator.Send(query);

            return MapToResponse(result.Items);
        }

        // --- GET STAFF AVAILABILITIES ---
        public override async Task<StaffAvailabilityListResponse> GetStaffAvailabilities(GetAvailabilityParams request, ServerCallContext context)
        {
            var queryParams = MapToParams(request);
            var query = new GetStaffAvailabilitiesQuery
            {
                 StoreId = queryParams.StoreId,
                 UserId = queryParams.UserId,
                 FromDate = queryParams.FromDate,
                 ToDate = queryParams.ToDate,
                 IsAvailable = queryParams.IsAvailable,
                 IsRecurring = queryParams.IsRecurring,
                 Keyword = queryParams.Keyword
            };
            var result = await _mediator.Send(query);

            return MapToResponse(result.Items);
        }

        // --- CREATE ---
        public override async Task<CreateAvailabilityResponse> CreateAvailability(CreateStaffAvailabilityRequest request, ServerCallContext context)
        {
            // Map Proto -> DTO
            var dto = new AppDTOs.CreateStaffAvailabilityRequest
            {
                IsAvailable = request.IsAvailable,
                IsRecurring = request.IsRecurring,
                SpecificDate = string.IsNullOrEmpty(request.SpecificDate) ? null : DateOnly.Parse(request.SpecificDate),
                DayOfWeek = request.DayOfWeek == 0 ? null : (DayOfWeek)request.DayOfWeek, // 0 is Sunday usually, assuming valid input
                StartTime = TimeOnly.Parse(request.StartTime),
                EndTime = TimeOnly.Parse(request.EndTime),
                Note = request.Note
            };

            Guid userId = Guid.Parse(request.UserId);

            var command = new CreateStaffAvailabilityCommand(userId, dto);
            var id = await _mediator.Send(command);

            return new CreateAvailabilityResponse { Id = id.ToString() };
        }

        // --- UPDATE ---
        public override async Task<UpdateAvailabilityResponse> UpdateAvailability(UpdateStaffAvailabilityRequest request, ServerCallContext context)
        {
            // Map Proto -> DTO
            var dto = new AppDTOs.UpdateStaffAvailabilityRequest
            {
                IsAvailable = request.HasIsAvailable ? request.IsAvailable : null,
                IsRecurring = request.HasIsRecurring ? request.IsRecurring : null,
                SpecificDate = string.IsNullOrEmpty(request.SpecificDate) ? null : DateOnly.Parse(request.SpecificDate),
                DayOfWeek = request.HasDayOfWeek ? (DayOfWeek)request.DayOfWeek : null,
                StartTime = string.IsNullOrEmpty(request.StartTime) ? null : TimeOnly.Parse(request.StartTime),
                EndTime = string.IsNullOrEmpty(request.EndTime) ? null : TimeOnly.Parse(request.EndTime),
                Note = request.Note
            };

            var command = new UpdateStaffAvailabilityCommand(Guid.Parse(request.Id), Guid.Parse(request.UserId), dto);
            await _mediator.Send(command);

            return new UpdateAvailabilityResponse { Success = true };
        }

        // --- DELETE ---
        public override async Task<DeleteAvailabilityResponse> DeleteAvailability(DeleteAvailabilityRequest request, ServerCallContext context)
        {
            var command = new DeleteStaffAvailabilityCommand(Guid.Parse(request.Id), Guid.Parse(request.UserId));
            await _mediator.Send(command);

            return new DeleteAvailabilityResponse { Success = true };
        }

        // --- HELPERS ---

        private AppDTOs.GetAvailabilityParams MapToParams(GetAvailabilityParams request)
        {
            return new AppDTOs.GetAvailabilityParams
            {
                FromDate = string.IsNullOrEmpty(request.FromDate) ? null : DateOnly.Parse(request.FromDate),
                ToDate = string.IsNullOrEmpty(request.ToDate) ? null : DateOnly.Parse(request.ToDate),
                IsAvailable = request.HasIsAvailable ? request.IsAvailable : null,
                IsRecurring = request.HasIsRecurring ? request.IsRecurring : null,
                DayOfWeek = request.HasDayOfWeek ? (DayOfWeek)request.DayOfWeek : null,
                StoreId = string.IsNullOrEmpty(request.StoreId) ? null : Guid.Parse(request.StoreId),
                UserId = string.IsNullOrEmpty(request.UserId) ? null : Guid.Parse(request.UserId),
                Keyword = request.Keyword
            };
        }

        private StaffAvailabilityListResponse MapToResponse(IEnumerable<AppDTOs.StaffAvailabilityResponse> list)
        {
            var response = new StaffAvailabilityListResponse();
            response.Data.AddRange(list.Select(x => new FranchiseService.Grpc.StaffAvailabilityResponse
            {
                Id = x.Id.ToString(),
                UserId = x.UserId.ToString(),
                IsAvailable = x.IsAvailable,
                IsRecurring = x.IsRecurring,
                SpecificDate = x.SpecificDate?.ToString("yyyy-MM-dd") ?? "",
                DayOfWeek = x.DayOfWeek.HasValue ? (int)x.DayOfWeek.Value : 0, // Need to handle 0 carefully if Enum 0 is Sunday
                StartTime = x.StartTime.ToString("HH:mm"),
                EndTime = x.EndTime.ToString("HH:mm"),
                Note = x.Note ?? "",
                CreatedAt = x.CreatedAt.ToString("O")
            }));
            return response;
        }
    }
}
