using EngagementService.Domain.Commons;
using EngagementService.Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace EngagementService.Domain.Entities;

public partial class PointRule : IEntity
{
    [Key]
    public Guid Id { get; set; }

    public PointReasonCode ReasonCode { get; set; }

    public int ExpiryDays { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
