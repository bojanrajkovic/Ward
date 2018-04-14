using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Aelfweard.Dns
{
    public class Question
    {
        public string Name { get; }
        public Type Type { get; }
        public Class Class { get; }

        internal static Question ParseFromStream(Stream stream)
        {
            using (var binReader = new BinaryReader(stream, Encoding.ASCII, true)) {
                var name = Utils.ParseComplexName(null, stream);
                var type = (Type)Utils.SwapUInt16(binReader.ReadUInt16());
                var @class = (Class)Utils.SwapUInt16(binReader.ReadUInt16());

                return new Question(name, type, @class);
            }
        }

        public Question(string name, Type type, Class @class) {
            Name = name;
            Type = type;
            Class = @class;
        }

        internal async Task WriteToStreamAsync(Stream s)
        {
            var qname = Utils.WriteQName(Name);
            await s.WriteAsync(qname, 0, qname.Length);

            var type = BitConverter.GetBytes(Utils.SwapUInt16((ushort) Type));
            await s.WriteAsync(type, 0, 2);

            var @class = BitConverter.GetBytes(Utils.SwapUInt16((ushort) Class));
            await s.WriteAsync(@class, 0, 2);
        }
    }
}
