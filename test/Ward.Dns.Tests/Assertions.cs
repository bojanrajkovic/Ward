using System;
using System.Collections.Generic;
using System.Linq;

using Nett;
using Ward.Dns;
using Ward.Dns.Records;
using Ward.Dns.Tests;
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
            Equal(expectedQuestion.Get<string>("name"), question.Name);
            Equal(expectedQuestion.Get<Class>("class"), question.Class);
            Equal(expectedQuestion.Get<Ward.Dns.Type>("type"), question.Type);
        }

        public static void Record(TomlTable expectedRecord, Ward.Dns.Record record)
        {
            Equal(expectedRecord.TryGetValue("name")?.Get<string>(), record.Name);
            Equal(expectedRecord.Get<Ward.Dns.Type>("type"), record.Type);

            var @class = expectedRecord.Get("class");
            if (@class.TomlType == TomlObjectType.Int)
                Equal((Class)(@class.Get<int>()), record.Class);
            else
                Equal(@class.Get<Class>(), record.Class);

            Equal(expectedRecord.Get<uint>("ttl"), record.TimeToLive);
            Equal(expectedRecord.Get<ushort>("length"), record.Length);
            Equal(
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
                case Ward.Dns.Type.SOA:
                    Assert.SOARecord(expectedRecord, (SoaRecord)record);
                    break;
                case Ward.Dns.Type.CAA:
                    Assert.CAARecord(expectedRecord, (CaaRecord)record);
                    break;
                case Ward.Dns.Type.CNAME:
                    Assert.CNAMERecord(expectedRecord, (CnameRecord)record);
                    break;
                case Ward.Dns.Type.TXT:
                    Assert.TXTRecord(expectedRecord, (TxtRecord)record);
                    break;
                case Ward.Dns.Type.PTR:
                    Assert.PTRRecord(expectedRecord, (PtrRecord)record);
                    break;
                case Ward.Dns.Type.NS:
                    Assert.NSRecord(expectedRecord, (NsRecord)record);
                    break;
                case Ward.Dns.Type.OPT:
                    OPTRecord(expectedRecord, (OptRecord)record);
                    break;
            }
        }

        public static void OPTRecord(TomlTable expectedRecord, OptRecord record)
        {
            Equal(expectedRecord.Get<ushort>("udpPayload"), record.UdpPayloadSize);
            Equal(expectedRecord.Get<byte>("extendedRcode"), record.ExtendedRcode);
            Equal(expectedRecord.Get<byte>("version"), record.Edns0Version);
            Equal(expectedRecord.Get<bool>("dnsSecOK"), record.DnsSecOk);

            if (expectedRecord.ContainsKey("optionalData")) {
                var optionalData = expectedRecord.Get<TomlTableArray>("optionalData");
                Equal(optionalData.Items.Count, record.OptionalData.Count());
                var recordData = (IList<(OptRecord.OptionCode optionCode, ReadOnlyMemory<byte> optionalData)>)record.OptionalData;
                recordData.ForEach((val, idx) => {
                    var expectedData = optionalData.Items[idx];
                    Equal(expectedData.Get<OptRecord.OptionCode>("optionCode"), val.optionCode);
                    Equal(expectedData.Get<int>("dataLength"), val.optionalData.Length);
                    Equal(
                        expectedData.Get<string>("data"),
                        val.optionalData.ToArray().Aggregate(string.Empty, (s, v) => {
                            return s += v.ToString("X2").ToLower();
                        })
                    );
                });
            }
        }

        public static void NSRecord(TomlTable expectedRecord, NsRecord record)
        {
            Equal(expectedRecord.Get<string>("hostname"), record.Hostname);
        }

        public static void PTRRecord(TomlTable expectedRecord, PtrRecord record)
        {
            Equal(expectedRecord.Get<string>("hostname"), record.Hostname);
        }

        public static void TXTRecord(TomlTable expectedRecord, TxtRecord record)
        {
            Equal(expectedRecord.Get<string>("value"), record.TextData);
        }

        public static void CNAMERecord(TomlTable expectedRecord, CnameRecord record)
        {
            Equal(expectedRecord.Get<string>("hostname"), record.Hostname);
        }

        public static void CAARecord(TomlTable expectedRecord, CaaRecord record)
        {
            Equal(expectedRecord.Get<bool>("critical"), record.Critical);
            Equal(expectedRecord.Get<string>("tag"), record.Tag);
            Equal(expectedRecord.Get<string>("value"), record.Value);
        }

        public static void SOARecord(TomlTable expectedRecord, SoaRecord record)
        {
            Equal(expectedRecord.Get<string>("primary"), record.PrimaryNameServer);
            Equal(expectedRecord.Get<string>("responsible"), record.ResponsibleName);
            Equal(expectedRecord.Get<uint>("serial"), record.Serial);
            Equal(expectedRecord.Get<int>("refresh"), record.Refresh);
            Equal(expectedRecord.Get<int>("retry"), record.Retry);
            Equal(expectedRecord.Get<int>("expire"), record.Expire);
            Equal(expectedRecord.Get<uint>("minimum"), record.MinimumTtl);
        }

        public static void MXRecord(TomlTable expectedRecord, MailExchangerRecord record)
        {
            Equal(expectedRecord.Get<int>("preference"), record.Preference);
            Equal(expectedRecord.Get<string>("hostname"), record.Hostname);
        }

        public static void ARecord(TomlTable expectedRecord, AddressRecord record)
        {
            Equal(expectedRecord.Get<string>("address"), record.Address.ToString());
        }

        public static void HeaderFlags(TomlTable expectedHeader, Header.HeaderFlags flags)
        {
            Equal(expectedHeader.Get<bool>("query"), flags.Query);
            Equal(expectedHeader.Get<bool>("authoritative"), flags.Authoritative);
            Equal(expectedHeader.Get<bool>("truncated"), flags.Truncated);
            Equal(expectedHeader.Get<bool>("recurse"), flags.Recurse);
            Equal(expectedHeader.Get<bool>("recursionAvailable"), flags.RecursionAvailable);
            Equal(expectedHeader.Get<bool>("z"), flags.Z);
            Equal(expectedHeader.Get<bool>("authenticated"), flags.Authenticated);
            Equal(expectedHeader.Get<bool>("checkingDisabled"), flags.CheckingDisabled);
        }

        public static void Header(TomlTable expectedHeader, Header header)
        {
            Equal(expectedHeader.Get<ushort>("id"), header.Id);
            Equal(expectedHeader.Get<Opcode>("opcode"), header.Opcode);
            Equal(expectedHeader.Get<ReturnCode>("returnCode"), header.ReturnCode);
            Assert.HeaderFlags(expectedHeader, header.Flags);
            Equal(expectedHeader.Get<int>("questions"), header.TotalQuestions);
            Equal(expectedHeader.Get<int>("answers"), header.TotalAnswerRecords);
            Equal(expectedHeader.Get<int>("authority"), header.TotalAuthorityRecords);
            Equal(expectedHeader.Get<int>("additional"), header.TotalAdditionalRecords);
        }
    }
}
