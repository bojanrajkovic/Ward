using System.Collections.Immutable;

using Aelfweard.Dns;

namespace Aelfweard.DnsClient
{
    public interface IResolveResult
    {
        ImmutableList<Record> Results { get; }
    }
}
