using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantManagement.Core.Interfaces
{
    public interface ICurrentUserContext
    {
        Guid? TenantId { get; }
        bool IsSuperAdmin { get; }
        string? UserId { get; }
    }
}
