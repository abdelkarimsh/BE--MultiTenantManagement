using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantManagement.Infrastructure.Features.Product.Dtos
{
    public class ProductDto
    {

        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public int? StockQuantity { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAtUtc { get; set; }

    }
}
