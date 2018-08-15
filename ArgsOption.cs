using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace RedisClient
{
    class ArgsOption
    {
        [Option("connectionString", Required = true, HelpText = "Specify connection string, i.e. ")]
        public string ConnectionString { get; set; }

        [Option("sendSize", Required = false, Default = 1024, HelpText = "Specify the message size")]
        public int SendSize { get; set; }

        [Option("channelCount", Required = false, Default = 1000, HelpText = "Specify the channel count")]
        public int ChannelCount { get; set; }

        [Option("connectionCount", Required = false, Default = 1, HelpText = "Specify the Redis connection count")]
        public int ConnectionCount { get; set; }
    }
}
