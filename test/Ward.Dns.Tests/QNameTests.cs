using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Ward.Dns.Records;
using Xunit;

namespace Ward.Dns.Tests
{
    public class QNameTests
    {
        [Theory]
        [InlineData("🍆👊.ws", new byte[] { 0x0a, 0x78, 0x6e, 0x2d, 0x2d, 0x67, 0x69, 0x38, 0x68, 0x69, 0x71, 0x02, 0x77, 0x73, 0x00 })]
        [InlineData("ssl.gstatic.com", new byte[] { 0x03, 0x73, 0x73, 0x6c, 0x07, 0x67, 0x73, 0x74, 0x61, 0x74, 0x69, 0x63, 0x03, 0x63, 0x6f, 0x6d, 0x00 })]
        public void Can_write_qname(string name, byte[] expectedQName)
        {
            var qname = Utils.WriteQName(name, null);
            Assert.Equal(expectedQName, qname);
        }

        [Fact]
        public async Task Writes_recursive_names_correctly()
        {
            var headerFlags = new Header.HeaderFlags(false, false, false, true, true, false, false, false);
            var header = new Header(0xaaaa, Opcode.Query, ReturnCode.NoError, headerFlags, 1, 1, 0, 0);
            var question = new Question("google.com", Type.MX, Class.Internet);
            var answers = new Record[] {
                new MailExchangerRecord("google.com", Class.Internet, 600, 1, "aspmx.l.google.com"),
                new MailExchangerRecord("google.com", Class.Internet, 600, 5, "alt1.aspmx.l.google.com")
            };
            var message = new Message(
                header,
                new [] { question },
                answers,
                Array.Empty<Record>(),
                Array.Empty<Record>()
            );
            ReadOnlyMemory<byte> messageData = await MessageWriter.SerializeMessageAsync(message);

            // Now we play the game of where are the answers that we want to check on...
            // The header is 12 bytes, the question section is 16 bytes long
            // (1 byte + 6 bytes of google + 1 byte + 3 bytes of com + null terminator
            // + 4 bytes of type and class), so the first answer will be at 28 bytes.
            Assert.Equal(new byte[] { 0xc0, 0x0c }, messageData.Slice(28, 2).ToArray());
            // To get to where the next encoded name is, we add 2 bytes for the name we
            // just checked, 8 bytes for the type, class, and TTL, 2 bytes for the data
            // length, and 2 bytes for the MX preference, which means we should expect
            // our next name at 42 bytes. We expect it should be 10 bytes long: 1 byte of
            // length + 5 bytes for aspmx, 1 byte of length + 1 byte for l, and 2 bytes
            // of offset to google.com earlier in the message.
            Assert.Equal(
                new byte[] { 0x05, (byte)'a', (byte)'s', (byte)'p', (byte)'m', (byte)'x', 0x01, (byte)'l', 0xc0, 0x0c },
                messageData.Slice(42, 10).ToArray()
            );
            // The next encoded name is just after this, at the start of the next record,
            // at 52 bytes. It should be again just a pointer to google.com at position 12.
            Assert.Equal(new byte[] { 0xc0, 0x0c }, messageData.Slice(52, 2).ToArray());
            // And following the same logic as before, we add 2+8+2+2 (14) bytes for the next
            // name at 66 bytes. It should be 7 bytes long: 1 byte for length, 4 bytes for alt1,
            // and a pointer to aspmx.l.google.com at byte 42.
            Assert.Equal(
                new byte[] { 0x04, (byte)'a', (byte)'l', (byte)'t', (byte)'1', 0xc0, 0x2a },
                messageData.Slice(66, 7).ToArray()
            );
        }

        [Fact]
        public void Can_roundtrip_name_with_offset()
        {
            var offsetMap = new Dictionary<string, ushort> {
                ["aspmx.l.google.com"] = 0
            };
            var baseName = Utils.WriteQName("aspmx.l.google.com", null);
            var name = "alt4.aspmx.l.google.com";
            var qname = Utils.WriteQName(name, offsetMap);

            var data = new byte[baseName.Length + qname.Length];
            Buffer.BlockCopy(baseName, 0, data, 0, baseName.Length);
            Buffer.BlockCopy(qname, 0, data, baseName.Length, qname.Length);

            var offset = baseName.Length;
            var parsedName = Utils.ParseComplexName(data, null, ref offset);

            Assert.Equal("alt4.aspmx.l.google.com.", parsedName);
        }

        [Fact]
        public void Can_write_offset_qname()
        {
            var offsetMap = new Dictionary<string, ushort> {
                ["aspmx.l.google.com"] = 12
            };
            var name = "alt4.aspmx.l.google.com";
            var qname = Utils.WriteQName(name, offsetMap);

            Assert.Equal(new byte [] {
                0x04,
                (byte)'a',
                (byte)'l',
                (byte)'t',
                (byte)'4',
                0xc0,
                0x0c
            }, qname);
        }
    }
}
