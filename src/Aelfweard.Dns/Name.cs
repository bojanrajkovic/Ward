using System.IO;
using System.Text;

namespace Aelfweard.Dns
{
    public class Name
    {
        byte[] originalMessage;
        ushort offsetToName;
        string cachedName;

        public Name(byte[] originalMessage, ushort value)
        {
            this.originalMessage = originalMessage;
            offsetToName = (ushort)(value & 0b0011111111111111);
        }

        public override string ToString()
        {
            if (cachedName != null)
                return cachedName;

            var nameBuilder = new StringBuilder();
            var messageStream = new MemoryStream(originalMessage);
            messageStream.Seek(offsetToName, SeekOrigin.Begin);
            var reader = new BinaryReader(messageStream);

            while (true) {
                var nextByte = reader.ReadByte();

                // Null terminator ends the QNAME section.
                if (nextByte == 0)
                    break;

                var labelBytes = reader.ReadBytes(nextByte);
                nameBuilder.Append(Encoding.ASCII.GetString(labelBytes));
                nameBuilder.Append('.');
            }

            cachedName = nameBuilder.Remove(nameBuilder.Length-1, 1).ToString();
            return cachedName;
        }
    }
}
