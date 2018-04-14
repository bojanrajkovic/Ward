using System.Threading.Tasks;

using Ward.Dns;

namespace Ward.DnsClient
{
    public interface IDnsClient
    {
        Task<IResolveResult> ResolveAsync(Question question);
        Task<IResolveResult> ResolveAsync(string host, Type type, Class @class);
    }
}
