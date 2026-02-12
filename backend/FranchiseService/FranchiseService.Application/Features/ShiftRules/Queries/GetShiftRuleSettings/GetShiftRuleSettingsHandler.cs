using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Features.ShiftRules.DTOs;
using FranchiseService.Domain.Commons;
using FranchiseService.Domain.Entities;
using MediatR;

namespace FranchiseService.Application.Features.ShiftRules.Queries.GetShiftRuleSettings;

public class GetShiftRuleSettingsHandler : IRequestHandler<GetShiftRuleSettingsQuery, ShiftRuleSettingsResponse>
{
    private readonly IGenericRepository<ShiftRule> _repository;

    public GetShiftRuleSettingsHandler(IGenericRepository<ShiftRule> repository)
    {
        _repository = repository;
    }

    public async Task<ShiftRuleSettingsResponse> Handle(GetShiftRuleSettingsQuery request, CancellationToken cancellationToken)
    {
        // 1. Get Global Rule
        var globalRules = await _repository.GetAllAsync(predicate: x => x.StoreId == null);
        var globalRule = globalRules.FirstOrDefault();

        // 2. Get Store Rules
        IEnumerable<ShiftRule> storeRules;
        if (request.StoreId.HasValue)
        {
            storeRules = await _repository.GetAllAsync(predicate: x => x.StoreId == request.StoreId);
        }
        else
        {
            // If StoreId is null, return all store rules (Admin view)
            storeRules = await _repository.GetAllAsync(predicate: x => x.StoreId != null);
        }

        // 3. Map to Response
        var response = new ShiftRuleSettingsResponse
        {
            Global = globalRule != null ? MapToDto(globalRule) : null,
            Stores = storeRules.Select(MapToDto).ToList()
        };

        return response;
    }

    private static ShiftRuleResponse MapToDto(ShiftRule entity)
    {
        return new ShiftRuleResponse
        {
            Id = entity.Id,
            StoreId = entity.StoreId,
            AllowShuttle = entity.AllowShuttle,
            AllowShiftSwap = entity.AllowShiftSwap,
            MinHoursBetweenShifts = entity.MinHoursBetweenShifts,
            MaxHoursPerWeek = entity.MaxHoursPerWeek,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
