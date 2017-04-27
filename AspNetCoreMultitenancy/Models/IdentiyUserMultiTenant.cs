using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace AspNetCoreMultitenancy.Models
{
    public class IdentiyUserMultiTenant<TKey, TTenantId> : IdentityUser<TKey>
        where TKey : IEquatable<TKey>
        where TTenantId : IEquatable<TTenantId>
    {
        public TTenantId TenantId { get; set; }
    }
}