﻿using System;
using System.Threading.Tasks;

using Aelfweard.Dns;

namespace Aelfweard.DnsClient
{
    public interface IDnsClient
    {
        Task<IResolveResult> ResolveAsync(Question question);
    }
}
