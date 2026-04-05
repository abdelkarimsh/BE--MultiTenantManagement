using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantManagement.Infrastructure.Features.Product.Dtos
{
    public class CreateProductDto
    {
        [Required]
        public Guid TenantId { get; set; }

        public Guid? AttachmentId { get; set; }

        [Required]
        [MaxLength(200)]
        public string? Name { get; set; }

        public string? Description { get; set; }

        [Range(0.0, double.MaxValue)]
        public decimal Price { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [Range(0, int.MaxValue)]
        public int? StockQuantity { get; set; }

        public bool IsActive { get; set; } = true;

    }
}
