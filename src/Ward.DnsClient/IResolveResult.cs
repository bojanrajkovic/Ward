using System.Collections.Immutable;

using Ward.Dns;

namespace Ward.DnsClient
{
    public interface IResolveResult
    {
        ImmutableList<Record> Results { get; }
    }
}
