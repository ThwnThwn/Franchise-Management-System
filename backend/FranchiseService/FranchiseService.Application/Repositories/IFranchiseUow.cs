namespace FranchiseService.Application.Repositories
{
    public interface IFranchiseUow
    {
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Lưu EF Core Changes và Commit Transaction.
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Hủy bỏ nếu có lỗi (DB lỗi, Kafka lỗi, hoặc Cloudinary lỗi).
        /// </summary>
        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}