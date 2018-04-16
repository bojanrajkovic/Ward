using System;
using System.IO;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Diagnosers;
using Ward.Dns;
using Ward.Tests.Core;

namespace Ward.Benchmarks
{
    [Config(typeof(Config))]
    [RunInCI]
    public class MessageParseBenchmark
    {
        byte[] messageData;
        Stream messageStream;

        [GlobalSetup]
        public void Setup()
        {
            var message = TestCaseLoader.LoadTestCase("example.com-a-response-from-8.8.8.8");
            messageData = Convert.FromBase64String(message.Get<string>("data"));
        }

        [Benchmark]
        public int ParseMessage()
        {
            var message = Message.ParseFromBytes(messageData, 0);
            return message.Header.TotalQuestions;
        }
    }
}
