using System.Collections.Generic;
using System.Collections.Immutable;

using Aelfweard.Dns;

namespace Aelfweard.DnsClient
{
    public class ResolveResult : IResolveResult
    {
        public ImmutableList<Record> Results { get; }

        public ResolveResult(IEnumerable<Record> results) =>
            Results = ImmutableList.ToImmutableList(results);
    }
}
