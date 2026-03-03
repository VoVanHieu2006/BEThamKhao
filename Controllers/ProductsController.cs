using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopifyAPI.Data;
using ShopifyAPI.Models;
using System.Text.Json;

namespace ShopifyAPI.Controllers
{
    [Route("api/tenants/{tenantId}/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Lấy tất cả sản phẩm
        [HttpGet]
        public async Task<ActionResult> GetProducts(Guid tenantId)
        {
            var products = await _context.Products
                .Where(p => p.TenantId == tenantId)
                .Include(p => p.Category)
                .ToListAsync();

            return Ok(products);
        }

        // GET BY ID
        [HttpGet("{id}")]
        public async Task<ActionResult> GetProduct(Guid tenantId, Guid id)
        {
            var product = await _context.Products
                .Where(p => p.TenantId == tenantId && p.Id == id)
                .Include(p => p.Category)
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        // POST: Thêm sản phẩm
        [HttpPost]
        public async Task<ActionResult> CreateProduct(Guid tenantId, [FromBody] JsonElement data)
        {
            try
            {
                Console.WriteLine("=== CREATE PRODUCT ===");
                Console.WriteLine($"TenantId: {tenantId}");
                Console.WriteLine($"Raw Data: {data}");

                // Parse JSON
                string name = data.GetProperty("name").GetString() ?? "";
                string? description = data.TryGetProperty("description", out var descProp)
                    ? descProp.GetString()
                    : null;
                string? sku = data.TryGetProperty("sku", out var skuProp)
                    ? skuProp.GetString()
                    : null;

                decimal price = 0;
                if (data.TryGetProperty("price", out var priceProp))
                {
                    if (priceProp.ValueKind == JsonValueKind.Number)
                    {
                        price = priceProp.GetDecimal();
                    }
                    else if (priceProp.ValueKind == JsonValueKind.String)
                    {
                        decimal.TryParse(priceProp.GetString(), out price);
                    }
                }

                int stock = 0;
                if (data.TryGetProperty("stock", out var stockProp))
                {
                    if (stockProp.ValueKind == JsonValueKind.Number)
                    {
                        stock = stockProp.GetInt32();
                    }
                    else if (stockProp.ValueKind == JsonValueKind.String)
                    {
                        int.TryParse(stockProp.GetString(), out stock);
                    }
                }

                Guid? categoryId = null;
                if (data.TryGetProperty("categoryId", out var catProp))
                {
                    string? catStr = catProp.GetString();
                    if (!string.IsNullOrEmpty(catStr) && Guid.TryParse(catStr, out Guid catGuid))
                    {
                        categoryId = catGuid;
                    }
                }

                bool isActive = data.TryGetProperty("isActive", out var activeProp)
                    ? activeProp.GetBoolean()
                    : true;

                Console.WriteLine($"Parsed:");
                Console.WriteLine($"  Name: {name}");
                Console.WriteLine($"  Description: {description}");
                Console.WriteLine($"  SKU: {sku}");
                Console.WriteLine($"  Price: {price}");
                Console.WriteLine($"  Stock: {stock}");
                Console.WriteLine($"  CategoryId: {categoryId}");
                Console.WriteLine($"  IsActive: {isActive}");

                // Validate
                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest(new { message = "Tên sản phẩm không được để trống!" });
                }

                if (price <= 0)
                {
                    return BadRequest(new { message = "Giá sản phẩm phải lớn hơn 0!" });
                }

                // Create Product
                var product = new Product
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    CategoryId = categoryId,
                    Name = name.Trim(),
                    Description = description?.Trim(),
                    Sku = sku?.Trim(),
                    Price = price,
                    Stock = stock,
                    IsActive = isActive
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Product created: {product.Id}");

                return Ok(new
                {
                    id = product.Id,
                    name = product.Name,
                    description = product.Description,
                    sku = product.Sku,
                    price = product.Price,
                    stock = product.Stock,
                    categoryId = product.CategoryId,
                    isActive = product.IsActive
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                Console.WriteLine($"InnerException: {ex.InnerException?.Message}");

                return BadRequest(new
                {
                    message = "Lỗi khi tạo sản phẩm",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        // PUT: Cập nhật sản phẩm
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(Guid tenantId, Guid id, [FromBody] JsonElement data)
        {
            try
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == id);

                if (product == null)
                {
                    return NotFound(new { message = "Không tìm thấy sản phẩm!" });
                }

                // Update fields
                if (data.TryGetProperty("name", out var nameProp))
                {
                    product.Name = nameProp.GetString() ?? product.Name;
                }

                if (data.TryGetProperty("description", out var descProp))
                {
                    product.Description = descProp.GetString();
                }

                if (data.TryGetProperty("sku", out var skuProp))
                {
                    product.Sku = skuProp.GetString();
                }

                if (data.TryGetProperty("price", out var priceProp))
                {
                    if (priceProp.ValueKind == JsonValueKind.Number)
                    {
                        product.Price = priceProp.GetDecimal();
                    }
                    else if (priceProp.ValueKind == JsonValueKind.String &&
                             decimal.TryParse(priceProp.GetString(), out decimal parsedPrice))
                    {
                        product.Price = parsedPrice;
                    }
                }

                if (data.TryGetProperty("stock", out var stockProp))
                {
                    if (stockProp.ValueKind == JsonValueKind.Number)
                    {
                        product.Stock = stockProp.GetInt32();
                    }
                    else if (stockProp.ValueKind == JsonValueKind.String &&
                             int.TryParse(stockProp.GetString(), out int parsedStock))
                    {
                        product.Stock = parsedStock;
                    }
                }

                if (data.TryGetProperty("categoryId", out var catProp))
                {
                    string? catStr = catProp.GetString();
                    if (string.IsNullOrEmpty(catStr))
                    {
                        product.CategoryId = null;
                    }
                    else if (Guid.TryParse(catStr, out Guid catGuid))
                    {
                        product.CategoryId = catGuid;
                    }
                }

                if (data.TryGetProperty("isActive", out var activeProp))
                {
                    product.IsActive = activeProp.GetBoolean();
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    id = product.Id,
                    name = product.Name,
                    description = product.Description,
                    sku = product.Sku,
                    price = product.Price,
                    stock = product.Stock,
                    categoryId = product.CategoryId,
                    isActive = product.IsActive
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Update error: {ex.Message}");
                return BadRequest(new { message = "Lỗi khi cập nhật", error = ex.Message });
            }
        }

        // DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(Guid tenantId, Guid id)
        {
            try
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == id);

                if (product == null)
                {
                    return NotFound();
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Xóa thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi xóa", error = ex.Message });
            }
        }
    }
}