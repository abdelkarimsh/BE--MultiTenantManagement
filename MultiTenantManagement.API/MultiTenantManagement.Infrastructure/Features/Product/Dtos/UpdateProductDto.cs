using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantManagement.Infrastructure.Features.Product.Dtos
{
    public class UpdateProductDto : CreateProductDto
    {
        public Guid Id { get; set; }
    }
}
