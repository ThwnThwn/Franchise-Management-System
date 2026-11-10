using SalesService.Domain.Commons;
using SalesService.Domain.Enums;

namespace SalesService.Domain.Entities
{
    public class OrderSaleItem : SoftDeletedEntity, IEntity
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public OrderSaleItemType Type { get; set; }
        public Guid? ReferenceId { get; set; }
        public string Name { get; set; } = null!;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public Guid OrderId { get; set; }
        public virtual Order Order { get; set; } = null!;
    }
}
