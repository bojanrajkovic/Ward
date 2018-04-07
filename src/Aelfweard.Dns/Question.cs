using System.IO;
using System.Text;

namespace Aelfweard.Dns
{
    public class Question
    {
        public string Name { get; }
        public Type Type { get; }
        public Class Class { get; }

        public Question(string name, Type type, Class @class) {
            Name = name;
            Type = type;
            Class = @class;
        }

        public static Question ParseFromBytes(byte[] messageBytes, int offset)
        {
            var stream = new MemoryStream(messageBytes);
            stream.Position = offset;
            return ParseFromStream(stream);
        }

        public static Question ParseFromStream(MemoryStream stream)
        {
            using (var binReader = new BinaryReader(stream, Encoding.ASCII, true)) {
                var name = Utils.ParseQName(stream);
                var type = (Type)(Utils.SwapUInt16(binReader.ReadUInt16()));
                var @class = (Class)(Utils.SwapUInt16(binReader.ReadUInt16()));

                return new Question(name, type, @class);
            }
        }
    }
}
