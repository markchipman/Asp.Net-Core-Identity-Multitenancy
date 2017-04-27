using System;

namespace AspNetCoreMultitenancy.Models
{
    public class TenantIdProvider<TTenantId> : ITenantIdProvider<TTenantId>
        where TTenantId : IEquatable<TTenantId>
    {
        public TenantIdProvider(TTenantId teantId)
        {
            this.TenantId = teantId;
        }
        public TTenantId TenantId { get; }
    }
}