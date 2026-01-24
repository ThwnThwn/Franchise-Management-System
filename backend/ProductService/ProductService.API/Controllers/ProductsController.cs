using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.DTOs.Responses;
using ProductService.Application.Services;
using AppRequests = ProductService.Application.DTOs.Requests;

namespace ProductService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductApplicationService _productService;

    public ProductsController(ProductApplicationService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Lấy danh sách menu (tất cả sản phẩm kèm Size và Giá)
    /// </summary>
    /// <returns>Danh sách sản phẩm</returns>
    [HttpGet]
    public async Task<ActionResult<ProductListResponse>> GetProducts()
    {
        try
        {
            var result = await _productService.GetProductsAsync(new AppRequests.GetProductsRequest { CategoryId = null });
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Đã xảy ra lỗi khi lấy danh sách sản phẩm", details = ex.Message });
        }
    }

    /// <summary>
    /// Lấy thông tin chi tiết sản phẩm theo ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Thông tin sản phẩm</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductResponse>> GetProductById(Guid id)
    {
        try
        {
            var result = await _productService.GetProductByIdAsync(new AppRequests.GetProductByIdRequest { Id = id });
            
            if (result == null)
                return NotFound(new { error = "Không tìm thấy sản phẩm" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Đã xảy ra lỗi khi lấy thông tin sản phẩm", details = ex.Message });
        }
    }
}
