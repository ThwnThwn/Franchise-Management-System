using MediatR;

namespace FranchiseService.Application.Features.ShiftApplications.Commands.ReviewApplication;

/// <summary>
/// Command cho Manager duyet (Approve) hoac tu choi (Reject) don dang ky nhan ca.
/// Khi duyet: tu dong tao ShiftAssignment.
/// Khi du nguoi: tu dong chuyen Shift ve Published va tu choi cac don con lai.
/// </summary>
public class ReviewApplicationCommand : IRequest<bool>
{
    /// <summary>
    /// ID don dang ky can duyet
    /// </summary>
    public Guid ApplicationId { get; set; }

    /// <summary>
    /// true = Duyet (Approve), false = Tu choi (Reject)
    /// </summary>
    public bool IsApproved { get; set; }

    /// <summary>
    /// ID Manager dang duyet
    /// </summary>
    public Guid ReviewedBy { get; set; }

    /// <summary>
    /// Ghi chu cua Manager (bat buoc khi tu choi)
    /// </summary>
    public string? ReviewNote { get; set; }
}
