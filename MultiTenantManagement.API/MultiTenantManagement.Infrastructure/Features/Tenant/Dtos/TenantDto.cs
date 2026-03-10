using MultiTenantManagement.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantManagement.Infrastructure.Features.Tenant.Dtos
{
    public class TenantDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Status { get; set; }

        public string? LogoURL { get; set; }
        public Guid? AttachmentId { get; set; }
        public string? AttachmentUrl { get; set; }

        public string SubDomain { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public StoreSettingDto StoreSetting { get; set; }

    }
}
