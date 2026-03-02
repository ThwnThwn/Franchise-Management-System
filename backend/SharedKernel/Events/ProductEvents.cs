namespace SharedKernel.Events
{
    // Sự kiện đồng bộ hóa sản phẩm (Dùng cho cả Create và Update)
    public record ProductPersistedIntegrationEvent(
        Guid Id,
        string Name,
        string? Description,
        string? ImageUrl,
        string? ImagePublicId,
        bool IsActive,
        bool IsDeleted, // Để xử lý Soft Delete
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt,

        // Thông tin Category
        Guid CategoryId,
        string CategoryName,

        // Danh sách Variants (Bao gồm đầy đủ Attributes)
        List<ProductVariantIntegrationModel> Variants
    );

    public record ProductVariantIntegrationModel(
        Guid Id,
        string Sku,
        string? Name,
        string? Attributes, // <--- Đã bổ sung theo yêu cầu của bro
        decimal BasePrice,
        decimal Cost,
        bool IsActive
    );

    // Sự kiện xóa hẳn sản phẩm
    public record ProductDeletedIntegrationEvent(Guid Id);
}