using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantManagement.Core.Interfaces
{
    public interface ICurrentUserContext
    {
        string? UserId { get; }
        Guid? TenantId { get; }
        IReadOnlyList<string> Roles { get; }
        bool IsSystemAdmin { get; }
        bool IsTenantAdmin { get; }
    }
}
