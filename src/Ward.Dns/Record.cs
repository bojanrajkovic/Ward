using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Ward.Dns.Records;
using static Ward.Dns.Utils;

namespace Ward.Dns
{
    public class Record
    {
        public string Name { get; }
        public Type Type { get; }
        public Class Class { get; }
        public uint TimeToLive { get; }
        public ushort Length { get; }
        public ReadOnlyMemory<byte> Data { get; }

        public Record(
            string name,
            Type type,
            Class @class,
            uint timeToLive,
            ushort length,
            ReadOnlyMemory<byte> data
        ) {
            Name = name;
            Type = type;
            Class = @class;
            TimeToLive = timeToLive;
            Length = length;
            Data = data;
        }
    }
}
