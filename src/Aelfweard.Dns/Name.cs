using System.IO;
using System.Text;

namespace Aelfweard.Dns
{
    public class Name
    {
        byte[] originalMessage;
        ushort offsetToName;
        string cachedName;

        public Name(byte[] originalMessage, ushort offset)
        {
            this.originalMessage = originalMessage;
            offsetToName = (ushort)(offset & 0b0011111111111111);
        }

        public override string ToString()
        {
            if (cachedName != null)
                return cachedName;

            using (var messageStream = new MemoryStream(originalMessage)) {
                messageStream.Seek(offsetToName, SeekOrigin.Begin);
                cachedName = Utils.ParseQName(messageStream);
                return cachedName;
            }
        }
    }
}
