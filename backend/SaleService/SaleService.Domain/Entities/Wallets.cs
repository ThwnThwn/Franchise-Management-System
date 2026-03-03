using System.ComponentModel.DataAnnotations;
using SalesService.Domain.Commons;
using SalesService.Domain.Enums;

namespace SalesService.Domain.Entities
{
    public partial class Wallet : SoftDeletedEntity,IEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public decimal Balance { get; set; }
        public WalletStatus Status { get; set; }

        [ConcurrencyCheck]
        public byte[]? RowVersion { get; set; } = default;
        public string? Notes { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public Guid UpdatedBy { get; set; }
    }
}
