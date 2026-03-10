using MultiTenantManagement.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantManagement.Infrastructure.Features.Tenant.Dtos
{
    public class CreateTenantRequestDto
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = default!;

        public TenantStatus Status { get; set; }

        public string SubDomain { get; set; }

        public string? LogoURL { get; set; }

        public Guid? AttachmentId { get; set; }

        public StoreSettingDto? StoreSetting { get; set; }

    }
}
