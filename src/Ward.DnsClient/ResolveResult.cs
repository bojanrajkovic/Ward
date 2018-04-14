using System.Collections.Generic;
using System.Collections.Immutable;

using Ward.Dns;

namespace Ward.DnsClient
{
    public class ResolveResult : IResolveResult
    {
        public ImmutableList<Record> Results { get; }

        public ResolveResult(IEnumerable<Record> results) =>
            Results = results.ToImmutableList();
    }
}
