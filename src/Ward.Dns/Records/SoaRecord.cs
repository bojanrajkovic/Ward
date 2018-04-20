using System;
using System.IO;
using System.Net;
using static Ward.Dns.Utils;

namespace Ward.Dns.Records
{
    public class SoaRecord : IRecord
    {
        public string Name { get; }
        public Type Type { get; }
        public Class Class { get; }
        public uint TimeToLive { get; }
        public ushort Length { get; }
        public ReadOnlyMemory<byte> Data { get; }
        public string PrimaryNameServer { get; }
        public string ResponsibleName { get; }
        public uint Serial { get; }
        public int Refresh { get; }
        public int Retry { get; }
        public int Expire { get; }
        public uint MinimumTtl { get; }

        public SoaRecord (
            string name,
            Dns.Type type,
            Class @class,
            uint timeToLive,
            ushort length,
            ReadOnlyMemory<byte> data,
            byte[] message
        ) {
            Name = name;
            Type = type;
            Class = @class;
            TimeToLive = timeToLive;
            Length = length;
            Data = data;

            var dataArray = data.ToArray();
            var offset = 0;
            PrimaryNameServer = Utils.ParseComplexName(message, dataArray, ref offset);
            ResponsibleName = Utils.ParseComplexName(message, dataArray, ref offset);
            Serial = Utils.SwapUInt32(BitConverter.ToUInt32(dataArray, offset));
            Refresh = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(dataArray, offset + 4));
            Retry = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(dataArray, offset + 8));
            Expire = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(dataArray, offset + 12));
            MinimumTtl = Utils.SwapUInt32(BitConverter.ToUInt32(dataArray, offset + 16));
        }

        public override string ToString() =>
            $"{Name}\t{TimeToLive}\t{Class}\t{Type}\t{PrimaryNameServer} {ResponsibleName} " +
            $"{Serial} {Refresh} {Retry} {Expire} {MinimumTtl}";
    }
}
