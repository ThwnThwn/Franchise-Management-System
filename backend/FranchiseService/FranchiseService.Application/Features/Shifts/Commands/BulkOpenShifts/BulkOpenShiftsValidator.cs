using FluentValidation;

namespace FranchiseService.Application.Features.Shifts.Commands.BulkOpenShifts;

/// <summary>
/// Kiem tra du lieu dau vao truoc khi mo ca ra Cho.
/// </summary>
public class BulkOpenShiftsValidator : AbstractValidator<BulkOpenShiftsCommand>
{
    public BulkOpenShiftsValidator()
    {
        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("Ma cua hang khong duoc de trong.");

        RuleFor(x => x.ShiftIds)
            .NotEmpty().WithMessage("Danh sach ca khong duoc de trong.")
            .Must(ids => ids.Count <= 50)
            .WithMessage("Toi da 50 ca moi lan mo.");
    }
}
