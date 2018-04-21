using System.Collections.Immutable;

using Ward.Dns;

namespace Ward.DnsClient
{
    public interface IResolveResult
    {
        int MessageSize { get; }
        Header Header { get; }
        ImmutableList<Record> Answers { get; }
        ImmutableList<Record> Authority { get; }
        ImmutableList<Record> Additional { get; }
        ImmutableList<Question> Questions { get; }
    }
}
