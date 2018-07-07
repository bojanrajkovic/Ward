using BenchmarkDotNet.Attributes;
using Ward.Dns;
using Ward.Tests.Core;

namespace Ward.Benchmarks
{
    [Config(typeof(Config))]
    [RunInCI]
    public class MessageParseBenchmark
    {
        byte[] messageData;

        [GlobalSetup]
        public void Setup()
        {
            var message = TestCaseLoader.LoadMessageTestCase("example.com-a-response-from-8.8.8.8");
            messageData = message.MessageData;
        }

        [Benchmark]
        public int ParseMessage()
        {
            var message = MessageParser.ParseMessage(messageData);
            return message.Header.TotalQuestions;
        }
    }
}
