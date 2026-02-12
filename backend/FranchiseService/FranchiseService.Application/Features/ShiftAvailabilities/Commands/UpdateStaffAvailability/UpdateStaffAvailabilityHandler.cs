using FranchiseService.Application.Repositories;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;
using MediatR;

namespace FranchiseService.Application.Features.ShiftAvailabilities.Commands.UpdateStaffAvailability;

public class UpdateStaffAvailabilityHandler : IRequestHandler<UpdateStaffAvailabilityCommand, Unit>
{
    private readonly IGenericRepository<StaffAvailability> _repository;

    public UpdateStaffAvailabilityHandler(IGenericRepository<StaffAvailability> repository)
    {
        _repository = repository;
    }

    public async Task<Unit> Handle(UpdateStaffAvailabilityCommand command, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(command.Id)
            ?? throw new KeyNotFoundException($"Khong tim thay lich ranh/ban {command.Id}");

        // Check ownership
        if (entity.UserId != command.UserId)
        {
            throw new UnauthorizedAccessException("Ban khong co quyen sua lich cua nguoi khac");
        }

        var req = command.Request;

        // Partial update
        if (req.IsAvailable.HasValue) entity.IsAvailable = req.IsAvailable.Value;
        if (req.StartTime.HasValue) entity.StartTime = req.StartTime.Value;
        if (req.EndTime.HasValue) entity.EndTime = req.EndTime.Value;
        if (req.Note != null) entity.Note = req.Note; 

        // Handle Reccuring logic check
        // Neu user muon thay doi kieu lich (Recurring -> Specific hoặc Specific -> Recurring)
        if (req.IsRecurring.HasValue)
        {
            entity.IsRecurring = req.IsRecurring.Value;

            if (entity.IsRecurring.Value)
            {
                // Switch to Recurring
                // Validator da dam bao req.DayOfWeek != null
                if (req.DayOfWeek.HasValue) entity.DayOfWeek = (short)req.DayOfWeek.Value;
                
                // Clear SpecificDate
                entity.SpecificDate = null;
            }
            else
            {
                // Switch to SpecificDate
                // Validator da dam bao req.SpecificDate != null
                if (req.SpecificDate.HasValue) entity.SpecificDate = req.SpecificDate.Value;
                
                // Clear DayOfWeek
                entity.DayOfWeek = null;
            }
        }
        else 
        {
            // Neu khong doi kieu lich, nhung doi ngay/thu thi cap nhat
            if (entity.IsRecurring == true && req.DayOfWeek.HasValue)
                entity.DayOfWeek = (short)req.DayOfWeek.Value;
            
            if (entity.IsRecurring == false && req.SpecificDate.HasValue)
                entity.SpecificDate = req.SpecificDate.Value;
        }

        // BUSINES LOGIC: Validation (Past Date & Min Duration)
        // 1. Past Date Check (Prevent modifying past records or moving to past)
        if (entity.IsRecurring == false && entity.SpecificDate < DateOnly.FromDateTime(DateTime.UtcNow))
        {
             throw new FluentValidation.ValidationException("Khong duoc cap nhat lich trong qua khu!");
        }

        // 2. Min Duration Check
        var duration = entity.EndTime - entity.StartTime;
        if (duration.TotalHours < 2)
        {
             throw new FluentValidation.ValidationException("Thoi gian dang ky toi thieu phai la 2 gio!");
        }

        // Check overlap 
        IEnumerable<StaffAvailability> existingRecords;
        
        if (entity.IsRecurring == true)
        {
             existingRecords = await _repository.GetAllAsync(predicate: x => 
                x.Id != entity.Id &&
                x.UserId == entity.UserId && 
                x.IsRecurring == true && 
                x.DayOfWeek == entity.DayOfWeek);
        }
        else
        {
             existingRecords = await _repository.GetAllAsync(predicate: x => 
                x.Id != entity.Id &&
                x.UserId == entity.UserId && 
                x.IsRecurring == false && 
                x.SpecificDate == entity.SpecificDate);
        }

        foreach (var record in existingRecords)
        {
            if (IsOverlap(entity.StartTime, entity.EndTime, record.StartTime, record.EndTime))
            {
                throw new FluentValidation.ValidationException("Khung gio nay bi trung voi lich da co!");
            }
        }

        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await _repository.UpdateAsync(entity);

        return Unit.Value;
    }

    private static bool IsOverlap(TimeOnly s1, TimeOnly e1, TimeOnly s2, TimeOnly e2)
    {
        return s1 < e2 && e1 > s2;
    }
}
