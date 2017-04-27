using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace AspNetCoreMultitenancy.Models
{
    public class IdentiyRoleMultiTenant<TKey, TTenantId> : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
        where TTenantId : IEquatable<TTenantId>
    {
        public TTenantId TenantId { get; set; }
    }
}