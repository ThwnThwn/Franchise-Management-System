using FranchiseService.Application.Repositories;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;
using MediatR;

namespace FranchiseService.Application.Features.ShiftAvailabilities.Commands.CreateStaffAvailability;

public class CreateStaffAvailabilityHandler : IRequestHandler<CreateStaffAvailabilityCommand, Guid>
{
    private readonly IGenericRepository<StaffAvailability> _repository;

    public CreateStaffAvailabilityHandler(IGenericRepository<StaffAvailability> repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateStaffAvailabilityCommand request, CancellationToken cancellationToken)
    {
        // BUSINES LOGIC: Past Date Check
        if (request.Request.IsRecurring == false && request.Request.SpecificDate.HasValue)
        {
            if (request.Request.SpecificDate.Value < DateOnly.FromDateTime(DateTime.UtcNow))
            {
                throw new FluentValidation.ValidationException("Khong duoc tao lich trong qua khu!");
            }
        }

        // BUSINES LOGIC: Min Duration Check (at least 2 hours)
        var duration = request.Request.EndTime - request.Request.StartTime;
        if (duration.TotalHours < 2)
        {
             throw new FluentValidation.ValidationException("Thoi gian dang ky toi thieu phai la 2 gio!");
        }

        // BUSINES LOGIC: Overlap Check
        // 1. Get existing availabilities for this user matching the same type (Recurring or Specific)
        IEnumerable<StaffAvailability> existingRecords;
        
        if (request.Request.IsRecurring)
        {
             existingRecords = await _repository.GetAllAsync(predicate: x => 
                x.UserId == request.UserId && 
                x.IsRecurring == true && 
                x.DayOfWeek == (short)request.Request.DayOfWeek!);
        }
        else
        {
             existingRecords = await _repository.GetAllAsync(predicate: x => 
                x.UserId == request.UserId && 
                x.IsRecurring == false && 
                x.SpecificDate == request.Request.SpecificDate!);
        }

        // 2. Check overlap
        foreach (var record in existingRecords)
        {
            if (IsOverlap(request.Request.StartTime, request.Request.EndTime, record.StartTime, record.EndTime))
            {
                throw new FluentValidation.ValidationException("Khung gio nay bi trung voi lich da co!");
            }
        }

        var entity = new StaffAvailability
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            IsAvailable = request.Request.IsAvailable,
            IsRecurring = request.Request.IsRecurring,
            SpecificDate = request.Request.SpecificDate,
            DayOfWeek = request.Request.DayOfWeek.HasValue ? (short)request.Request.DayOfWeek.Value : null,
            StartTime = request.Request.StartTime,
            EndTime = request.Request.EndTime,
            Note = request.Request.Note,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _repository.AddAsync(entity);
        return entity.Id;
    }

    private static bool IsOverlap(TimeOnly s1, TimeOnly e1, TimeOnly s2, TimeOnly e2)
    {
        return s1 < e2 && e1 > s2;
    }
}
