using ProtoBuf;
using StackExchange.Redis;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace RedisClient
{
    public class RedisBench : IDisposable
    {
        private ConnectionMultiplexer _redis;
        private bool _redisDisposed;
        private List<string> _channels;
        private Random _rnd;
        private Timer _timer;
        private bool _start;
        private string _content;
        private TimeSpan _interval = TimeSpan.FromMilliseconds(1000);
        private Counter _counter;
        private ArrayPool<byte> _arrayPool;

        private ISubscriber PubSub { get; }

        public RedisBench(
            string connectString,
            int channelCount,
            int singleMsgSize,
            Counter counter)
        {
            _arrayPool = ArrayPool<byte>.Shared;
            try
            {
                var configuration = ConfigurationOptions.Parse(connectString);
                configuration.CertificateValidation +=
                    delegate (object s, X509Certificate certificate,
                        X509Chain chain, SslPolicyErrors sslPolicyErrors)
                { return true; };
                _redis = ConnectionMultiplexer.Connect(configuration);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message}");
                return;
            }
            PubSub = _redis.GetSubscriber();
            TestPubSub();
            Init(channelCount, singleMsgSize, counter);
        }

        private void TestPubSub()
        {
            PubSub.Subscribe("_test", (c, data) =>
            {
                Console.WriteLine($"Received: {data}");
            });
            PubSub.Publish("_test", "hello");
        }

        public void StartBench()
        {
            _start = true;
            _counter.StartPrint();
        }

        public void StopBench()
        {
            _start = false;
        }

        private void Init(int channelCount, int singleMsgSize, Counter counter)
        {
            _start = false;
            _rnd = new Random();
            byte[] content = new byte[singleMsgSize];
            _rnd.NextBytes(content);
            _content = Encoding.UTF8.GetString(content);

            _channels = new List<string>(channelCount);
            for (var i = 0; i < channelCount; i++)
            {
                _channels.Add($"chan_{i}");
            }

            SubChannels();
            _timer = new Timer(Publishing, this, _interval, _interval);
            if (counter == null)
            {
                counter = new Counter();
            }
            _counter = counter;
        }

        private void SubChannels()
        {
            for (var i = 0; i < _channels.Count; i++)
            {
                PubSub.SubscribeAsync(_channels[i], (c, data) =>
                {
                    using (var memoryStream = new MemoryStream(data))
                    {
                        var message = Serializer.Deserialize<MessageData>(memoryStream);
                        var latency = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - message.Timestamp;
                        // do something with the message
                        _counter.Latency(latency);
                        _counter.RecordRecvSize(message.Content.Length);
                    }
                });
            }
        }

        private void PubChannels()
        {
            if (_start)
            {
                for (var i = 0; i < _channels.Count; i++)
                {
                    var data = new MessageData
                    {
                        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        Content = _content
                    };
                    var buffer = _arrayPool.Rent(8192); // TODO. hardcode a mimimum length, it should be configurable
                    try
                    {
                        using (var memoryStream = new MemoryStream(buffer))
                        {
                            // serialize a ChatMessage using protobuf-net
                            Serializer.Serialize(memoryStream, data);

                            // publish the message to the channel

                            var sentData = new byte[memoryStream.Position];
                            Array.Copy(buffer, sentData, memoryStream.Position);
                            try
                            {
                                PubSub.PublishAsync(_channels[i], sentData);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"{e.Message}");
                            }

                            _counter.RecordSentSize(sentData.Length);
                        }
                    }
                    finally
                    {
                        _arrayPool.Return(buffer);
                    }
                }
            }
        }

        private void Publishing(object state)
        {
            RedisBench rb = (RedisBench)state;
            rb.PubChannels();
        }

        public void Dispose()
        {
            if (!_redisDisposed)
            {
                PubSub.UnsubscribeAll();
                _redis.Dispose();
                _redisDisposed = true;
            }
        }
    }
}
