using InventoryService.Domain.Entities;
using System;
using System.Collections.Generic;

namespace InventoryService.Domain.Services;

/// <summary>
/// Service chứa các logic nghiệp vụ cốt lõi của kho hàng.
/// </summary>
public interface IInventoryDomainService
{
    /// <summary>
    /// Tính toán tổng lượng nguyên liệu cần thiết dựa trên danh sách món và công thức.
    /// </summary>
    /// <param name="items">Danh sách các món (VariantId, Số lượng).</param>
    /// <param name="recipes">Danh sách công thức của các món đó.</param>
    /// <returns>Dictionary: Key = IngredientId, Value = Tổng số lượng cần.</returns>
    Dictionary<Guid, decimal> CalculateRequiredIngredients(IEnumerable<(Guid VariantId, int Quantity)> items, IEnumerable<Recipe> recipes);

    /// <summary>
    /// Kiểm tra xem nguyên liệu có đang ở mức thấp (Low Stock) hay không.
    /// </summary>
    /// <param name="currentQuantity">Số lượng tồn kho hiện tại.</param>
    /// <param name="minThreshold">Mức cảnh báo tối thiểu (Threshold).</param>
    /// <returns>True nếu tồn kho < mức tối thiểu.</returns>
    bool IsLowStock(decimal currentQuantity, decimal minThreshold);
}
