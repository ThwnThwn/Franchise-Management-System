using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Commons;
using FranchiseService.Application.ClientInterfaces;
using MediatR;
using FluentValidation;

namespace FranchiseService.Application.Features.ShiftRules.Commands.SaveShiftRule;

public class SaveShiftRuleHandler : IRequestHandler<SaveShiftRuleCommand, Unit>
{
    private readonly IGenericRepository<ShiftRule> _repository;
    private readonly IIdentityServiceClient _identityService;

    public SaveShiftRuleHandler(IGenericRepository<ShiftRule> repository, IIdentityServiceClient identityService)
    {
        _repository = repository;
        _identityService = identityService;
    }

    public async Task<Unit> Handle(SaveShiftRuleCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;
        
        // 1. Authorization Check (Task 3.6)
        var user = await _identityService.GetUserByIdAsync(command.UserId);
        if (user == null)
        {
            throw new UnauthorizedAccessException("User khong ton tai");
        }

        if (req.StoreId.HasValue)
        {
            // Store Rule: Chi Manager cua Store do moi duoc sua (Check HomeStoreId)
            // Hoac Admin (tam thoi chua co Role Admin ro rang, ta bo qua check Admin cho Store Rule neu User ko phai la Manager)
            if (user.HomeStoreId != req.StoreId)
            {
                 throw new UnauthorizedAccessException("Ban khong phai la Quan ly cua cua hang nay");
            }
        }
        else
        {
            // Global Rule: Chi Admin
            // TODO: Can implement check Role Admin chuyen nghiep hon.
            // Hien tai tam chap nhan: Neu user khong thuoc store nao (HomeStoreId == null) thi coi nhu la Admin/Owner level.
            if (user.HomeStoreId != null)
            {
                throw new UnauthorizedAccessException("Chi Admin moi duoc quyen sua Global Rule");
            }
        }

        // 2. Logic Upsert
        var existingRules = await _repository.GetAllAsync(predicate: x => x.StoreId == req.StoreId);
        var existingRule = existingRules.FirstOrDefault();

        if (existingRule != null)
        {
            // Update
            existingRule.AllowShuttle = req.AllowShuttle;
            existingRule.AllowShiftSwap = req.AllowShiftSwap;
            existingRule.MinHoursBetweenShifts = req.MinHoursBetweenShifts;
            existingRule.MaxHoursPerWeek = req.MaxHoursPerWeek;
            existingRule.UpdatedAt = DateTimeOffset.UtcNow;

            await _repository.UpdateAsync(existingRule);
        }
        else
        {
            // Create
            var newRule = new ShiftRule
            {
                Id = Guid.NewGuid(),
                StoreId = req.StoreId,
                AllowShuttle = req.AllowShuttle,
                AllowShiftSwap = req.AllowShiftSwap,
                MinHoursBetweenShifts = req.MinHoursBetweenShifts,
                MaxHoursPerWeek = req.MaxHoursPerWeek,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _repository.AddAsync(newRule);
        }

        return Unit.Value;
    }
}
