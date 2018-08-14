using CommandLine;
using System;
using System.Collections.Generic;

namespace RedisClient
{
    class Program
    {
        static int SplitNumber(int total, int clients, int index)
        {
            var i = total % clients;
            var rtn = total / clients;
            if (index < i)
            {
                rtn++;
            }
            return rtn;
        }

        static void EvaluateRedisBench(ArgsOption argsOption)
        {
            var counter = new Counter();
            var redisBenchList = new List<RedisBench>(argsOption.ConnectionCount);
            for (var i = 0; i < argsOption.ConnectionCount; i++)
            {
                redisBenchList.Add(new RedisBench(
                    argsOption.ConnectionString,
                    SplitNumber(argsOption.ChannelCount, argsOption.ConnectionCount, i),
                    argsOption.SendSize,
                    counter));
            }
            for (var i = 0; i < argsOption.ConnectionCount; i++)
            {
                redisBenchList[i].StartBench();
            }
            Console.WriteLine("Press any key to stop...");
            Console.ReadLine();
            for (var i = 0; i < argsOption.ConnectionCount; i++)
            {
                redisBenchList[i].StopBench();
                redisBenchList[i].Dispose();
            }
        }

        static void Main(string[] args)
        {
            bool isInputValid = true;
            var argsOption = new ArgsOption();
            var result = Parser.Default.ParseArguments<ArgsOption>(args)
                .WithParsed(options => argsOption = options)
                .WithNotParsed(error => {
                    isInputValid = false;
                    Console.WriteLine($"Input is invalid for {error}");
                });
            if (!isInputValid)
            {
                return;
            }
            EvaluateRedisBench(argsOption);
        }
    }
}
