using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;

namespace RedisClient
{
    public class Counter
    {
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);
        private readonly long Step = 100;    // latency unit
        private readonly long Length = 10;    // how many latency categories will be displayed

        private long[] _latency;
        private long _totalReceived;
        //private long _lastReceived;
        //private long _recvRate;

        private long _totalRecvSize;
        //private long _lastRecvSize;
        //private long _recvSizeRate;

        private long _totalSent;
        //private long _lastSent;
        //private long _sentRate;

        private long _totalSentSize;
        //private long _lastSentSize;
        //private long _sentSizeRate;

        private Timer _timer;
        private long _startPrint;
        private bool _hasRecord;

        private object _lock = new object();

        public Counter()
        {
            _latency = new long[Length];
        }

        public void Latency(long dur)
        {
            long index = dur / Step;
            if (index >= Length)
            {
                index = Length - 1;
            }
            Interlocked.Increment(ref _latency[index]);
            _hasRecord = true;
        }

        public void RecordSentSize(long sentSize)
        {
            Interlocked.Increment(ref _totalSent);
            Interlocked.Add(ref _totalSentSize, sentSize);
        }

        public void RecordRecvSize(long recvSize)
        {
            Interlocked.Increment(ref _totalReceived);
            Interlocked.Add(ref _totalRecvSize, recvSize);
        }

        public void StartPrint()
        {
            if (Interlocked.CompareExchange(ref _startPrint, 1, 0) == 0)
            {
                _timer = new Timer(Report, state: this, dueTime: Interval, period: Interval);
            }
        }

        private void Report(object state)
        {
            if (_hasRecord)
            {
                ((Counter)state).InternalReport();
                _hasRecord = false;
            }
        }

        private void InternalReport()
        {
            /*
            lock (_lock)
            {
                var totalReceivedBytes = Interlocked.Read(ref _totalRecvSize);
                var lastReceivedBytes = Interlocked.Read(ref _lastRecvSize);
                Interlocked.Exchange(ref _recvSizeRate, totalReceivedBytes - lastReceivedBytes);
                _lastRecvSize = totalReceivedBytes;

                var totalReceived = Interlocked.Read(ref _totalReceived);
                var lastReceived = Interlocked.Read(ref _lastReceived);
                Interlocked.Exchange(ref _recvRate, totalReceived - lastReceived);
                _lastReceived = totalReceived;

                var totalSent = Interlocked.Read(ref _totalSent);
                var lastSent = Interlocked.Read(ref _lastSent);
                Interlocked.Exchange(ref _sentRate, totalSent - lastSent);
                _lastSent = totalSent;

                var totalSentSize = Interlocked.Read(ref _totalSentSize);
                var lastSentSize = Interlocked.Read(ref _lastSentSize);
                Interlocked.Exchange(ref _sentSizeRate, totalSentSize - lastSentSize);
                _lastSentSize = totalSentSize;
            }
            */
            var dic = new ConcurrentDictionary<string, long>();
            var batchMessageDic = new ConcurrentDictionary<string, long>();
            StringBuilder sb = new StringBuilder();
            for (var i = 0; i < Length; i++)
            {
                sb.Clear();
                var label = Step + i * Step;
                if (i < Length - 1)
                {
                    sb.Append("message:lt:");
                }
                else
                {
                    sb.Append("message:ge:");
                }
                sb.Append(Convert.ToString(label));
                dic[sb.ToString()] = _latency[i];
            }
            dic["message:sent"] = Interlocked.Read(ref _totalSent);
            dic["message:received"] = Interlocked.Read(ref _totalReceived);
            dic["message:sendSize"] = Interlocked.Read(ref _totalSentSize);
            dic["message:recvSize"] = Interlocked.Read(ref _totalRecvSize);
            // dump out all statistics
            Console.WriteLine(JsonConvert.SerializeObject(new
            {
                Time = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddThh:mm:ssZ"),
                Counters = dic
            }));
        }
    }
}
