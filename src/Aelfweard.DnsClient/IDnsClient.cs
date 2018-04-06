using System;
using System.Threading.Tasks;

namespace Aelfweard.DnsClient
{
    public interface IDnsClient
    {
        Task<IResolveResult> ResolveAsync(string host);
    }
}
