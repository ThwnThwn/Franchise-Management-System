using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Constants
{
    public static class Message
    {
        public static class Recipe
        {
            public const string NotFound = "Không tìm thấy công thức cho các sản phẩm đã chọn.";
            public const string AlreadyExists = "Công thức cho sản phẩm này đã tồn tại.";
        }

        public static class StoreIngredient
        {
            public const string NotFoundIngredient = "Không tìm thấy nguyên liệu trong kho.";
            public const string NotEnoughStock = "Không đủ nguyên liệu trong kho.";
        }
        public static class Ingredient
        {
            public const string NotFound = "Không tìm thấy nguyên liệu.";
            public const string Required = "Nguyên liệu Không được để trống.";
            public const string InvalidBaseUnit = "Đơn vị cơ bản không hợp lệ. Vui lòng sử dụng đơn vị đo khối lượng (g) hoặc thể tích (ml), hoặc tính cái (pcs)";
            public const string NameRequired = "Tên nguyên liệu không được để trống";
            public const string NameTooLong = "Tên nguyên liệu không được vượt quá 100 ký tự.";
            public const string BaseUnitRequired = "Đơn vị cơ bản không được để trống.";
            public const string CategoryRequired = "Danh mục nguyên liệu không được để trống.";
        }
            public static class User
        {
            public const string NotFound = "Không tìm thấy người dùng.";
        }

        public static class Category
        {
            public const string NotFound = "Không tìm thấy danh mục nguyên liệu.";
            public const string NameRequired = "Tên danh mục không được để trống.";
            public const string NameTooLong = "Tên danh mục không được vượt quá 100 ký tự.";
        }

        public static class ProductVariant
        {
            public const string NotFound = "Không tìm thấy biến thể sản phẩm.";
        }   
    }
}
