using FluentValidation;

namespace SalesService.Application.Features.WithdrawalRequests.Commands.CreateWithdrawal;

public class CreateWithdrawalCommandValidator : AbstractValidator<CreateWithdrawalCommand>
{
    public CreateWithdrawalCommandValidator()
    {
        RuleFor(x => x.Amount)
            .NotEmpty().WithMessage("Số tiền không được để trống")
            .Must(BeAValidDecimal).WithMessage("Định dạng số tiền không hợp lệ")
            .Must(BePositive).WithMessage("Số tiền phải lớn hơn 0");
    }

    private bool BeAValidDecimal(string amount)
        => decimal.TryParse(amount, out _);

    private bool BePositive(string amount)
        => decimal.TryParse(amount, out var val) && val > 0;
}