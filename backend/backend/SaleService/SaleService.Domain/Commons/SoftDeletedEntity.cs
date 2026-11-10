namespace SalesService.Domain.Commons
{
    public abstract class SoftDeletedEntity
    {
        public bool IsDeleted { get; set; }
    }
}