using System;
using System.Collections.Generic;

namespace ShopifyAPI.Models;

public partial class Product
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid? CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Sku { get; set; }

    public decimal Price { get; set; }

    public int? Stock { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Category? Category { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<ProductAttribute> ProductAttributes { get; set; } = new List<ProductAttribute>();

    public virtual Tenant Tenant { get; set; } = null!;
}
