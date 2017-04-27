using System;

namespace AspNetCoreMultitenancy.Models
{
    public interface ITenantIdProvider<out T>
        where T : IEquatable<T>
    {
        T TenantId { get; }
    }
}