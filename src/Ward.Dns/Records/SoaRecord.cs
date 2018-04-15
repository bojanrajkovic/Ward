using System;
using System.IO;
using System.Net;
using static Ward.Dns.Utils;

namespace Ward.Dns.Records
{
    public class SoaRecord : Record
    {
        readonly byte[] message;

        public SoaRecord (
            string name,
            Dns.Type type,
            Class @class,
            uint timeToLive,
            ushort length,
            byte[] data,
            byte[] message
        ) : base(name, type, @class, timeToLive, length, data) {
            this.message = message;

            using (var memoryStream = new MemoryStream(data))
            using (var binaryReader = new BinaryReader(memoryStream)) {
                PrimaryNameServer = Utils.ParseComplexName(message, memoryStream);
                ResponsibleName = Utils.ParseComplexName(message, memoryStream);
                Serial = Utils.SwapUInt32(binaryReader.ReadUInt32());
                Refresh = IPAddress.NetworkToHostOrder(binaryReader.ReadInt32());
                Retry = IPAddress.NetworkToHostOrder(binaryReader.ReadInt32());
                Expire = IPAddress.NetworkToHostOrder(binaryReader.ReadInt32());
                MinimumTtl = Utils.SwapUInt32(binaryReader.ReadUInt32());
            }
        }

        public string PrimaryNameServer { get; }
        public string ResponsibleName { get; }
        public uint Serial { get; }
        public int Refresh { get; }
        public int Retry { get; }
        public int Expire { get; }
        public uint MinimumTtl { get; }

        public override string ToString() =>
            $"{Name}\t{TimeToLive}\t{Class}\t{Type}\t{PrimaryNameServer} {ResponsibleName} " +
            $"{Serial} {Refresh} {Retry} {Expire} {MinimumTtl}";
    }
}
