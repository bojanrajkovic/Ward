using System.Threading;
using System.Threading.Tasks;

using Ward.Dns;

namespace Ward.DnsClient
{
    public interface IDnsClient
    {
        Task<IResolveResult> ResolveAsync(Question question, CancellationToken cancellationToken = default);
        Task<IResolveResult> ResolveAsync(string host, Type type, Class @class, CancellationToken cancellationToken = default);
    }
}
