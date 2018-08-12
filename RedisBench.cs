﻿using ProtoBuf;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
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

        private ISubscriber PubSub { get; }

        public RedisBench(string connectString, int channelCount, int singleMsgSize)
        {
            _redis = ConnectionMultiplexer.Connect(connectString);
            PubSub = _redis.GetSubscriber();
            Init(channelCount, singleMsgSize);
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

        private void Init(int channelCount, int singleMsgSize)
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
            _counter = new Counter();
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
                    using (var memoryStream = new MemoryStream())
                    {
                        // serialize a ChatMessage using protobuf-net
                        Serializer.Serialize(memoryStream, data);

                        // publish the message to the channel
                        var sentData = memoryStream.ToArray();
                        _ = PubSub.PublishAsync(_channels[i], sentData);
                        _counter.RecordSentSize(sentData.Length);
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
