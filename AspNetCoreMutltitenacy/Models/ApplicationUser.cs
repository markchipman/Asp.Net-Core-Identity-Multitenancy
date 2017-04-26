using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AspNetCore.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore.Models
{
    public interface ITenantIdProvider<out T>
        where T : IEquatable<T>
    {
        T TenantId { get; }
    }

    public class ApplicationTenantIdProvider<TTenantId> : ITenantIdProvider<TTenantId>
        where TTenantId : IEquatable<TTenantId>
    {
        public ApplicationTenantIdProvider(TTenantId teantId)
        {
            this.TenantId = teantId;
        }
        public TTenantId TenantId { get; }
    }

    public class ApplicationTenantIdProvider : ApplicationTenantIdProvider<string>
    {
        public ApplicationTenantIdProvider(string tenantId) : base(tenantId) { }
    }

    public class IdentiyUserMultiTenant<TKey, TTenantId> : IdentityUser<TKey>
        where TKey : IEquatable<TKey>
        where TTenantId : IEquatable<TTenantId>
    {
        public TTenantId TenantId { get; set; }
    }

    public class IdentiyRoleMultiTenant<TKey, TTenantId> : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
        where TTenantId : IEquatable<TTenantId>
    {
        public TTenantId TenantId { get; set; }
    }

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
            return Users.FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName && u.TenantId.Equals(this.TenantKey), cancellationToken);
        }
        public override Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            return Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail && u.TenantId.Equals(this.TenantKey), cancellationToken);
        }
    }

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
            return Roles.FirstOrDefaultAsync(r => r.NormalizedName == normalizedName && r.TenantId.Equals(this.TenantKey), cancellationToken);
        }
    }


    public class ApplicationUser : IdentiyUserMultiTenant<Guid, string>
    {
    }

    public class ApplicationRole : IdentiyRoleMultiTenant<Guid, string>
    {
    }


    public class ApplicationUserStore : UserStoreMultiTenant<ApplicationUser, ApplicationRole, Guid, string>
    {
        public ApplicationUserStore(ApplicationDbContext context, ApplicationTenantIdProvider tenantProvider) : base(context, tenantProvider)
        {
            this.TenantKey = tenantProvider.TenantId;
        }
    }
    public class ApplicationRoleStore : RoleStoreMultiTenant<ApplicationRole, Guid, string>
    {
        public ApplicationRoleStore(ApplicationDbContext context, ApplicationTenantIdProvider tenantProvider) : base(context, tenantProvider)
        {
            this.TenantKey = tenantProvider.TenantId;
        }
    }

    // Add profile data for application users by adding properties to the ApplicationUser class
}
