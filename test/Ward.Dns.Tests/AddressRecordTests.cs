using System;
using Ward.Dns.Records;
using Xunit;

namespace Ward.Dns.Tests
{
    public class AddressRecordTests
    {
        [Fact]
        public void Address_record_throws_if_you_pass_bad_type()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new AddressRecord(null, Type.MX, Class.Internet, 0, 0, null)
            );
        }
    }
}
