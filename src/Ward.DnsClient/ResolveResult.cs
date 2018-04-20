using System.Collections.Generic;
using System.Collections.Immutable;

using Ward.Dns;

namespace Ward.DnsClient
{
    public class ResolveResult : IResolveResult
    {
        public ImmutableList<IRecord> Results { get; }

        public ResolveResult(IEnumerable<IRecord> results) =>
            Results = results.ToImmutableList();
    }
}
