using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;

namespace ProductService.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ProductDbContext context)
    {
        // Kiểm tra xem đã có data chưa
        if (await context.Categories.AnyAsync())
        {
            return; // Đã có data, không seed lại
        }

        var now = DateTime.UtcNow;

        // Tạo Categories
        var cafeCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Cafe",
            Description = "Các loại cà phê",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        var traCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Trà",
            Description = "Các loại trà",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        await context.Categories.AddRangeAsync(cafeCategory, traCategory);
        await context.SaveChangesAsync();

        // Tạo Products - Cafe (12 món)
        var cafeProducts = new List<Product>
        {
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Espresso",
                Description = "Cà phê espresso đậm đà, nguyên chất",
                CategoryId = cafeCategory.Id,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Americano",
                Description = "Espresso pha loãng với nước nóng",
                CategoryId = cafeCategory.Id,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Cappuccino",
                Description = "Espresso với sữa nóng và bọt sữa",
                CategoryId = cafeCategory.Id,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Latte",
                Description = "Espresso với nhiều sữa nóng",
                CategoryId = cafeCategory.Id,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Mocha",
                Description = "Latte với chocolate",
                CategoryId = cafeCategory.Id,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Macchiato",
                Description = "Espresso với một chút sữa",
                CategoryId = cafeCategory.Id,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Cà Phê Đen",
                Description = "Cà phê phin truyền thống",
                CategoryId = cafeCategory.Id,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Cà Phê Sữa",
                Description = "Cà phê phin với sữa đặc",
                CategoryId = cafeCategory.Id,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Cold Brew",
                Description = "Cà phê ủ lạnh",
                CategoryId = cafeCategory.Id,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Frappuccino",
                Description = "Cà phê đá xay",
                CategoryId = cafeCategory.Id,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Caramel Macchiato",
                Description = "Espresso với caramel và sữa",
                CategoryId = cafeCategory.Id,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Flat White",
                Description = "Espresso với sữa tươi",
                CategoryId = cafeCategory.Id,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = now,
                UpdatedAt = now
            }
        };

        await context.Products.AddRangeAsync(cafeProducts);
        await context.SaveChangesAsync();

        // Tạo Products - Trà (8 món)
        var traProducts = new List<Product>
        {
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Trà Đen",
                Description = "Trà đen truyền thống",
                CategoryId = traCategory.Id,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Trà Xanh",
                Description = "Trà xanh thanh mát",
                CategoryId = traCategory.Id,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Trà Oolong",
                Description = "Trà oolong thơm ngon",
                CategoryId = traCategory.Id,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Trà Sữa",
                Description = "Trà sữa thơm ngon",
                CategoryId = traCategory.Id,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Trà Đào",
                Description = "Trà đào mát lạnh",
                CategoryId = traCategory.Id,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Trà Chanh",
                Description = "Trà chanh giải nhiệt",
                CategoryId = traCategory.Id,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Trà Bạc Hà",
                Description = "Trà bạc hà thơm mát",
                CategoryId = traCategory.Id,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Trà Gừng",
                Description = "Trà gừng ấm bụng",
                CategoryId = traCategory.Id,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = now,
                UpdatedAt = now
            }
        };

        await context.Products.AddRangeAsync(traProducts);
        await context.SaveChangesAsync();

        // Tạo ProductVariants (Sizes) cho tất cả products
        var allProducts = cafeProducts.Concat(traProducts).ToList();
        var variants = new List<ProductVariant>();

        foreach (var product in allProducts)
        {
            // Xác định giá base theo loại
            decimal basePriceS, basePriceM, basePriceL;
            decimal costS, costM, costL;

            if (product.CategoryId == cafeCategory.Id)
            {
                // Cafe: giá cao hơn
                basePriceS = 25000;
                basePriceM = 35000;
                basePriceL = 45000;
                costS = 15000;
                costM = 20000;
                costL = 25000;
            }
            else
            {
                // Trà: giá thấp hơn
                basePriceS = 20000;
                basePriceM = 28000;
                basePriceL = 35000;
                costS = 10000;
                costM = 15000;
                costL = 18000;
            }

            // Size S
            variants.Add(new ProductVariant
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Sku = $"{product.Name.ToUpper().Replace(" ", "-")}-S",
                Name = "Size S",
                Attributes = "{\"size\":\"S\",\"volume\":\"250ml\"}",
                BasePrice = basePriceS,
                Cost = costS,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = now,
                UpdatedAt = now
            });

            // Size M
            variants.Add(new ProductVariant
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Sku = $"{product.Name.ToUpper().Replace(" ", "-")}-M",
                Name = "Size M",
                Attributes = "{\"size\":\"M\",\"volume\":\"350ml\"}",
                BasePrice = basePriceM,
                Cost = costM,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = now,
                UpdatedAt = now
            });

            // Size L
            variants.Add(new ProductVariant
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Sku = $"{product.Name.ToUpper().Replace(" ", "-")}-L",
                Name = "Size L",
                Attributes = "{\"size\":\"L\",\"volume\":\"500ml\"}",
                BasePrice = basePriceL,
                Cost = costL,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        await context.ProductVariants.AddRangeAsync(variants);
        await context.SaveChangesAsync();
    }
}
