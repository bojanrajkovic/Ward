using System;
using System.Collections.Generic;
using System.Linq;

using Nett;
using Ward.Dns;
using Ward.Dns.Records;
using Xunit;

namespace Ward.Dns.Tests
{
    static class Helpers
    {
        public static void ForEach<T>(this IList<T> list, Action<T, int> callback)
        {
            for (var i = 0; i < list.Count; i++)
                callback(list[i], i);
        }
    }
}

namespace Xunit
{
    public partial class Assert
    {
        public static void Question(TomlTable expectedQuestion, Question question)
        {
            Assert.Equal(expectedQuestion.Get<string>("name"), question.Name);
            Assert.Equal(expectedQuestion.Get<Class>("class"), question.Class);
            Assert.Equal(expectedQuestion.Get<Ward.Dns.Type>("type"), question.Type);
        }

        public static void Record(TomlTable expectedRecord, Ward.Dns.IRecord record)
        {
            Assert.Equal(expectedRecord.TryGetValue("name")?.Get<string>(), record.Name);
            Assert.Equal(expectedRecord.Get<Ward.Dns.Type>("type"), record.Type);

            var @class = expectedRecord.Get("class");
            if (@class.TomlType == TomlObjectType.Int)
                Assert.Equal((Class)(@class.Get<int>()), record.Class);
            else
                Assert.Equal(@class.Get<Class>(), record.Class);

            Assert.Equal(expectedRecord.Get<uint>("ttl"), record.TimeToLive);
            Assert.Equal(expectedRecord.Get<ushort>("length"), record.Length);
            Assert.Equal(
                expectedRecord.Get<string>("data"),
                record.Data.ToArray().Aggregate(string.Empty, (s, v) => {
                    return s += v.ToString("X2").ToLower();
                })
            );

            switch (record.Type) {
                case Ward.Dns.Type.A:
                    Assert.ARecord(expectedRecord, (AddressRecord)record);
                    break;
                case Ward.Dns.Type.MX:
                    Assert.MXRecord(expectedRecord, (MailExchangerRecord)record);
                    break;
            }
        }

        public static void MXRecord(TomlTable expectedRecord, MailExchangerRecord record)
        {
            Assert.Equal(expectedRecord.Get<int>("preference"), record.Preference);
            Assert.Equal(expectedRecord.Get<string>("hostname"), record.Hostname);
        }

        public static void ARecord(TomlTable expectedRecord, AddressRecord record)
        {
            Assert.Equal(expectedRecord.Get<string>("address"), record.Address.ToString());
        }

        public static void HeaderFlags(TomlTable expectedHeader, Header.HeaderFlags flags)
        {
            Assert.Equal(expectedHeader.Get<bool>("query"), flags.Query);
            Assert.Equal(expectedHeader.Get<bool>("authoritative"), flags.Authoritative);
            Assert.Equal(expectedHeader.Get<bool>("truncated"), flags.Truncated);
            Assert.Equal(expectedHeader.Get<bool>("recurse"), flags.Recurse);
            Assert.Equal(expectedHeader.Get<bool>("recursionAvailable"), flags.RecursionAvailable);
            Assert.Equal(expectedHeader.Get<bool>("z"), flags.Z);
            Assert.Equal(expectedHeader.Get<bool>("authenticated"), flags.Authenticated);
            Assert.Equal(expectedHeader.Get<bool>("checkingDisabled"), flags.CheckingDisabled);
        }

        public static void Header(TomlTable expectedHeader, Header header)
        {
            Assert.Equal(expectedHeader.Get<ushort>("id"), header.Id);
            Assert.Equal(expectedHeader.Get<Opcode>("opcode"), header.Opcode);
            Assert.Equal(expectedHeader.Get<ReturnCode>("returnCode"), header.ReturnCode);
            Assert.HeaderFlags(expectedHeader, header.Flags);
            Assert.Equal(expectedHeader.Get<int>("questions"), header.TotalQuestions);
            Assert.Equal(expectedHeader.Get<int>("answers"), header.TotalAnswerRecords);
            Assert.Equal(expectedHeader.Get<int>("authority"), header.TotalAuthorityRecords);
            Assert.Equal(expectedHeader.Get<int>("additional"), header.TotalAdditionalRecords);
        }
    }
}
