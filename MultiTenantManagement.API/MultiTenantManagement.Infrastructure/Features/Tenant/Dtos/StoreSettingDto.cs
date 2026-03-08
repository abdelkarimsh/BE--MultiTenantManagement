using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantManagement.Infrastructure.Features.Tenant.Dtos
{
    public  class StoreSettingDto
    {
        
        public Guid? TenantId { get; set; }

        [Required]
        [MaxLength(10)]
        public string Currency { get; set; } = default!;

        [Required]
        [MaxLength(50)]
        public string Theme { get; set; } = default!;

        [Required]
        [MaxLength(30)]
        public string SupportPhone { get; set; } = default!;

    }
}
