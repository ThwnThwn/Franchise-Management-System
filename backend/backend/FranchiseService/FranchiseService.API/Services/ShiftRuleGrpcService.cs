using FranchiseService.Grpc;
using FranchiseService.Application.Features.ShiftRules.Commands.SaveShiftRule;
using FranchiseService.Application.Features.ShiftRules.Queries.GetShiftRuleSettings;
using AppDTOs = FranchiseService.Application.Features.ShiftRules.DTOs; // Alias DTOs
using Grpc.Core;
using MediatR;
using System;

namespace FranchiseService.API.Services
{
    public class ShiftRuleGrpcService : FranchiseService.Grpc.ShiftRuleGrpcService.ShiftRuleGrpcServiceBase
    {
        private readonly IMediator _mediator;

        public ShiftRuleGrpcService(IMediator mediator)
        {
            _mediator = mediator;
        }

        // --- GET SETTINGS ---
        public override async Task<ShiftRuleSettingsResponse> GetShiftRuleSettings(GetShiftRuleSettingsRequest request, ServerCallContext context)
        {
            var query = new GetShiftRuleSettingsQuery
            {
                StoreId = string.IsNullOrEmpty(request.StoreId) ? null : Guid.Parse(request.StoreId)
            };

            var result = await _mediator.Send(query);

            return MapToResponse(result);
        }

        // --- SAVE RULE ---
        public override async Task<SaveShiftRuleResponse> SaveShiftRule(SaveShiftRuleRequest request, ServerCallContext context)
        {
            var dto = new AppDTOs.SaveShiftRuleRequest
            {
                StoreId = string.IsNullOrEmpty(request.StoreId) ? null : Guid.Parse(request.StoreId),
                AllowShuttle = request.AllowShuttle,
                AllowShiftSwap = request.AllowShiftSwap,
                MinHoursBetweenShifts = request.MinHoursBetweenShifts,
                MaxHoursPerWeek = request.MaxHoursPerWeek,
                GracePeriodMinutes = request.GracePeriodMinutes,
                DailyOtThresholdHours = request.DailyOtThresholdHours,
                WeeklyOtThresholdHours = request.WeeklyOtThresholdHours
            };

            Guid userId = Guid.Parse(request.UserId);
            var command = new SaveShiftRuleCommand(userId, dto); // Check constructor order if fail

            await _mediator.Send(command);

            return new SaveShiftRuleResponse { Success = true };
        }

        // --- HELPERS ---
        private ShiftRuleSettingsResponse MapToResponse(AppDTOs.ShiftRuleSettingsResponse dto)
        {
            var response = new ShiftRuleSettingsResponse();
            
            if (dto.Global != null)
            {
                response.Global = MapRule(dto.Global);
            }

            if (dto.Stores != null)
            {
                response.Stores.AddRange(dto.Stores.Select(MapRule));
            }

            return response;
        }

        private ShiftRuleResponse MapRule(AppDTOs.ShiftRuleResponse rule)
        {
            return new ShiftRuleResponse
            {
                Id = rule.Id.ToString(),
                StoreId = rule.StoreId?.ToString() ?? "",
                AllowShuttle = rule.AllowShuttle,
                AllowShiftSwap = rule.AllowShiftSwap,
                MinHoursBetweenShifts = rule.MinHoursBetweenShifts,
                MaxHoursPerWeek = rule.MaxHoursPerWeek,
                GracePeriodMinutes = rule.GracePeriodMinutes,
                DailyOtThresholdHours = rule.DailyOtThresholdHours,
                WeeklyOtThresholdHours = rule.WeeklyOtThresholdHours,
                UpdatedAt = rule.UpdatedAt.ToString("O")
            };
        }
    }
}
