using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantManagement.Infrastructure.Features.Tenant.Dtos
{
    public class UpdateTenantRequestDto : CreateTenantRequestDto 
    {
        public Guid Id { get; set; }
    }
}
