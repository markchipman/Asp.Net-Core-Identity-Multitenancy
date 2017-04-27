using System;
using AspNetCoreMultitenancy.Data;

namespace AspNetCoreMultitenancy.Models
{
    public class ApplicationUserStore : UserStoreMultiTenant<ApplicationUser, ApplicationRole, Guid, string>
    {
        public ApplicationUserStore(ApplicationDbContext context, ApplicationTenantIdProvider tenantProvider) : base(context, tenantProvider)
        {
            this.TenantKey = tenantProvider.TenantId;
        }
    }
}