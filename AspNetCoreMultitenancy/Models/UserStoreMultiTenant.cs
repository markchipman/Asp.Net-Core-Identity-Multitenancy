using System;
using System.Threading;
using System.Threading.Tasks;
using AspNetCoreMultitenancy.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AspNetCoreMultitenancy.Models
{
    public class UserStoreMultiTenant<TUser, TRole, TKey, TTenantId> : UserStore<TUser, TRole, ApplicationDbContext, TKey>
        where TUser : IdentiyUserMultiTenant<TKey, TTenantId>
        where TRole : IdentiyRoleMultiTenant<TKey, TTenantId>
        where TKey : IEquatable<TKey>
        where TTenantId : IEquatable<TTenantId>
    {
        public TTenantId TenantKey { get; set; }
        public UserStoreMultiTenant(ApplicationDbContext context, ITenantIdProvider<TTenantId> tenantProvider, IdentityErrorDescriber describer = null) : base(context, describer)
        {
            this.TenantKey = tenantProvider.TenantId;
        }

        public override Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken = new CancellationToken())
        {
            user.TenantId = this.TenantKey;
            return base.CreateAsync(user, cancellationToken);
        }
        public override Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            return EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(Users, u => u.NormalizedUserName == normalizedUserName && u.TenantId.Equals(this.TenantKey), cancellationToken);
        }
        public override Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            return EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(Users, u => u.NormalizedEmail == normalizedEmail && u.TenantId.Equals(this.TenantKey), cancellationToken);
        }
    }
}