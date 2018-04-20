using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Ward.Dns.Records;
using static Ward.Dns.Utils;

namespace Ward.Dns
{
    public interface IRecord
    {
        string Name { get; }
        Type Type { get; }
        Class Class { get; }
        uint TimeToLive { get; }
        ushort Length { get; }
        ReadOnlyMemory<byte> Data { get; }
    }
}
