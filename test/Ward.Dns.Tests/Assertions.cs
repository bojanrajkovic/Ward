using System;
using System.Collections.Generic;
using System.Linq;

using Nett;
using Ward.Dns;
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

        public static void Record(TomlTable expectedRecord, Ward.Dns.Record record)
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
            Assert.Equal(expectedRecord.Get<string>("data"), record.Data.Aggregate(string.Empty, (s, v) => {
                return s += v.ToString("X2").ToLower();
            }));
        }

        public static void Header(TomlTable expectedHeader, Header header)
        {
            Assert.Equal(expectedHeader.Get<ushort>("id"), header.Id);
            Assert.Equal(expectedHeader.Get<bool>("query"), header.Query);
            Assert.Equal(expectedHeader.Get<bool>("authoritative"), header.Authoritative);
            Assert.Equal(expectedHeader.Get<Opcode>("opcode"), header.Opcode);
            Assert.Equal(expectedHeader.Get<bool>("truncated"), header.Truncated);
            Assert.Equal(expectedHeader.Get<bool>("recurse"), header.Recurse);
            Assert.Equal(expectedHeader.Get<bool>("recursionAvailable"), header.RecursionAvailable);
            Assert.Equal(expectedHeader.Get<bool>("z"), header.Z);
            Assert.Equal(expectedHeader.Get<bool>("authenticated"), header.Authenticated);
            Assert.Equal(expectedHeader.Get<bool>("checkingDisabled"), header.CheckingDisabled);
            Assert.Equal(expectedHeader.Get<ReturnCode>("returnCode"), header.ReturnCode);

            Assert.Equal(expectedHeader.Get<int>("questions"), header.TotalQuestions);
            Assert.Equal(expectedHeader.Get<int>("answers"), header.TotalAnswerRecords);
            Assert.Equal(expectedHeader.Get<int>("authority"), header.TotalAuthorityRecords);
            Assert.Equal(expectedHeader.Get<int>("additional"), header.TotalAdditionalRecords);
        }
    }
}
