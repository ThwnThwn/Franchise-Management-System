using FranchiseService.Application.Repositories;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;
using MediatR;

namespace FranchiseService.Application.Features.ShiftAvailabilities.Commands.DeleteStaffAvailability;

public class DeleteStaffAvailabilityHandler : IRequestHandler<DeleteStaffAvailabilityCommand, Unit>
{
    private readonly IGenericRepository<StaffAvailability> _repository;

    public DeleteStaffAvailabilityHandler(IGenericRepository<StaffAvailability> repository)
    {
        _repository = repository;
    }

    public async Task<Unit> Handle(DeleteStaffAvailabilityCommand command, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(command.Id)
            ?? throw new KeyNotFoundException($"Khong tim thay lich ranh/ban {command.Id}");

        // Check ownership
        if (entity.UserId != command.UserId)
        {
            throw new UnauthorizedAccessException("Ban khong co quyen xoa lich cua nguoi khac");
        }

        // Soft delete
        await _repository.DeleteAsync(entity.Id); 

        return Unit.Value;
    }
}
