using System;
using AspNetCoreMultitenancy.Data;

namespace AspNetCoreMultitenancy.Models
{
    public class ApplicationRoleStore : RoleStoreMultiTenant<ApplicationRole, Guid, string>
    {
        public ApplicationRoleStore(ApplicationDbContext context, ApplicationTenantIdProvider tenantProvider) : base(context, tenantProvider)
        {
            this.TenantKey = tenantProvider.TenantId;
        }
    }
}