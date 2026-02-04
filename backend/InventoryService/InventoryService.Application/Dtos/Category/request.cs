using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Dtos.Category
{
    public class CreateCategoryRequest
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; } = null;
    }

    public class UpdateCategoryRequest
    {
        public Guid Id { get; set; }
        public string? Name { get; set; } = null;
        public string? Description { get; set; } = null;
    }
}
