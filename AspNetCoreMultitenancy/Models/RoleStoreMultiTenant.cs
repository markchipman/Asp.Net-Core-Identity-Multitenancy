using System;
using System.Threading;
using System.Threading.Tasks;
using AspNetCoreMultitenancy.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AspNetCoreMultitenancy.Models
{
    public class RoleStoreMultiTenant<TRole, TKey, TTenantId> : RoleStore<TRole, ApplicationDbContext, TKey>
        where TRole : IdentiyRoleMultiTenant<TKey, TTenantId>
        where TKey : IEquatable<TKey>
        where TTenantId : IEquatable<TTenantId>
    {
        public TTenantId TenantKey { get; set; }
        public RoleStoreMultiTenant(ApplicationDbContext context, ITenantIdProvider<TTenantId> tenantProvider, IdentityErrorDescriber describer = null) : base(context, describer)
        {
            this.TenantKey = tenantProvider.TenantId;
        }
        public override async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }
            role.TenantId = this.TenantKey;
            Context.Add(role);
            if (AutoSaveChanges)
            {
                await Context.SaveChangesAsync(cancellationToken);
            }
            return IdentityResult.Success;
        }
        public override Task<TRole> FindByNameAsync(string normalizedName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            return EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(Roles, r => r.NormalizedName == normalizedName && r.TenantId.Equals(this.TenantKey), cancellationToken);
        }
    }
}