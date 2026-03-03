using System;
using System.Collections.Generic;

namespace ShopifyAPI.Models;

public partial class ProductAttribute
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public string Name { get; set; } = null!;

    public string Value { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
