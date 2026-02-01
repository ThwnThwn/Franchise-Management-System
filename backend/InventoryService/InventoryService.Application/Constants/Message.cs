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
        }
        public static class User
        {
            public const string NotFound = "Không tìm thấy người dùng.";
        }
    }
}
